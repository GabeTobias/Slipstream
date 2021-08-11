using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Shape)), CanEditMultipleObjects]
public class ShapeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Shape shape = (Shape)target;

        shape.Range = EditorGUILayout.FloatField("Shape Size", shape.Range);
        shape.Size = EditorGUILayout.IntField("Vertex Count", shape.Size);

        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate"))
        {
            shape.GenerateRandom(shape.Size, shape.Range);
        }

        if (GUILayout.Button("Generate Mesh"))
        {
            TrackPath path = FindObjectOfType<TrackPath>();
            RaceLine line = FindObjectOfType<RaceLine>();
            TrackExtras extra = FindObjectOfType<TrackExtras>();

            path.Shape = shape.GenerateRandom(shape.Size, shape.Range);

            path.vertexTransforms = FindObjectOfType<TrackPath>().GeneratePath();
            path.segments = FindObjectOfType<TrackPath>().SegmentTrack(FindObjectOfType<TrackPath>().vertexTransforms);
            path.GetComponent<TrackMesh>().Generate();

            if (line != null)
            {
                line.Calculate(path);
                line.GenerateLine();
                line.GenerateMesh();
            }

            if (extra != null) 
            {
                extra.Generate(path);
            }
        }
    }

    private void OnSceneGUI()
    {
        Shape shape = (Shape)target;

        for (int i = 0; i < shape.vertices.Length; i++) 
        {
            Handles.Label(shape.vertices[i], i.ToString());
        }
    }
}
