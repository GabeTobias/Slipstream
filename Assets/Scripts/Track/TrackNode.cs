using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackNode 
{
    [SerializeField]
    public LocRot transform;

    [SerializeField]
    public Vector3 HandleA;

    [SerializeField]
    public Vector3 HandleB;

    public float HandleStrength = 10;
    public bool raycasting = false;

    public TrackNode(LocRot trans) { transform = trans; }

	public void CalculateHandles(LocRot next, LocRot last, float handleScale = 1) 
    {
        //Find directions to new and previouse nodes
        Vector3 v1 = (last.position - transform.position).normalized;
        Vector3 v2 = (next.position - transform.position).normalized;

        //Interpolate between them to find tangent
        Vector3 v3 = Vector3.Lerp(v1, v2, 0.5f);

        float distA = Vector3.Distance(last.position,transform.position);
        float distB = Vector3.Distance(next.position,transform.position);

        //Find Handle Distance
        float mDist = Mathf.Max(distA,distB);
        float handleDist = ((180f-Vector3.Angle(v1,v2)) / 180f) * (mDist/2f);
        handleDist = Mathf.Clamp(handleDist, 30, (mDist / 4f));
        handleDist *= handleScale;

        //Determine if turning to left or right
        if (Vector3.Dot(v3, transform.right()) < 0) 
        {
            HandleB = (Quaternion.Euler(0, -90, 0) * v3).normalized * handleDist + transform.position;
            HandleA = (Quaternion.Euler(0, 90, 0) * v3).normalized * handleDist + transform.position;
        } else {
            HandleB = (Quaternion.Euler(0, 90, 0) * v3).normalized * handleDist + transform.position;
            HandleA = (Quaternion.Euler(0, -90, 0) * v3).normalized * handleDist + transform.position;
        }
    }

    public LocRot GetPoint(TrackNode last, float val)
    {
        //Generate Loc Rot Values
        Vector3 Loc = VMath.GetPoint(new Vector3[] { last.transform.position,last.HandleB, HandleA, transform.position }, val);
        Quaternion Rot = VMath.GetOrientation3D(new Vector3[] { last.transform.position, last.HandleB, HandleA, transform.position }, val, Vector3.up);
        Rot.eulerAngles = new Vector3(Rot.eulerAngles.x%360,Rot.eulerAngles.y % 360,Rot.eulerAngles.z % 360);
        
        LocRot t = new LocRot();
        float yy = Loc.y;

        //Move y of loc to the ground collider
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(Loc.x, 100, Loc.z), -Vector3.up, out hit)) {
            if(raycasting) yy = hit.point.y;
        }

        //Set Loc Rot values
        t.position = new Vector3(Loc.x, yy+0.02f, Loc.z);
        t.rotation = Rot;

        return t;
    }


}