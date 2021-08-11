using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleFX : MonoBehaviour
{
    Vehicle vehicle;

    [Header("Wheels")]
    public Transform Wheel_FL;
    public Transform Wheel_FR;
    public Transform Wheel_BL;
    public Transform Wheel_BR;

    [Header("Trails")]
    public TrailRenderer Trail_FL;
    public TrailRenderer Trail_FR;
    public TrailRenderer Trail_BL;
    public TrailRenderer Trail_BR;

    [Header("Particles")]
    public ParticleSystem DirtFL;
    public ParticleSystem DirtFR;
    public ParticleSystem DirtBL;
    public ParticleSystem DirtBR;

    [Space()]
    public ParticleSystem RockFL;
    public ParticleSystem RockFR;
    public ParticleSystem RockBL;
    public ParticleSystem RockBR;

    [Space()]
    public ParticleSystem SmokeFL;
    public ParticleSystem SmokeFR;
    public ParticleSystem SmokeBL;
    public ParticleSystem SmokeBR;

    [Header("Properties")]
    public Color DirtColor;
    public float maxSteer;
    public float trailMin;
    public bool SmokeFX;

    float wheelSpin = 0;

    // Start is called before the first frame update
    void Start()
    {
        vehicle = GetComponent<Vehicle>();
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Create Input Manager System
        var gamepad = Gamepad.current;
        if (gamepad == null)
            return;

        // Wheel Spin Calculation
        wheelSpin += 1 * vehicle.getSpeed();

        //Steer Rotation Calculation
        float steerRotation = vehicle.steering_angle * maxSteer * ((gamepad.squareButton.ReadValue() > 0) ? 0.5f : 1f);
        steerRotation *= (vehicle.gas_input < 0) ? -1 : 1;

        //Front Wheel Rotation
        Wheel_FL.localRotation = Quaternion.Euler(wheelSpin, steerRotation, 0);
        Wheel_FR.localRotation = Quaternion.Euler(wheelSpin, steerRotation, 0);

        //Back Wheel Rotation
        Wheel_BL.localRotation = Quaternion.Euler(wheelSpin, 0, 0);
        Wheel_BR.localRotation = Quaternion.Euler(wheelSpin, 0, 0);

        //Check for wheel collisions
        RaycastHit hit;
        bool hit_fl = Physics.Raycast(transform.position + (-transform.right * 0.5f) + (transform.forward * 2), Vector3.down, out hit, 0.5f);
        bool hit_fr = Physics.Raycast(transform.position + (transform.right * 0.5f) + (transform.forward * 2), Vector3.down, out hit, 0.5f);
        bool hit_bl = Physics.Raycast(transform.position + (-transform.right * 0.5f) + (transform.forward * -2), Vector3.down, out hit, 0.5f);
        bool hit_br = Physics.Raycast(transform.position + (transform.right * 0.5f) + (transform.forward * -2), Vector3.down, out hit, 0.5f);

        //Wheel Trails
        Trail_FL.emitting = (gamepad.squareButton.ReadValue() > 0 || vehicle.GetSlip() < 0.6) && hit_fl;
        Trail_FR.emitting = (gamepad.squareButton.ReadValue() > 0 || vehicle.GetSlip() < 0.6) && hit_fr;
        Trail_BL.emitting = (gamepad.squareButton.ReadValue() > 0 || vehicle.GetSlip() < 0.6) && hit_bl;
        Trail_BR.emitting = (gamepad.squareButton.ReadValue() > 0 || vehicle.GetSlip() < 0.6) && hit_br;

        if (!SmokeFX)
        {
            //SmokeFX
            SmokeFL.enableEmission = false;
            SmokeFR.enableEmission = false;
            SmokeBL.enableEmission = false;
            SmokeBR.enableEmission = false;

            //FL
            DirtFL.enableEmission = false;
            RockFL.enableEmission = false;
                                    
            //FR                    
            DirtFR.enableEmission = false;
            RockFR.enableEmission = false;
                                    
            //BL                    
            DirtBL.enableEmission = false;
            RockBL.enableEmission = false;
                                    
            //BL                    
            DirtBR.enableEmission = false;
            RockBR.enableEmission = false;

            return;
        }

        //Wheel Trails
        SmokeFL.enableEmission = (gamepad.squareButton.ReadValue() > 0 || vehicle.GetSlip() < 0.3) && hit_fl;
        SmokeFR.enableEmission = (gamepad.squareButton.ReadValue() > 0 || vehicle.GetSlip() < 0.3) && hit_fr;
        SmokeBL.enableEmission = (gamepad.squareButton.ReadValue() > 0 || vehicle.GetSlip() < 0.3) && hit_bl;
        SmokeBR.enableEmission = (gamepad.squareButton.ReadValue() > 0 || vehicle.GetSlip() < 0.3) && hit_br;

        //Check for wheel collisions
        RaycastHit hit2;
        float speedLimit = 3;

        //FL
        if(Physics.Raycast(transform.position + (-transform.right * 0.5f) + (transform.forward*2), Vector3.down, out hit2, 0.5f))
        {
            DirtFL.enableEmission = (hit2.transform.tag == "Dirt" && vehicle.rigidbody.velocity.magnitude > speedLimit);
            RockFL.enableEmission = (hit2.transform.tag == "Dirt" && vehicle.rigidbody.velocity.magnitude > speedLimit);
        }

        //FR
        if (Physics.Raycast(transform.position + (transform.right * 0.5f) + (transform.forward * 2), Vector3.down, out hit2, 0.5f))
        {
            DirtFR.enableEmission = (hit2.transform.tag == "Dirt" && vehicle.rigidbody.velocity.magnitude > speedLimit);
            RockFR.enableEmission = (hit2.transform.tag == "Dirt" && vehicle.rigidbody.velocity.magnitude > speedLimit);
        }

        //BL
        if (Physics.Raycast(transform.position + (-transform.right * 0.5f) + (transform.forward * -2), Vector3.down, out hit2, 0.5f))
        {
            DirtBL.enableEmission = (hit2.transform.tag == "Dirt" && vehicle.rigidbody.velocity.magnitude > speedLimit);
            RockBL.enableEmission = (hit2.transform.tag == "Dirt" && vehicle.rigidbody.velocity.magnitude > speedLimit);
        }

        //BL
        if (Physics.Raycast(transform.position + (transform.right * 0.5f) + (transform.forward * -2), Vector3.down, out hit2, 0.5f))
        {
            DirtBR.enableEmission = (hit2.transform.tag == "Dirt" && vehicle.rigidbody.velocity.magnitude > speedLimit);
            RockBR.enableEmission = (hit2.transform.tag == "Dirt" && vehicle.rigidbody.velocity.magnitude > speedLimit);
        }
    }
}
