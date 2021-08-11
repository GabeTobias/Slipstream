using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loop", menuName = "Track/Loop")]
public class TrackLoop : ScriptableObject {

    public LocRot transform;

    [SerializeField]
    Vector3[] vertices;

    [SerializeField]
    Vector3[] normals;

    [SerializeField]
    Vector2[] Uvs;

    public TrackLoop() { 
        SetDefaults(); 
    }

    public TrackLoop(TrackLoop loop){
        
        vertices = loop.GetVertices();
        normals = loop.GetNormals();
    }

	public int[] ConnectTo(TrackLoop loop, int index){
        List<int> tri = new List<int>();

        for (int i = index; i < vertices.Length + index; i++){
            tri.Add(i + vertices.Length);
            tri.Add(i);
            tri.Add(i + 1);

            tri.Add(i + 1);
            tri.Add(i + vertices.Length + 1);
            tri.Add(i + vertices.Length);
        }

        return tri.ToArray();
    }

    public void SetVertices(Vector3[] v){
        vertices = v;
    }

    public Vector3[] GetVertices() { return vertices; }

    public Vector3[] GetNormals() { return normals;  }

    public Vector2[] GetUVs(float val) {
         
        for (int i = 0; i < Uvs.Length; i++){
            Uvs[i] = new Vector2(Uvs[i].x,val);
        }

        return Uvs; 
    }

    public void SetDefaults()
    {

        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> UVs = new List<Vector2>();

        Vector3 innerPoint = Vector3.zero;
        //innerPoint = new Vector3 (innerPoint.x,terrain.HeightMap [(int)innerPoint.x + 200, (int)innerPoint.y + 200] + 1.5f,innerPoint.z);

        Vector3 Left = innerPoint - (Vector3.right * 14);
        Vector3 Right = innerPoint + (Vector3.right * 14);

        Vector3 UpLeft = Left + (Vector3.up * 0f);
        Vector3 UpRight = Right + (Vector3.up * 0f);

        Vector3 FarLeft = UpLeft - (Vector3.right*1);
        Vector3 FarRight = UpRight + (Vector3.right*1);

        Vector3 BottomLeft = Left - (Vector3.right*1);
        Vector3 BottomRight = Right + (Vector3.right*1);

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

        //Add UVs
        UVs.Add(new Vector2(0, 0));
        UVs.Add(new Vector2(0, 0));
        UVs.Add(new Vector2(0, 0));

        UVs.Add(new Vector2(0, 0));
        UVs.Add(new Vector2(1, 1));

        UVs.Add(new Vector2(1, 0));
        UVs.Add(new Vector2(1, 0));
        UVs.Add(new Vector2(1, 0));

        vertices = verts.ToArray();
        normals = norms.ToArray();
        Uvs = UVs.ToArray();

    }
}