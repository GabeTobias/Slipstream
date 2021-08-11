using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode, CanEditMultipleObjects]
public class PathNode : MonoBehaviour
{

    [Header("Properties")]
    [Range(0,0.1f)]  public float TurningPoint = 0.1f;
    [Range(0, 150f)] public float Speed;
    
    ///////////////////////////////////////////////////////////////

    [Header("DEBUG")]
    public static bool hideInGame = true;
    public float Time = -1;

    ///////////////////////////////////////////////////////////////

    private RacePath racePath;

    ///////////////////////////////////////////////////////////////

    private void OnDrawGizmos()
    {
        //Find Path if null
        if (racePath == null) racePath = FindObjectOfType<RacePath>();

        //Add Node to path
        if (!racePath.Nodes.Contains(this)) racePath.Nodes.Add(this);

        //Hide in Game
        if (Application.isPlaying && hideInGame) return;

        //Draw Handle Outline
        Gizmos.color = new Color(1,1,1,0.75f);
        Gizmos.DrawWireCube(transform.position + new Vector3(0,2.5f,0), Vector3.one * 5f);

        //Draw Handle Fill (This is so it can be selected in editor)
        Gizmos.color = new Color(1, 1, 1, 0.1f);
        Gizmos.DrawCube(transform.position + new Vector3(0,2.5f,0), Vector3.one * 5f);

        //Adjust font style for overlay text
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = new Color(1, 1, 1, 0.5f);

        //Change Color on Selection
        if (Selection.activeObject == gameObject) style.normal.textColor = Color.white;

        //Draw Speed & Time Overlay
        Handles.Label(
            transform.position + new Vector3(0, 1, 0),
            "Node: " + GetTrackTime().ToString("N2") + "\n" + "Speed: " + Speed.ToString("N1"),
            style
        );

        //Get the Current Track time
        Time = GetTrackTime();
        
        //Before Turning Point
        Vector3 turn1 = racePath.path.path.GetPointAtTime(Time-TurningPoint);
        Gizmos.color = new Color(1, 0, 0, 0.1f);
        Gizmos.DrawCube(turn1 + new Vector3(0, 2.5f, 0), Vector3.one * 5f);

        //After Turning Point
        Vector3 turn2 = racePath.path.path.GetPointAtTime(Time + TurningPoint);
        Gizmos.color = new Color(1, 0, 0, 0.1f);
        Gizmos.DrawCube(turn2 + new Vector3(0, 2.5f, 0), Vector3.one * 5f);

    }

    public float GetTrackTime() 
    {
        //Check if the transform has moved
        if (Time == -1 || transform.hasChanged)
        {
            //Update the time value
            Time = racePath.path.path.GetClosestTimeOnPath(transform.position);
            
            //Set Transform flag
            transform.hasChanged = false;
        }
    
        return Time;
    }
}
