using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SplinePlacment : MonoBehaviour
{
    //TODO: Maybe convert this into a list
    public TrackNode nodeA;
    public TrackNode nodeB;
    
    public float divisions = 8;
    public float strength = 5;
    public float dDistance = 0.8f;

    public bool Double;
    public bool FlooSnap;

    public float ObjectMass = 1;
    public bool isKinematic = true;

    public Mesh mesh;

    [SerializeField]
    private GameObject parent;

    // Start is called before the first frame update

    void OnDrawGizmos()
    {
        if (!Selection.Contains(gameObject)) return;

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawSphere(nodeA.transform.position, 2);
        Gizmos.DrawSphere(nodeB.transform.position, 2);

        //Draw Handle B
        Gizmos.DrawSphere(nodeA.HandleB, 1);
        Gizmos.DrawLine(nodeA.HandleB, nodeA.transform.position);
                       
        //Draw Handle A
        Gizmos.DrawSphere(nodeB.HandleA, 1);
        Gizmos.DrawLine(nodeB.HandleA, nodeB.transform.position);

        for (int i = 1; i < divisions*10; i++)
        {
            LocRot l = nodeB.GetPoint(nodeA, i / (divisions*10));
            LocRot last = nodeB.GetPoint(nodeA, (i-1) / (divisions*10));

            Gizmos.color = Color.green;
            Gizmos.DrawLine(l.position + Vector3.up, last.position + Vector3.up);
            Gizmos.DrawSphere(l.position + Vector3.up, 0.25f);
        }
    }

    public void ClearShape() 
    {
        DestroyImmediate(parent);
    }

    public void PlaceObject(LocRot trans,Vector3 current, Vector3 last, Vector3 offset)
    {
        //Create new object
        GameObject g = new GameObject();

        //Add Mesh filter
        MeshFilter filter = g.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        //Add Mesh Renderer
        MeshRenderer renderer = g.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Diffuse"));

        //Add Mesh Collider
        MeshCollider collider = g.AddComponent<MeshCollider>();
        collider.convex = true;

        //Add Rigidbody
        Rigidbody rig = g.AddComponent<Rigidbody>();
        rig.isKinematic = isKinematic;
        rig.mass = ObjectMass;

        //Calculate Rotation
        Quaternion r = Quaternion.LookRotation(current - last, Vector3.up);

        //Raycast to ground
        RaycastHit hit;
        if (Physics.Raycast(trans.position + (Vector3.up * 100), -Vector3.up, out hit, 200.0f) && FlooSnap)
            trans.position = new Vector3(trans.position.x, hit.point.y, trans.position.z);

        //Set Object transform
        g.transform.position = trans.position + offset;
        g.transform.rotation = r;
        g.transform.SetParent(parent.transform);
    }

    public void GenerateShape() 
    {
        //Generate new Parent object
        parent = new GameObject();
        parent.transform.SetParent(transform);

        Vector3 lastPlace = nodeA.transform.position;

        //TODO: Make this loop distance based instead of percent based
        for (int i = 0; i < divisions*100; i++) 
        {
            //Calculate curent transform
            float percent = i / (divisions*100);
            LocRot trans = nodeB.GetPoint(nodeA,percent);

            //Isolate y axis
            Vector3 current = new Vector3(trans.position.x, 0, trans.position.z);
            Vector3 last = new Vector3(lastPlace.x, 0, lastPlace.z);

            //Check last points distance
            if (Vector3.Distance(current, last) >= divisions)
            {
                if (Double) 
                {
                    PlaceObject(trans, current, last, trans.right() * dDistance);
                    PlaceObject(trans, current, last, -trans.right() * dDistance);
                }
                else PlaceObject(trans, current, last, Vector3.zero);

                //Update Last position
                lastPlace = trans.position;
            }
        }
    }
}
