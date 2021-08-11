using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrackMesh : MonoBehaviour 
{
    public TrackPath Path;

    public TrackLoop basic;

    List<TrackLoop> Loops = new List<TrackLoop>();

    List<Vector3> Vertices = new List<Vector3>();
    List<int> Triangles = new List<int>();

    List<Vector3> Normals = new List<Vector3>();
    List<Vector2> UVs = new List<Vector2>();

    public MeshFilter filter;

    public float TextureDistance = 50;

    private bool generated = false;

	// Use this for initialization
	public void Generate() 
    {
        Loops = new List<TrackLoop>();

        Vertices = new List<Vector3>();
        Triangles = new List<int>();

        Normals = new List<Vector3>();
        UVs = new List<Vector2>();


        if(Path == null) Path = GetComponent<TrackPath>();
        if(filter == null) filter = GetComponent<MeshFilter>();
        
        BuildMesh();

        generated = true;
	}

    public void Regenerate() 
    {
        Generate();
    }

    public void Clear() 
    {
        generated = false;
    }

    void BuildMesh()
    {
        GenerateMesh(Path.vertexTransforms);
    }

    void AddLoop(TrackLoop loop, LocRot node){

        TrackLoop l = new TrackLoop();

        l.transform = node;

        Loops.Add(l);
    }

    void GenerateTris(){
        TrackLoop t = new TrackLoop();
        
        for (int i = 0; i < Path.vertexTransforms.Length-1; i++) {
            Triangles.AddRange(t.ConnectTo(t,i*8));
        }

        for (int i = 0; i < Triangles.Count; i++) {
            Triangles[i] = Triangles[i] % (Vertices.Count);
        }
    }

    void GenerateLoops(LocRot[] points){
        for (int i = 0; i < points.Length; i++)
        {
            AddLoop(basic, points[i]);
        }
    }

    void GenerateAllUVs(float Stretch)
    {
        TrackLoop t = new TrackLoop();

        for (int i = 0; i < Path.vertexTimes.Length; i++) 
        {
            float time = Path.vertexTimes[i] * Stretch;
            UVs.AddRange(t.GetUVs(time));
        }
    }

    int GenerateUVs(int start, float Stretch, bool pole){
        float distance = 0;

        //Calculate number of steps till desired distance
        for (int i = start; i < Loops.Count-1; i++){
            distance += Vector3.Distance(Loops[i].transform.position,Loops[i+1].transform.position);

            if(distance >= Stretch){
                
                //Generate UV Data
                for (int j = start; j < i; j++)
                {
                    if (pole)
                        UVs.AddRange(Loops[j].GetUVs(j / distance));
                    else
                        UVs.AddRange(Loops[j].GetUVs(1 - (j / distance)));
                }

                return i - start;
            }
        }

        return 0;
    }

    void GenerateMesh(LocRot[] data)
    {
        TrackLoop t = new TrackLoop();

        //Add Vertices
        for(int i = 0; i < data.Length; i++)
        {
            for(int j = 0; j < t.GetVertices().Length; j++)
            {
                //Transform to world space
                Vertices.Add( data[i].LocalToWorld(t.GetVertices()[j]));
            }
        }

        //Add Normals
        for(int i = 0; i < data.Length; i++)
        {
            Normals.AddRange(t.GetNormals());
        }

        //Generate Triangles
        GenerateTris();

        //Generate UVs
        GenerateAllUVs(TextureDistance);

        if (filter.sharedMesh == null)
        {
            filter.sharedMesh = new Mesh();
        }

        filter.sharedMesh.Clear();
        filter.sharedMesh.vertices = Vertices.ToArray();
        filter.sharedMesh.triangles =  Triangles.ToArray();
        filter.sharedMesh.normals = Normals.ToArray();
        filter.sharedMesh.uv = UVs.ToArray();

    }

}

//800 1600 20 1