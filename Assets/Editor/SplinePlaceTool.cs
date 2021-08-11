using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplinePlacment)), CanEditMultipleObjects]
public class SplinePlaceTool : Editor
{
    public override void OnInspectorGUI()
    {
        SplinePlacment path = (SplinePlacment)target;

        path.divisions = EditorGUILayout.FloatField("Divisions", path.divisions);
        path.strength = EditorGUILayout.FloatField("Strength", path.strength);
        path.dDistance = EditorGUILayout.FloatField("Double Distance", path.dDistance);
        path.Double = EditorGUILayout.Toggle("Double", path.Double);
        path.FlooSnap = EditorGUILayout.Toggle("Snap To Floor", path.FlooSnap);
        
        EditorGUILayout.Separator();
        
        path.mesh = (Mesh)EditorGUILayout.ObjectField(path.mesh, typeof(Mesh), true);

        EditorGUILayout.Separator();

        path.ObjectMass = EditorGUILayout.FloatField("Mass", path.ObjectMass);
        path.isKinematic = EditorGUILayout.Toggle("Is Kinematic?", path.isKinematic);

        EditorGUILayout.Separator();

        if (GUILayout.Button("Clear Shape"))
        {
            path.ClearShape();
        }

        if (GUILayout.Button("Generate Shape"))
        {
            path.ClearShape();
            path.GenerateShape();
        }

        if (GUILayout.Button("flatten Shape"))
        {
            path.nodeA.transform.position = new Vector3(path.nodeA.transform.position.x, 0, path.nodeA.transform.position.z);
            path.nodeA.HandleA = new Vector3(path.nodeA.HandleA.x, 0, path.nodeA.HandleA.z);
            path.nodeA.HandleB = new Vector3(path.nodeA.HandleB.x, 0, path.nodeA.HandleB.z);

            path.nodeB.transform.position = new Vector3(path.nodeB.transform.position.x, 0, path.nodeB.transform.position.z);
            path.nodeB.HandleA = new Vector3(path.nodeB.HandleA.x, 0, path.nodeB.HandleA.z);
            path.nodeB.HandleB = new Vector3(path.nodeB.HandleB.x, 0, path.nodeB.HandleB.z);

            path.ClearShape();
            path.GenerateShape();
        }


        if (GUI.changed)
        {
            path.ClearShape();
            path.GenerateShape();
            EditorUtility.SetDirty((SplinePlacment)target);
        }
    }

    private void OnSceneGUI()
    {
        SplinePlacment path = (SplinePlacment)target;

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
            path.ClearShape();
            path.GenerateShape();
            EditorUtility.SetDirty((SplinePlacment)target);
        }
    }
}
