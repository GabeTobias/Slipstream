using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopData : MonoBehaviour {

    public Transform[] Nodes;

    public bool LoadedNodes;
    public float Stretch;

    [HideInInspector]
    public Vector3[] vertices;

    [HideInInspector]
    public Vector3[] normals;

    [HideInInspector]
    public Vector2[] Uvs;

    public void LoadTransforms(){
        if(vertices == null)SetDefaults();

        if(Nodes == null)Nodes = new Transform[vertices.Length];

        for (int i = 0; i < Nodes.Length; i++){
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.transform.localScale = new Vector3(0.075f,0.075f,0.075f);

            Nodes[i] = g.transform;
            Nodes[i].position = vertices[i];
        }

        LoadedNodes = true;
    }

    public void SetDefaults(){

        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();

        Vector3 innerPoint = transform.InverseTransformPoint(transform.position);
        //innerPoint = new Vector3 (innerPoint.x,terrain.HeightMap [(int)innerPoint.x + 200, (int)innerPoint.y + 200] + 1.5f,innerPoint.z);

        Vector3 Left = innerPoint - (transform.right * 4 * 2);
        Vector3 Right = innerPoint;

        Vector3 UpLeft = Left + (transform.up * 0.1f);
        Vector3 UpRight = Right + (transform.up * 0.1f);

        Vector3 FarLeft = UpLeft - transform.right;
        Vector3 FarRight = UpRight + transform.right;

        Vector3 BottomLeft = Left - transform.right;
        Vector3 BottomRight = Right + transform.right;

        //Add Vertices
        verts.Add(BottomRight);
        verts.Add(FarRight);
        verts.Add(UpRight);
        verts.Add(Right);
        verts.Add(Left);
        verts.Add(UpLeft);
        verts.Add(FarLeft);
        verts.Add(BottomLeft);

        //Add Normals
        norms.Add(Vector3.right);
        norms.Add(Vector3.up);
        norms.Add(Vector3.up);
        norms.Add(Vector3.up);
        norms.Add(Vector3.up);
        norms.Add(Vector3.up);
        norms.Add(Vector3.up);
        norms.Add(Vector3.right);

        vertices = verts.ToArray();
        normals = norms.ToArray();

        for (int i = 0; i < Nodes.Length; i++)
        {
            Nodes[i].position = vertices[i];
        }

    }

}
