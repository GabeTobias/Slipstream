using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackExtras : MonoBehaviour
{

    [SerializeField]
    private GameObject parent;

    public Material rumbleMat;
    public float SplineStrength = 25;

    public void Generate(TrackPath path)
    {
        TrackNode[] trackNodes = path.Nodes.ToArray();

        //Generate new Parent object
        DestroyImmediate(parent);
        parent = new GameObject();
        parent.transform.SetParent(transform);

        for (int i = 0; i < trackNodes.Length; i++)
        {
            TrackNode current = new TrackNode(new LocRot(trackNodes[i].transform.position, trackNodes[i].transform.rotation));
            TrackNode last = (i == 0) ? trackNodes[trackNodes.Length - 1] : trackNodes[i - 1];
            TrackNode next = (i == trackNodes.Length - 1) ? trackNodes[0] : trackNodes[i + 1];

            Vector3 lastDir = last.transform.position - current.transform.position;
            Vector3 nextDir = next.transform.position - current.transform.position;

            //find the direction to points
            Vector3 avg = (last.transform.position + next.transform.position) / 2f;

            float angle = Vector3.Angle(lastDir, nextDir);
            float time = path.SnapTo(current.transform.position).Item1;
            bool toRight = Vector3.Dot(current.transform.right(), current.transform.position - avg) > 0;


            //Generate Rumble Object
            GameObject g = new GameObject();
            g.transform.SetParent(parent.transform);
            g.transform.localPosition = Vector3.up * 0.11f;


            //Add Mesh filter
            MeshFilter filter = g.AddComponent<MeshFilter>();

            //Add Mesh Renderer
            MeshRenderer renderer = g.AddComponent<MeshRenderer>();
            renderer.material = rumbleMat;

            //Add Mesh Collider
            MeshCollider collider = g.AddComponent<MeshCollider>();
            collider.convex = true;


            ////Generate Node A
            //LocRot aTransform = path.GetTime(time + (toRight ? 0.02f : -0.02f));
            //aTransform.position += aTransform.right() * (toRight ? 15f : -15f);
            //aTransform.position += new Vector3(0, 0.1f, 0);

            ////Generate Node B
            //LocRot bTransform = path.GetTime(time + (toRight ? -0.02f : 0.02f));
            //bTransform.position += bTransform.right() * (toRight ? 15f : -15f);
            //bTransform.position += new Vector3(0, 0.1f, 0);

            //Generate Spline Mesh
            SplineMesh rumbleMesh = g.AddComponent<SplineMesh>();
            rumbleMesh.divisions = 20.1f;
            rumbleMesh.Taper = true;
            rumbleMesh.width = 2f;
            rumbleMesh.height = 0.1f;
            rumbleMesh.UVStretch = 10;

            //rumbleMesh.nodeA = new TrackNode(aTransform);
            //rumbleMesh.nodeA.HandleB = aTransform.position + aTransform.forward() * (toRight ? -SplineStrength - 1 : SplineStrength + 1);

            //rumbleMesh.nodeB = new TrackNode(bTransform);
            //rumbleMesh.nodeB.HandleA = bTransform.position + bTransform.forward() * (toRight ? SplineStrength - 1 : -SplineStrength + 1);

            //rumbleMesh.GenerateMesh();
            rumbleMesh.GenerateMesh(path, time+0.02f,time-0.02f, toRight ? 12.25f: -12.25f);

        }
    }
}
