using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineMesh)), CanEditMultipleObjects]
public class SplineMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SplineMesh path = (SplineMesh)target;

        path.divisions = EditorGUILayout.FloatField("Divisions", path.divisions);
        path.width = EditorGUILayout.FloatField("Width", path.width);
        path.height = EditorGUILayout.FloatField("Height", path.height);
        path.UVStretch = EditorGUILayout.FloatField("UV", path.UVStretch);

        path.Snap = EditorGUILayout.Toggle("Snap To Floor", path.Snap);
        path.Taper = EditorGUILayout.Toggle("Taper Ends", path.Taper);

        EditorGUILayout.Separator();

        if (GUILayout.Button("Generate Shape"))
        {
            path.GenerateMesh();
        }

        if (GUILayout.Button("flatten Shape"))
        {
            path.nodeA.transform.position = new Vector3(path.nodeA.transform.position.x, 0, path.nodeA.transform.position.z);
            path.nodeA.HandleA = new Vector3(path.nodeA.HandleA.x, 0, path.nodeA.HandleA.z);
            path.nodeA.HandleB = new Vector3(path.nodeA.HandleB.x, 0, path.nodeA.HandleB.z);

            path.nodeB.transform.position = new Vector3(path.nodeB.transform.position.x, 0, path.nodeB.transform.position.z);
            path.nodeB.HandleA = new Vector3(path.nodeB.HandleA.x, 0, path.nodeB.HandleA.z);
            path.nodeB.HandleB = new Vector3(path.nodeB.HandleB.x, 0, path.nodeB.HandleB.z);


            path.GenerateMesh();
        }

        if (GUI.changed)
        {
            path.GenerateMesh();
            EditorUtility.SetDirty((SplineMesh)target);
        }
    }

    private void OnSceneGUI()
    {
        SplineMesh path = (SplineMesh)target;

        //Node A Handle
        Handles.CubeHandleCap(0, path.nodeA.transform.position, path.nodeA.transform.rotation, 4, EventType.MouseDown);
        path.nodeA.transform.position = Handles.PositionHandle(path.nodeA.transform.position, path.nodeA.transform.rotation);
        path.nodeA.transform.rotation = Quaternion.FromToRotation(Vector3.right, path.nodeB.transform.position);

        //Node B Handle
        Handles.CubeHandleCap(0, path.nodeB.transform.position, path.nodeB.transform.rotation, 4, EventType.MouseDown);
        path.nodeB.transform.position = Handles.PositionHandle(path.nodeB.transform.position, path.nodeB.transform.rotation);
        path.nodeB.transform.rotation = Quaternion.FromToRotation(Vector3.right, path.nodeA.transform.position);

        //Node Handles
        path.nodeB.HandleA = Handles.PositionHandle(path.nodeB.HandleA, Quaternion.identity);
        path.nodeA.HandleB = Handles.PositionHandle(path.nodeA.HandleB, Quaternion.identity);

        if (GUI.changed)
        {
            path.GenerateMesh();
            EditorUtility.SetDirty((SplineMesh)target);
        }
    }
}
