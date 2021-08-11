using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInput : MonoBehaviour
{
    public Vehicle vehicle;
    public int user;

    private void Awake()
    {
        vehicle = GetComponent<Vehicle>();
    }

    private void FixedUpdate()
    {
        //Load input from Manager
        UserInput gamepad = InputManager.GetUser(user);

        //Assign Inputs to values
        Vector2 steer = gamepad.leftStick;
        float gas = gamepad.rightTrigger;
        float brake = gamepad.leftTrigger;

        //Vehicle Throttle
        vehicle.Gas(Mathf.Clamp(gas - (brake * 2), -1.5f, 1f));
        
        //Vehicle Steering     
        if (vehicle.isOnGround()) vehicle.Steer(steer.x);
    }
}
