using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Shape : MonoBehaviour
{
    public Vector3[] vertices;

    public int Size;
    public float Range;

    void OnDrawGizmos()
    {
        if (!Selection.Contains(gameObject)) return;

        //Draw cubes vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.color = new Color(1, 1, 1, 0.5f);
            Gizmos.DrawCube(vertices[i], Vector3.one * 2);

            Gizmos.color = new Color(1, 1, 1, 0.1f);
            Gizmos.DrawLine(vertices[i], (i == vertices.Length-1) ? vertices[0] : vertices[i+1]);
        }
    }

    public LocRot[] GenerateRandom(int pointCount, float range)
    {
        //List of points
        List<Vector3> points = new List<Vector3>();

        //Generate Points
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 point = new Vector3(Random.Range(-range / 2, range / 2), 0, Random.Range(-range / 2, range / 2));
            bool cont = false;

            for (int j = 0; j < points.Count; j++)
            {
                if (Vector3.Distance(point, points[j]) < 80)
                {
                    cont = true;
                }
            }

            if (!cont) points.Add(point);
            else i--;
        }

        //Get Hull
        vertices = HullGenerator.concaveHull(points).ToArray();

        //Create LocRots from vertices
        LocRot[] transforms = new LocRot[vertices.Length];
        for(int i = 0; i < vertices.Length; i++) 
        {
            transforms[i] = new LocRot();
            transforms[i].position = vertices[i];
            transforms[i].rotation = Quaternion.FromToRotation(-Vector3.right, transforms[i].position);
        }

        return transforms;
    }

    public Vector3[] CleanShape(Vector3[] shape, float minAng)
    {
        List<Vector3> final = new List<Vector3>();

        for (int i = 1; i < shape.Length-1; i++) 
        {
            //Calculate node positions
            Vector2 last = new Vector2(shape[i - 1].x, shape[i - 1].z);
            Vector2 next = new Vector2(shape[i + 1].x, shape[i + 1].z);
            Vector2 current = new Vector2(shape[i].x, shape[i].z);

            float angle = Vector2.Angle(last - current, next - current);

            //Check Angle & Distances are in range
            if (Mathf.Abs(angle) >= minAng) 
            {
                final.Add(shape[i]);
            }
        }

        return final.ToArray();
    }
}
