using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCamera : MonoBehaviour
{
    [Header("References")]
    public Camera camera;
    public Vehicle vehicle;
    
    private Rigidbody rigidbody;

    [Header("Properties")]
    public float viewDistance;
    public float viewHeight;
    public float swivelSpeed;

    // Start is called before the first frame update
    void Start()
    {
        //Get Components
        rigidbody = GetComponent<Rigidbody>();
        vehicle = GetComponent<Vehicle>();

        //Set Anitaliasing
        QualitySettings.antiAliasing = 8;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Load input from Manager
        UserInput gamepad = InputManager.GetUser(0);

        //Set Position & Rotation
        camera.transform.position = Vector3.Lerp(camera.transform.position, getPosition(), (gamepad.btnC) ? 1:swivelSpeed);
        camera.transform.LookAt(transform.position);
    }

    Vector3 getPosition() 
    {
        //TODO: Get User from input
        //Load input from Manager
        UserInput gamepad = InputManager.GetUser(0);

        //Set the camera positions based on vehicle velocity
        Vector3 viewForward = rigidbody.position - (vehicle.new_velocity.normalized * viewDistance) + new Vector3(0,viewHeight,0);
        Vector3 viewBack = rigidbody.position + (vehicle.new_velocity.normalized * viewDistance) + new Vector3(0,viewHeight,0);
        Vector3 viewRight = rigidbody.position + (transform.right * viewDistance);

        //Set position if moving slowly to avoiding shaking camera
        if (vehicle.new_velocity.magnitude < 0.1) return rigidbody.position - (transform.forward * viewDistance) + new Vector3(0, viewHeight, 0);

        //Input to change between the different views
        //if (Input.GetKey(KeyCode.LeftAlt)) return viewRight;
        if (gamepad.btnC) return viewBack;

        return viewForward;
    }
}
