using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SplineMesh : MonoBehaviour
{
    public float divisions = 8;
    public float width = 2f;
    public float height = 0.25f;

    public float UVStretch = 10;

    public bool Taper = true;
    public bool Snap = false;

    //TODO: Maybe convert this into a list
    public TrackNode nodeA;
    public TrackNode nodeB;

    void OnDrawGizmos()
    {
        if (!Selection.Contains(gameObject)) return;

        // Draw Node
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawSphere(nodeA.transform.position, 2);
        Gizmos.DrawSphere(nodeB.transform.position, 2);

        // Draw Handle B
        Gizmos.DrawSphere(nodeA.HandleB, 1);
        Gizmos.DrawLine(nodeA.HandleB, nodeA.transform.position);

        // Draw Handle A
        Gizmos.DrawSphere(nodeB.HandleA, 1);
        Gizmos.DrawLine(nodeB.HandleA, nodeB.transform.position);

        // Draw Points inbetween nodes
        for (int i = 1; i < divisions; i++)
        {
            LocRot l = nodeB.GetPoint(nodeA, i / divisions);
            LocRot last = nodeB.GetPoint(nodeA, (i - 1) / divisions);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(l.position + Vector3.up, last.position + Vector3.up);
            Gizmos.DrawSphere(l.position + Vector3.up, 0.25f);
        }
    }

    void ClearMesh() { }

    public void GenerateMesh(TrackPath path = null, float start = -1, float stop = -1, float pathOffset = 0)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> texcoords = new List<Vector2>();
        List<int> tris = new List<int>();

        List<float> distances = new List<float>();

        float totalDist = 0;

        //TODO: Make this loop distance based instead of percent based
        for (int i = 0; i < divisions; i++)
        {

            //Calculate curent transform
            LocRot trans;
            LocRot transLast;

            if (path == null)
            {
                trans = nodeB.GetPoint(nodeA, i / divisions);
                transLast = nodeB.GetPoint(nodeA, (i - 1) / divisions);
            }
            else 
            {
                float dist = (stop - start) / divisions;
                float time = start + (i * dist);

                trans = path.GetTime(time);
                transLast = path.GetTime(time-dist);

                trans.position += trans.right() * pathOffset;
                transLast.position += transLast.right() * pathOffset;
            }

            //Isolate y axis
            Vector3 current = new Vector3(trans.position.x, 0, trans.position.z);
            Vector3 last = new Vector3(transLast.position.x, 0, transLast.position.z);

            //Add to distance
            totalDist += Vector3.Distance(current,last);
            distances.Add(totalDist);

            //Calculate Rotation
            Quaternion r = Quaternion.LookRotation(current - last, Vector3.up);

            //Raycast to ground
            RaycastHit hit;
            if (Physics.Raycast(trans.position + (Vector3.up * 100), -Vector3.up, out hit, 200.0f) && false)
                trans.position = new Vector3(trans.position.x, hit.point.y, trans.position.z);

            //Add Vertices
            float localWidth = (Taper && (i == 0 || i == (int)divisions)) ? 0 : width;
            Vector3 localDir = (path == null) ? Vector3.right : Vector3.forward;

            vertices.Add(trans.position);
            vertices.Add(trans.position + (Vector3.up * height));
            vertices.Add(trans.position + (Vector3.up * height) + ((r * localDir) * width));
            vertices.Add(trans.position + ((r * localDir) * width));

            //Add Triangles
            if (i < divisions - 1)
            {
                //Get Index
                int index = i * 4;

                //Bottom
                tris.Add(index + 7);
                tris.Add(index + 4);
                tris.Add(index);

                tris.Add(index + 3);
                tris.Add(index + 7);
                tris.Add(index);


                //Top
                tris.Add(index+1);
                tris.Add(index + 5);
                tris.Add(index + 6);

                tris.Add(index + 1);
                tris.Add(index + 6);
                tris.Add(index + 2);

                //Left
                tris.Add(index);
                tris.Add(index + 4);
                tris.Add(index + 5);

                tris.Add(index);
                tris.Add(index + 5);
                tris.Add(index + 1);

                //Right
                tris.Add(index + 2);
                tris.Add(index + 6);
                tris.Add(index + 7);

                tris.Add(index + 2);
                tris.Add(index + 7);
                tris.Add(index + 3);
            }
        }

        for (int i = 0; i < distances.Count; i++) 
        {
            distances[i] /= totalDist;

            //Add UVs
            texcoords.Add(new Vector2(0, distances[i] * UVStretch));
            texcoords.Add(new Vector2(0, distances[i] * UVStretch));
            texcoords.Add(new Vector2(1, distances[i] * UVStretch));
            texcoords.Add(new Vector2(1, distances[i] * UVStretch));
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = texcoords.ToArray();

        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
