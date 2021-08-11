using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Vehicle : MonoBehaviour
{
    [Header("Vehicle")]
    public float acceleration;
    public float max_speed;
    public float max_steer;

    [Header("Vehicle Friction")]
    public float friction_factor_forward;
    public float friction_factor_lat;
    
    [Header("Vehicle Traction")]
    public bool useTraction;
    public float slipExtreme;
    public AnimationCurve slipCurve;

    [Header("Vehicle Inputs")]
    public float sensitivity;

    [Header("Drift")]
    public float driftSteer;
    public float driftLateralFriction;
    public float driftForwardFriction;

    [Header("DEBUG")]
    public bool inDraft;
    public float steering_angle;
    public float gas_input;

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    private float current_lat_friction;
    private float current_forward_friction;
    private float current_steer;

    /////////////////////////////////////////////////////////////////////////////////////////////////////////

    [HideInInspector]
    public Rigidbody rigidbody;

    [HideInInspector]
    public Vector3 new_velocity;

    /////////////////////////////////////////////////////////////////////////////////////////////////////////

    float driftLength = 0.0025f;


    // Start is called before the first frame update
    void Awake()
    {
        //Get the rigidbody
        rigidbody = GetComponent<Rigidbody>();

        //Set Default state values to avoid slow lerp at start of race
        current_steer = max_steer;
        current_lat_friction = friction_factor_lat;
        current_forward_friction = friction_factor_forward;
    }

    void Update()
    {
        //TODO: Create Input Manager System
        var gamepad = Gamepad.current;
        if (gamepad == null)
            return;

        //Check for square button
        if (gamepad.squareButton.ReadValue() > 0)
            SetState(driftLateralFriction,driftForwardFriction,driftSteer);
        else
            SetState(friction_factor_lat, friction_factor_forward, max_steer);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Lerp Rotations to keep car upright
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0,transform.rotation.eulerAngles.y,0), 0.2f);

        //Determine next Vehicle ratation from steering
        Vector3 new_forward = Vector3.Lerp(transform.forward, GetSteerDirection(), 0.1f);
        
        //Rotate vehicle
        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            Quaternion.LookRotation(new_forward, Vector3.up).eulerAngles.y,
            transform.rotation.eulerAngles.z
        );

        if (isOnGround())
        {
            //Set the new velocity based on acceleration and friction
            new_velocity = rigidbody.velocity + GetAccelerationDirection() + GetFriction();
            
            //Apply velocity to rigidbody
            rigidbody.velocity = new Vector3(new_velocity.x, rigidbody.velocity.y, new_velocity.z);
        }
    }

    Vector3 GetSteerDirection()
    {
        //Lerp from current direction to steer direction
        return Vector3.RotateTowards(transform.forward, transform.right, steering_angle, 3.14f);
    }

    Vector3 GetAccelerationDirection()
    {
        //Find Acceleration from Input & Traction
        return transform.forward * gas_input * acceleration * ((useTraction) ? GetSlip():1f);
    }

    public Vector3 GetFriction()
    {
        //Rigidbody shorthand
        Vector3 velocity_vector = rigidbody.velocity;

        //Find the sidways velocity
        Vector3 lateral_velocity = transform.right * Vector3.Dot(velocity_vector, transform.right);
        
        //Determine Sliding friction from slip & sideways velocity
        Vector3 lateral_friction = -lateral_velocity * (current_lat_friction * ((useTraction) ? GetSlip(): 1f));
        
        //Determine stoping force based on the surface below the vehicle
        Vector3 backwards_friction = -velocity_vector * (current_forward_friction * OffRoadPenalty());

        return backwards_friction + lateral_friction;
    }

    public float GetSlip() 
    {
        //Find the sideways velocity
        Vector3 d = rigidbody.velocity * Vector3.Dot(rigidbody.velocity.normalized, transform.right.normalized);
        
        //Normalize the value
        float p = (d.magnitude / slipExtreme);
        
        //Apply value to slip curve
        float r = 1f - slipCurve.Evaluate(p);

        return r;
    }

    // GAS AND STEERING GETTER & SETTER
    public void Gas(float AxisValue)    { gas_input = AxisValue; }
    public void Steer(float AxisValue)  { steering_angle = Mathf.Lerp(steering_angle, AxisValue * current_steer * TurnClamp(),  0.1f); }

    public string Speedometer() { return getSpeed().ToString(); }
    public float getSpeed()     { return rigidbody.velocity.magnitude; }


    float TurnClamp()
    {
        //I NO LONGER REMEMBER WHAT THIS DOES BUT EVERYTHING BREAKS IF I REMOVE IT IDK
        float vel = rigidbody.velocity.magnitude / 30f;
        return Mathf.Clamp(vel / 40.0f, -1.0f, 1.0f);
    }


    public bool isOnGround() 
    {
        //Check all 4 wheels for contact to the ground
        if (Physics.Raycast(transform.position + new Vector3(-1, 0,  2), Vector3.down, out _, 0.5f)) return true;
        if (Physics.Raycast(transform.position + new Vector3( 1, 0,  2), Vector3.down, out _, 0.5f)) return true;
        if (Physics.Raycast(transform.position + new Vector3(-1, 0, -2), Vector3.down, out _, 0.5f)) return true;
        if (Physics.Raycast(transform.position + new Vector3( 1, 0, -2), Vector3.down, out _, 0.5f)) return true;
        
        return false;
    }

    public float OffRoadPenalty() 
    {
        //Penalty Defaults to 1
        float penalty = 1;
        
        //Universal Raycast Object
        RaycastHit hit;

        //FL
        if (Physics.Raycast(transform.position + new Vector3(-0.5f, 0, 2), Vector3.down, out hit, 0.5f))
        {
            if (hit.transform.gameObject.layer == 9) penalty += 1.5f;
            if (hit.transform.gameObject.layer == 10) penalty += 0.5f;
        }
        //FR
        if (Physics.Raycast(transform.position + new Vector3( 0.5f, 0, 2), Vector3.down, out hit, 0.5f))
        {
            if (hit.transform.gameObject.layer == 9) penalty += 1.5f;
            if (hit.transform.gameObject.layer == 10) penalty += 0.5f;
        }
        //BL
        if (Physics.Raycast(transform.position + new Vector3(-0.5f, 0, -2), Vector3.down, out hit, 0.5f))
        {
            if (hit.transform.gameObject.layer == 9) penalty += 1.5f;
            if (hit.transform.gameObject.layer == 10) penalty += 0.5f;
        }
        //BR
        if (Physics.Raycast(transform.position + new Vector3( 0.5f, 0, -2), Vector3.down, out hit, 0.5f))
        {
            if (hit.transform.gameObject.layer == 9) penalty += 1.5f;
           if (hit.transform.gameObject.layer == 10) penalty += 0.5f;
        }

        return penalty;
    }

    public void SetState(float lateral, float forward, float steer) 
    {
        //Lerp to the drift state
        current_lat_friction = Mathf.Lerp(current_lat_friction, lateral, driftLength);
        current_forward_friction = Mathf.Lerp(current_forward_friction, forward, driftLength);
        current_steer = Mathf.Lerp(current_steer, steer, driftLength);
    }

    public void ToggleAutoDrive() 
    {
        //Remove Inputs
        Destroy(GetComponent<VehicleInput>());

        //Add AI
        VehicleAI ai = gameObject.AddComponent<VehicleAI>();

        //Toggle AI Coast Mode
        ai.AutoPilot = true;
    }


    //When the Primitive collides with the walls, it will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Draft")
        {
            //TODO: This should be relative to aerodynamics
            acceleration += 0.1f;
            inDraft = true;
        }
    }

    //When the Primitive exits the collision, it will change Color
    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Draft")
        {
            acceleration -= 0.1f;
            inDraft = false;
        }
    }

}


