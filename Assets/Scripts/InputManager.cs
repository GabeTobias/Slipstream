using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public struct UserInput
{
    public Vector2 leftStick;
    public Vector2 rightStick;

    public float leftTrigger;
    public float rightTrigger;

    public bool leftBumper, rightBumper;

    public bool btnA, btnB, btnC, btnD;
    public bool dLeft, dRight, dUp, dDown;
}

public class InputManager
{
    public static UserInput GetUser(int i) 
    {
        //Get Controller inputs from gamepad
        Gamepad controller = Gamepad.all[i];
        UserInput input = new UserInput();

        //Set stick Inputs
        input.leftStick = controller.leftStick.ReadValue();
        input.rightStick = controller.rightStick.ReadValue();

        //Set trigger Inputs
        input.leftTrigger = controller.leftTrigger.ReadValue();
        input.rightTrigger = controller.rightTrigger.ReadValue();

        //Set bumper Inputs
        input.leftBumper = controller.leftShoulder.ReadValue() > 0.1;
        input.rightBumper = controller.rightShoulder.ReadValue() > 0.1;

        //Set button Inputs
        input.btnA = controller.crossButton.ReadValue() > 0.1 || controller.aButton.ReadValue() > 0.1;
        input.btnB = controller.squareButton.ReadValue() > 0.1 || controller.xButton.ReadValue() > 0.1;
        input.btnC = controller.triangleButton.ReadValue() > 0.1 || controller.yButton.ReadValue() > 0.1;
        input.btnD = controller.circleButton.ReadValue() > 0.1 || controller.bButton.ReadValue() > 0.1;

        //Set dpad Inputs
        input.dDown = controller.buttonSouth.ReadValue() > 0.1;
        input.dLeft = controller.buttonWest.ReadValue() > 0.1;
        input.dUp = controller.buttonNorth.ReadValue() > 0.1;
        input.dRight = controller.buttonEast.ReadValue() > 0.1;

        return input;
    }
}
