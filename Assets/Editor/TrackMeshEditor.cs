using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackMesh)), CanEditMultipleObjects]
public class TrackMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TrackMesh mesh = (TrackMesh)target;

        mesh.TextureDistance = EditorGUILayout.FloatField("Texture Distance", mesh.TextureDistance);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Mesh"))
        {
            mesh.Generate();
            EditorUtility.SetDirty((TrackMesh)target);
        }

        if (GUILayout.Button("Clear Mesh"))
        {
            mesh.Clear();
            EditorUtility.SetDirty((TrackMesh)target);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty((TrackMesh)target);
            mesh.Generate();
        }
    }
}
