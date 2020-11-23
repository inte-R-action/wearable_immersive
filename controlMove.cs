/*
 * Touching and feeling the immersive world (table_surfaces_scene_6)
 * 
 * Author: Uriel Martinez-Hernandez
 * Date: 20/11/2020
 * 
 * Description: Tracks human hands and fingers and the information from the position and velocity
 *              of the hand and fingers is sent to a controller in Arduino to control the wearable
 *              fingertip device to provide sliding, touch and vibration stimuli.
 * 
 */
//////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using System.IO.Ports;



public class controlMove : MonoBehaviour
{
    Controller controller;
    Frame frame;

    public int materialValue = 0;   // 0 = no rought (smooth), 1 = slightly rough, 2 = mid-rough, 3 = rought

    public string comPort = "COM3";
    public int baudRate = 9600;

    private Vector palmNormal;
    private Vector palmPosition;
    private Vector fingertipPosition;
    private Vector palmVelocity;
    private Vector previousPalmPosition;
    private bool firstRun;
    private int motorSpeed;

    private Vector[] listOfObjectsInit;
    private Vector[] listOfObjectsEnd;

    SerialPort sp;

    // Start is called before the first frame update
    void Start()
    {
        sp = new SerialPort(comPort, baudRate);

        sp.Open();
        sp.ReadTimeout = 1;

        controller = new Controller();

        firstRun = true;
        motorSpeed = 0;

        listOfObjectsInit = new Vector[1];
        listOfObjectsEnd = new Vector[1];

        listOfObjectsInit[0].x = -160;
        listOfObjectsInit[0].y = 160;
        listOfObjectsInit[0].z = -60;

        listOfObjectsEnd[0].x = 150;
        listOfObjectsEnd[0].y = 160;
        listOfObjectsEnd[0].z = 121;

    }

    // Update is called once per frame
    void Update()
    {
        frame = controller.Frame();
//        print("Hands number: " + frame.Hands.Count);
        if (frame.Hands.Count > 0)
        {
            List<Hand> hands = frame.Hands;
            for (int id = 0; id < hands.Count; id++)
            {
                if (hands[id].IsRight == true)
                {
                    Hand firstHand = hands[id];

                    List<Finger> fingers = hands[id].Fingers;
                    Finger digit = fingers[1];
                    Vector indexFingerPosition = fingers[1].TipPosition;
                    palmPosition = firstHand.PalmPosition;
                    fingertipPosition = indexFingerPosition;

                    palmVelocity = firstHand.PalmVelocity;

                    // check if hand is in the region of any of the objects, otherwise set motorSpeed to 0
                    if (isHandTouchObjectRegion(fingertipPosition) == true && isHandInObjectRegion(fingertipPosition) == true)
                    {
                        sp.WriteLine(string.Concat("T", System.Convert.ToString(0)) + "\n");
                        print("TOUCH - SPEED " + System.Convert.ToString(0));

                        if (System.Math.Abs(palmVelocity.x) >= 0 && System.Math.Abs(palmVelocity.x) <= 20)
                        {
                            motorSpeed = 0;
                        }
                        else
                        {
                            motorSpeed = (int)((System.Math.Abs(palmVelocity.x) * 10) / 200);
                        }

                        if (motorSpeed > 0)
                        {
                            if (motorSpeed > 9)
                                motorSpeed = 9;

                            if (palmVelocity.x > 0)
                            {
                                print("MOVES TO RIGTH AT SPEED " + System.Convert.ToString(motorSpeed));
                                sp.WriteLine(string.Concat("R", System.Convert.ToString(motorSpeed), System.Convert.ToString(materialValue)) + "\n");
                            }
                            else if(palmVelocity.x < 0)
                            {
                                print("MOVES TO LEFT AT SPEED " + System.Convert.ToString(motorSpeed));
                                sp.WriteLine(string.Concat("L", System.Convert.ToString(motorSpeed), System.Convert.ToString(materialValue)) + "\n");
                            }
                            else
                            {
                                print("ZERO MOVE " + System.Convert.ToString(motorSpeed));
                                sp.WriteLine(string.Concat("T", System.Convert.ToString(motorSpeed), System.Convert.ToString(materialValue)) + "\n");
                            }
                        }

                    }
                    else
                    {
                        print("STOPPED - SPEED " + System.Convert.ToString(0));
                        sp.WriteLine(string.Concat("S", System.Convert.ToString(0), System.Convert.ToString(0)) + "\n");

                    }
                }
            }
        }
        else
        {
            print("NO_MOVE");
            sp.WriteLine(string.Concat("S", System.Convert.ToString(0), System.Convert.ToString(0)) + "\n");
        }


    }

    bool isHandInObjectRegion(Vector currentPosition)
    {
          for (int i = 0; i < 1; i++)
            {
                if ( currentPosition.x >= listOfObjectsInit[i].x && currentPosition.x <= listOfObjectsEnd[i].x)
            {
                if (currentPosition.z >= listOfObjectsInit[i].z && currentPosition.z <= listOfObjectsEnd[i].z)
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool isHandTouchObjectRegion(Vector currentPosition)
    {
          for (int i = 0; i < 1; i++)
            {
                if ( currentPosition.y <= listOfObjectsInit[i].y)
            {
                return true;
            }
        }
        return false;
    }
}