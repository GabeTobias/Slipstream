using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackPath)), CanEditMultipleObjects]
public class TrackPathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TrackPath path = (TrackPath)target;

        path.CurveStrength = EditorGUILayout.FloatField("Curve Strength", path.CurveStrength);
        path.TrackStart = EditorGUILayout.FloatField("Start Time", path.TrackStart);

        path.TrackSize = EditorGUILayout.IntField("Corner Count", path.TrackSize);
        path.TrackWidth = EditorGUILayout.IntField("Road Width", path.TrackWidth);

        string[] options = {"none", "FullTrack", "Segments"};
        path.GUI = (TrackPath.GUIState) EditorGUILayout.Popup((int)path.GUI, options);

        if (GUILayout.Button("Generate Random"))
        {
            if (path.shapeGen == null) path.shapeGen = path.gameObject.GetComponent<TrackShape>();

            path.Shape = path.shapeGen.GenerateShape(path.TrackSize);
            path.vertexTransforms = path.GeneratePath();
            path.segments = path.SegmentTrack(path.vertexTransforms);

            EditorUtility.SetDirty((TrackPath)target);
        }

        if (GUILayout.Button("Flatten"))
        {

            for (int i = 0; i < path.Nodes.Count; i++) 
            {
                path.Nodes[i].transform.position = new Vector3(path.Nodes[i].transform.position.x, 0, path.Nodes[i].transform.position.z);
            }

            path.vertexTransforms = path.GeneratePath();
            path.segments = path.SegmentTrack(path.vertexTransforms);

            path.GetComponent<TrackMesh>().Generate();

            EditorUtility.SetDirty((TrackPath)target);
        }
    }

    private void OnSceneGUI()
    {
        TrackPath path = (TrackPath)target;

        for (int i = 0; i < path.Nodes.Count; i++)
        {
            Handles.CubeHandleCap(0,path.Nodes[i].transform.position, path.Nodes[i].transform.rotation, 4, EventType.MouseDown);
            path.Nodes[i].transform.position = Handles.PositionHandle(path.Nodes[i].transform.position, path.Nodes[i].transform.rotation);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty((TrackPath)target);
            path.GetComponent<TrackMesh>().Generate();
        }

        if (path.Shape.Length > 0) path.GeneratePath();
        if(path.Nodes.Count > 0) path.segments = path.SegmentTrack(path.vertexTransforms);
    }
}
