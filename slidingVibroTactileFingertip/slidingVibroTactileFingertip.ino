/****************************************
 * Touching and feeling the immersive world
 * 
 * Author: Uriel Martinez-Hernandez
 * Filename: slidingVibroTactileFingertip.ino
 * Date: 20/11/2020
 * 
 * Description
 * ======================
 * Program that controls the cylinder (sliding modality) and vibration motor.
 * The direction and speed of sliding is controlled. The speed of the vibration
 * motor is controlled. This programs communicates to Unity (Table_surface_v1.unity), which
 * contains an scenario with 4 objects that simulate 4 different textures
 * (wood (soft), wood (hard), carpet, bricks). There is a cylinder that moves over
 * these textures and send the signal to Arduino to set the speed and direction of the
 * cylinder and speed of the vibration motor.
 * 
 * Limitation
 * ====================== 
 * - The haptic feedback (sliding and vibration) is in 1DoF only.
 * - Fixed speed and direction signals received from Unity.
 * - The simulated fingertip in Unity doesn't follow the movement of the human fingertip.
 * - Lack of tracking of the fingertip.
 * - The cylinder is made of one material (PLA).
 * 
 * Haptic modalities
 * ======================
 * - Sliding
 * - Vibration
 * - Touch
 ****************************************/


#include <Servo.h>

Servo servoSliding;
Servo servoPressure;

const int outputPinSliding = 6;      // servo output pin for sliding motor
const int outputPinPressure = 4;      // servo output pin for touch motor
const int outputPinVibration = 5; // vibration output pin

String inputString = "";
String commandString = "";
int commandSpeed = 0;
boolean stringComplete = false;  // whether the string is complete
float servoSlidingSpeed = 89;
float servoSlidingStop = 89;
float speedChange;
float biasRigth = 1;
float biasLeft = 4;
float CalibrationPressurePosition = 0;

const int noTouchPosition = 100;
const int touchPosition = 70;
int touchReady = false;

void setup() {
  // put your setup code here, to run once:

  Serial.begin(115200);

  servoSliding.attach(outputPinSliding);
  servoSliding.write(servoSlidingStop);

  servoPressure.attach(outputPinPressure);
  servoPressure.write(noTouchPosition);

  pinMode(outputPinVibration, OUTPUT);

  speedChange = 0;
}

void loop() {
  // put your main code here, to run repeatedly:
  while(Serial.available())
  {
    char inChar = (char)Serial.read();
    inputString += inChar;

    if( inChar == '\n' )
      stringComplete = true;
  }

  if( stringComplete )
  {
    getCommand();
    getSpeed();

    if(commandString.equals("R"))
    {
        servoSlidingSpeed = servoSlidingStop + commandSpeed + biasRigth; //speedChange;
        servoSliding.write(servoSlidingSpeed);      
    }
    else if(commandString.equals("L"))
    {
        servoSlidingSpeed = servoSlidingStop - commandSpeed - biasLeft; //speedChange;
        servoSliding.write(servoSlidingSpeed);
    }
    else if(commandString.equals("T"))
    {
      if( touchReady == false )
      {
        servoPressure.write(touchPosition);
        touchReady = true;
      }
    }
    else if(commandString.equals("S"))
    {
      servoSliding.write(servoSlidingStop);
      servoPressure.write(noTouchPosition);
      analogWrite(outputPinVibration, 0);

      touchReady = false;
    }
    else if(commandString.equals("C"))
    {
      servoSliding.write(servoSlidingStop);
      servoPressure.write(CalibrationPressurePosition);
      analogWrite(outputPinVibration, 0);
    }
  
    inputString = "";
    stringComplete = false;
  }      
}

void getCommand()
{
  if(inputString.length()>0)
  {
     commandString = inputString.substring(0,1);
  }
}

void getSpeed()
{
  if(inputString.length()>0)
  {
     commandSpeed = inputString.substring(1,2).toInt();
  }
}