////////////////////////////////////////////////////////////////////////////////

/*
 *  Power           |Acceleration|
 *  Steering        |Steering Angle|
 *  Traction        |Slip Extrenum|
 *  GearSpeed       |Maximum Speed|
 *  Aerodynamics    |Forward Friction|
 *  Weight          |Rigidbody|
 */

////////////////////////////////////////////////////////////////////////////////

/*
 *  Precision Pete  |Power/Traction/GearSpeed|
 *  Drift King      |Power/Steering/GearSpeed|
 *  Drag Stripper   |Power/GearSpeed/Aerodynamics|
 *  Corner Hugger   |Aerodynamics/Steering/Traction|
 *  Demolition Exp. |Power/GearSpeed|
 */

////////////////////////////////////////////////////////////////////////////////

/*
 *  Spoilers        |Roof vs. Lip vs. Chin|
 *  Turbos          |Single vs. Sequential vs. Twin vs. Electric vs. Supercharger|
 *  Breaks          |Disc vs. Drum vs ABS| & |Hydraulic vs Electromagnetic|
 *  Size            |2Door vs. 4Door|
 *  Drive           |AWD vs. RWD vs. FWD|
 *  Coolant Type    |Natural vs. Synthetic|
 *  Engine Size     |0.2-litres|
 *  Transmission    |Manual vs. Autmatic vs. Dual Clutch vs. CVT|
 *  Gear Box        |5-10 Speed|
 *  Fuel Injection  |Natural vs. Injected|
 *  Sterring Type   |Rack & Pinion vs. Power Steering|
 */

////////////////////////////////////////////////////////////////////////////////
