using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RaceLine : MonoBehaviour
{
    [SerializeField]
    public List<TrackNode> Nodes = new List<TrackNode>();
    public LocRot[] vertexTransforms;

    public float lineWidth;
    public float Smooth = 80;
    public float Stretch = 20;

    void OnDrawGizmos()
    {
        if (!Selection.Contains(gameObject)) return;

        if (vertexTransforms == null) GenerateLine();

        for (int i = 0; i < Nodes.Count; i++)
        {
            TrackNode node = Nodes[i];

            Gizmos.color = new Color(1, 1, 1, 0.5f);
            Gizmos.DrawCube(node.transform.position, Vector3.one * 4);

            //Draw Handle A
            Gizmos.color = new Color(1, 1, 1, 0.25f);
            Gizmos.DrawSphere(node.HandleA, 2);
            Gizmos.DrawLine(node.transform.position, node.HandleA);

            //Draw Handle B
            Gizmos.color = new Color(1, 1, 1, 0.25f);
            Gizmos.DrawSphere(node.HandleB, 2);
            Gizmos.DrawLine(node.transform.position, node.HandleB);
        }

        for (int i = 0; i < vertexTransforms.Length; i++)
        {
            LocRot current = vertexTransforms[i];
            LocRot next = (i == vertexTransforms.Length - 1) ? vertexTransforms[0] : vertexTransforms[i + 1];

            //Draw Handle A
            Gizmos.color = new Color(1, 1, 1, 0.5f);
            Gizmos.DrawLine(current.position, next.position);
        }
    }

    public void GenerateLine() 
    {
        List<LocRot> trans = new List<LocRot>();
        List<LocRot> t2 = new List<LocRot>();

        //Generate List of LocRots
        for (int i = 0; i < Nodes.Count; i++)
        {
            for (int j = 0; j < Smooth; j++)
            {
                TrackNode p = (i >= 1) ? Nodes[i - 1] : Nodes[Nodes.Count - 1];
                LocRot t = Nodes[i].GetPoint(p, j / Smooth);

                trans.Add(t);
            }
        }

        //Add Closing Node
        TrackNode p1 = Nodes[Nodes.Count - 1];
        LocRot t1 = Nodes[0].GetPoint(p1, 0);
        trans.Add(t1);

        t2.Add(trans[0]);
        LocRot last = trans[0];

        //Create new list of evenly spaced points
        for (int i = 1; i < trans.Count; i++)
        {
            float angle = trans[i].rotation.y - last.rotation.y;
            float dist = Vector3.Distance(trans[i].position, last.position);

            if (Mathf.Abs(angle) < 4 || dist < 0.5f)
            {
                t2.Add(trans[i]);
                last = trans[i];
            }
        }

        vertexTransforms = t2.ToArray();
    }

    public void GenerateMesh() 
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> texcoords = new List<Vector2>();
        List<int> tris = new List<int>();

        List<float> distances = new List<float>();

        float totalDist = 0;

        //TODO: Make this loop distance based instead of percent based
        for (int i = 0; i < vertexTransforms.Length; i++)
        {
            //Calculate curent transform
            LocRot trans = vertexTransforms[i];
            LocRot transLast = (i == 0) ? vertexTransforms[vertexTransforms.Length-1] : vertexTransforms[i-1];

            //Add to distance
            totalDist += Vector3.Distance(trans.position, transLast.position);
            distances.Add(totalDist);

            //Raycast to ground
            RaycastHit hit;
            if (Physics.Raycast(trans.position + (Vector3.up * 100), -Vector3.up, out hit, 200.0f) && false)
                trans.position = new Vector3(trans.position.x, hit.point.y, trans.position.z);

            //Calculate Rotation
            Quaternion r = Quaternion.LookRotation(trans.position - transLast.position, Vector3.up);

            if (i == vertexTransforms.Length-1) 
            {
                //Calculate curent transform
                trans = vertexTransforms[0];
                transLast = vertexTransforms[vertexTransforms.Length - 1];

                //Calculate Rotation
                r = Quaternion.LookRotation(trans.position - transLast.position, Vector3.up);
            }

            //Add Vertices
            vertices.Add(trans.position - (r * Vector3.right * lineWidth));
            vertices.Add(trans.position + (r * Vector3.right * lineWidth));

            //Add Triangles
            if (i < vertexTransforms.Length - 1)
            {
                //Get Index
                int index = i * 2;

                //Bottom
                tris.Add(index + 2);
                tris.Add(index + 3);
                tris.Add(index);

                tris.Add(index + 3);
                tris.Add(index + 1);
                tris.Add(index);
            }
        }

        for (int i = 0; i < distances.Count; i++)
        {
            distances[i] /= totalDist;

            //Add UVs
            texcoords.Add(new Vector2(0, distances[i] * Stretch));
            texcoords.Add(new Vector2(1, distances[i] * Stretch));
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = texcoords.ToArray();

        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    public void Calculate(TrackPath path)
    {
        //Set Nodes
        TrackNode[] trackNodes = path.Nodes.ToArray();

        //Clear Previous Nodes
        Nodes.Clear();

        //Offset Positions
        for (int i = 0; i < trackNodes.Length; i++)
        {
            TrackNode current = new TrackNode(new LocRot(trackNodes[i].transform.position, trackNodes[i].transform.rotation));
            TrackNode last = (i == 0) ? trackNodes[trackNodes.Length - 1] : trackNodes[i - 1];
            TrackNode next = (i == trackNodes.Length - 1) ? trackNodes[0] : trackNodes[i + 1];

            //find the direction to points
            Vector3 avg = (last.transform.position + next.transform.position) / 2f;

            //Dot product the right
            bool toRight = Vector3.Dot(current.transform.right(), current.transform.position - avg) > 0;
            float straight = Vector3.Dot(current.transform.position - last.transform.position, current.transform.position - next.transform.position);

            //Move in direction
            current.transform.position += (toRight) ? current.transform.right() * -10: current.transform.right() * 10;

            //Add Node to Array
            Nodes.Add(current);
        }

        //Recalculate Handles
        for (int i = 0; i < Nodes.Count; i++)
        {
            TrackNode last = (i == 0) ? Nodes[Nodes.Count - 1] : Nodes[i - 1];
            TrackNode next = (i == Nodes.Count - 1) ? Nodes[0] : Nodes[i + 1];

            Nodes[i].CalculateHandles(last.transform, next.transform, 1.4f);
        }
    }
}
