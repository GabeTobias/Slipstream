using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleAI : MonoBehaviour
{
    [Header("Difficulty")]
    [Range(1,10)] public int skill;
    public RaceProfile profile;

    [Header("References")]
    public Vehicle vehicle;
    public RaceTracker tracker;
    public RacePath racePath;

    [Header("Speed")]
    public float targetSpeed = 45;
    public float throttleSpeed = 0.1f;
    public float respectDistance = 8.0f;

    [Header("State")]
    public bool AutoPilot = false;
    public bool debugSphere = false;

    [Header("DEBUG")]
    public bool showRespectRay;

    private float throttle = 0;


    // Start is called before the first frame update
    void Awake()
    {
        vehicle = GetComponent<Vehicle>();
        tracker = GetComponent<RaceTracker>();
        racePath = FindObjectOfType<RacePath>();
    }

    // Update is called once per frame
    void Update()
    {
        //Set vehicle Inputs
        vehicle.Gas(getGas());
        vehicle.Steer(getSteer());

        //Calculate target speeds from profile
        float cruise = Mathf.Max(racePath.GetTargetSpeed(tracker.time) / 2f, 35);
        float sTarget = racePath.GetTargetSpeed(tracker.time, getBellAcceleration(), getBreakingDistance());

        //Adjust Target Speed based on race path
        targetSpeed = (AutoPilot) ? cruise : sTarget + getCornerSpeed();
    }

    float getGas() 
    {
        //Move throttle based on target speed
        if (vehicle.rigidbody.velocity.magnitude > targetSpeed)
            throttle -= throttleSpeed*2;
        else if (vehicle.rigidbody.velocity.magnitude < targetSpeed) 
            throttle += throttleSpeed;

        //Calculate forward Direction
        Vector3 dir = Quaternion.Euler(0,getSteer()*2,0) * transform.forward;
        Vector3 offset = (transform.right*2) * (getSteer() / vehicle.max_steer);

        //Respect Distance Raycast
        RaycastHit hit; bool act = false;
        if (Physics.Raycast(transform.position + (Vector3.up / 2.0f) + offset, dir, out hit, respectDistance))
        {
            if (hit.collider.tag != "Draft")
            {
                throttle -= throttleSpeed;
                act = true;
            }
        }

        //Calculate Raycast Orgin
        Vector3 rayOrgin = transform.position + (Vector3.up / 2.0f) + transform.forward;

        //Side Distance Raycast Right
        bool actR = false;
        if (Physics.Raycast(rayOrgin, transform.right, out hit, 1.5f))
        {
            if(getSteer() > 0 && hit.collider.tag != "Draft") throttle -= throttleSpeed;
            actR = true;
        }


        //Side Distance Raycast Right
        bool actL = false;
        if (Physics.Raycast(rayOrgin, -transform.right, out hit, 1.5f))
        {
            if (getSteer() < 0 && hit.collider.tag != "Draft") throttle -= throttleSpeed;
            actL = true;
        }

        //Debug Respect Ray
        if (showRespectRay) Debug.DrawRay(rayOrgin, transform.right * 1.5f, (actR) ? Color.red : Color.white);
        if (showRespectRay) Debug.DrawRay(rayOrgin, -transform.right * 1.5f, (actL) ? Color.red : Color.white);

        //Debug Respect Ray
        if (showRespectRay) Debug.DrawRay(transform.position + (Vector3.up / 2.0f) + offset, dir*respectDistance, (act) ? Color.red: Color.white);

        //Clamp throttle
        throttle = Mathf.Clamp(throttle, -4,1);

        return throttle;
    }

    float getSteer() 
    {
        //TODO: THIS IS NOT OPTIMUM
        //Get Ideal position race path
        Vector3 target = racePath.path.path.GetPointAtTime(tracker.time+getClip()+0.01f);
        Vector3 current = racePath.path.path.GetPointAtTime(tracker.time+getClip());
        
        //Calculate steer direction to target position
        Vector3 d1 = (target - transform.position).normalized;
        Vector3 d2 = (target - current).normalized;

        //Average target DIRECTION & POSITION
        Vector3 direction = Vector3.Lerp(d1,d2, getPathAdhearance());

        //Set Steering angle based on current angle
        float steerAngle = Vector3.SignedAngle(transform.forward,direction, Vector3.up);
        float steer = (steerAngle / vehicle.max_steer);


        //Calculate Raycast Orgin
        Vector3 rayOrgin = transform.position + (Vector3.up / 2.0f) + transform.forward;

        //Side Distance Raycast Right
        RaycastHit hit; bool actR = false;
        if (Physics.Raycast(rayOrgin, transform.right, out hit, 1.5f))
        {
            if (hit.collider.tag != "Draft")
            {
                steer -= steer * 0.2f;
                actR = true;
            }
        }


        //Side Distance Raycast Right
        bool actL = false;
        if (Physics.Raycast(rayOrgin, -transform.right, out hit, 1.5f))
        {
            if (hit.collider.tag != "Draft")
            {
                steer += steer * 0.2f;
                actR = true;
            }
        }

        //Debug Respect Ray
        if (showRespectRay) Debug.DrawRay(rayOrgin, transform.right * 1.5f, (actR) ? Color.red : Color.white);
        if (showRespectRay) Debug.DrawRay(rayOrgin, -transform.right * 1.5f, (actL) ? Color.red : Color.white);


        return steer;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !debugSphere) return;

        //Get Ideal Race path position
        Vector3 target = racePath.path.path.GetPointAtTime(tracker.time + 0.01f);
        
        //Draw Sphere at location
        Gizmos.DrawWireSphere(target, 1);
    }

    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Behaviour")]
    [SerializeField] private float _clip   =       float.NaN;
    [SerializeField] private float _corner =       float.NaN;
    [SerializeField] private float _bell =         float.NaN;
    [SerializeField] private float _breaking =     float.NaN;
    [SerializeField] private float _adhearance =   float.NaN;

    private float getClip() 
    {
        return _clip = Mathf.Max(0.001f * skill,0); 
    }

    private float getCornerSpeed() 
    {
        if (float.IsNaN(_corner)) _corner = ((skill / 10f) * 5) - 5;
        return _corner;
    
        // -3,8 | 0,0
    }

    private float getBellAcceleration() 
    {
        if (float.IsNaN(_bell)) _bell =  1.25f * (skill / 10f);
        return _bell;

        // 0,0 | 0.75,1.25
    }

    private float getBreakingDistance() 
    {
        if (float.IsNaN(_breaking)) _breaking = 0.015f * (1f - (skill / 10f));
        return _breaking;

        // -0.1,0.2 | -0.1,0
    }

    private float getPathAdhearance() 
    {
        if (float.IsNaN(_adhearance)) _adhearance = Mathf.Min(0.5f / (skill), 0.5f);
        return _adhearance;

        // 0.5-0.3 | 0.1
    }
}

////////////////////////////////////////////////////////////////////////////////

/*
 *  Cornering Speed   (CS)   (+)
 *  Cornering Clip    (CC)   (+)
 *  Bell Acceleration (BA)   (+)
 *  Contact Avoidance (CA)   (+)
 *  Breaking Distance (BD)   (-)
 *  Path Adherence    (PA)   (-)
 */

////////////////////////////////////////////////////////////////////////////////

/*
 *  Precision Pete  ||
 *  Drift King      ||
 *  Drag Stripper   |Ba+,Ca+,Pa+|
 *  Corner Hugger   ||
 *  Demolition Exp. |Ba+,Pa-,|
 */

////////////////////////////////////////////////////////////////////////////////
