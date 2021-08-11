using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RaceLine)), CanEditMultipleObjects]
public class RaceLineEditor : Editor
{
    int selected = -1;

    public override void OnInspectorGUI()
    {
        RaceLine line = (RaceLine)target;

        line.lineWidth = EditorGUILayout.FloatField("Line Width", line.lineWidth);
        line.Smooth = EditorGUILayout.FloatField("Vertex Smoothing", line.Smooth);
        line.Stretch = EditorGUILayout.FloatField("UV Stretch", line.Stretch);

        EditorGUILayout.Space();

        if (GUILayout.Button("Flatten"))
        {
            for (int i = 0; i < line.Nodes.Count; i++)
            {
                line.Nodes[i].transform.position = new Vector3(line.Nodes[i].transform.position.x, 0, line.Nodes[i].transform.position.z);
            }

            line.GenerateLine();
            line.GenerateMesh();
        }

        if (GUILayout.Button("Generate Line"))
        {
            line.GenerateLine();
            line.GenerateMesh();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Match Track"))
        {
            line.Nodes = FindObjectOfType<TrackPath>().Nodes;
        }

        if (GUI.changed) 
        {
            line.GenerateLine();
            line.GenerateMesh();
        }
    }

    private void OnSceneGUI()
    {
        RaceLine line = (RaceLine)target;

        for (int i = 0; i < line.Nodes.Count; i++)
        {
            TrackNode node = line.Nodes[i];

            //Draw Root Handle
            EditorGUI.BeginChangeCheck();
            Vector3 hR = Handles.PositionHandle(node.transform.position, node.transform.rotation);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(line, "Change Node Position");

                Vector3 offset = node.transform.position - hR;
                node.transform.position = hR;

                node.HandleA -= offset;
                node.HandleB -= offset;

                selected = i;
            }
                
            node.transform.rotation = Quaternion.FromToRotation(Vector3.right, node.transform.position);

            if (selected != i) continue;

            EditorGUI.BeginChangeCheck();

            Vector3 hA = Handles.PositionHandle(node.HandleA, node.transform.rotation);
            Vector3 hB = Handles.PositionHandle(node.HandleB, node.transform.rotation);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(line, "Change Node Handle");
                if (hA != node.HandleA)
                {
                    Vector3 offset = node.HandleA - node.transform.position;
                    node.HandleA = hA;
                    node.HandleB = node.transform.position - offset;
                }
                else if (hB != node.HandleB)
                {
                    Vector3 offset = node.HandleB - node.transform.position;
                    node.HandleB = hB;
                    node.HandleA = node.transform.position - offset;
                }
            }

        }


        if (GUI.changed)
        {
            line.GenerateLine();
            line.GenerateMesh();
            EditorUtility.SetDirty(line);
        }
    }
}
