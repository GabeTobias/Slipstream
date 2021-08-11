using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LoopData)), CanEditMultipleObjects]
public class LoopEditor : Editor {

    bool NameBox;
    string Name;

    int current = 0;

	private void OnEnable() {
        LoopData loop = (LoopData)target;

        if(!loop.LoadedNodes) loop.LoadTransforms();
	}

	public override void OnInspectorGUI(){

        LoopData loop = (LoopData)target;

        if (!loop.LoadedNodes) loop.LoadTransforms();

        //Values Section
        loop.Stretch = EditorGUILayout.FloatField("Stretch:", loop.Stretch);
        loop.Nodes[current].position = EditorGUILayout.Vector3Field("Current:", loop.Nodes[current].position);

        EditorGUILayout.Space();


        ////Connection Type Section
        //EditorGUILayout.BeginHorizontal();

        //if (GUILayout.Button("Direct"))
        //{
            
        //}

        //if (GUILayout.Button("Stretch"))
        //{
            
        //}

        //EditorGUILayout.EndHorizontal();

        //EditorGUILayout.Space();



        // Point Handling Section
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Insert Point "))
        {
            
        }

        if (GUILayout.Button("Remove Point"))
        {
            
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();





        //Export and Saving Options
        if (GUILayout.Button("Load"))
        {
            
        }

        if (GUILayout.Button("Default"))
        {
            loop.SetDefaults();
        }


        //Export Option
        if(NameBox){
            Name = EditorGUILayout.TextField("Name: ", Name);

            if (GUILayout.Button("Export")){
                CreateObj(Name,loop);
            }

        } else if (GUILayout.Button("Export"))
        {
            NameBox = true;
        }
    }

    void OnSceneGUI() {
            
        LoopData loop = (LoopData)target;

        if (!loop.LoadedNodes) loop.LoadTransforms();

        //Draws Connections between nodes
        for (int i = 0; i < loop.Nodes.Length; i++){
            int last = (i > 0) ? i - 1 : loop.Nodes.Length - 1;

            Handles.DrawLine(loop.Nodes[i].position,loop.Nodes[last].position);
        }

        SceneView.RepaintAll();
    }

    void CreateObj(string _name, LoopData loop){
        TrackLoop t = CreateInstance<TrackLoop>() as TrackLoop;

        t.name = _name;

        //t.SetVertices(loop.vertices);

        AssetDatabase.CreateAsset(t,"Assets/Scriptables/" + _name + ".asset");
        NameBox = false;
    }
}
