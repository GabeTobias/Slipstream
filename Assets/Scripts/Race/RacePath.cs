using Malee.List;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathCreation.PathCreator))] 
[ExecuteInEditMode]
public class RacePath : MonoBehaviour
{
    [Header("References")]
    public PathCreation.PathCreator path;

    ///////////////////////////////////////////////////////////////

    [Header("Render Properties")]
    public bool showLine;
    public bool distanceAlpha;

    ///////////////////////////////////////////////////////////////

    [Header("Node Listing")]
    [Reorderable]  public NodeArray Nodes;

    ///////////////////////////////////////////////////////////////

    private Vehicle vehicle;
    private RaceTracker tracker;
    private Material lineMaterial;

    ///////////////////////////////////////////////////////////////

    private void Awake()
    {
        //Find the vehicle flaged as player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        //Get Components
        vehicle = player.GetComponent<Vehicle>();
        tracker = player.GetComponent<RaceTracker>();

        //Create New Material
        GenerateMaterial();
    }

    private void Update()
    {
        //Update the Bezier Path
        for (int i = 0; i < Nodes.Count; i++)
        {
            //Set only the Anchor positions by skipping every 2 elements
            path.bezierPath.SetPoint(i * 3, transform.worldToLocalMatrix * (Nodes[i].transform.position));
        }

        //Set the controls to autogenerate
        path.bezierPath.ControlPointMode = PathCreation.BezierPath.ControlMode.Automatic;

        //Update the Control Points
        path.bezierPath.AutoSetAllControlPoints();

        //Update the Path
        path.TriggerPathUpdate();
    }

    private void OnRenderObject()
    {
        if(!showLine) return;

        //Double check that there is a material
        GenerateMaterial();

        //Find Percent Difference Between Speeds
        float p = 0;
        if (vehicle.rigidbody != null) p = (vehicle.rigidbody.velocity.magnitude - GetVisualSpeed(tracker.time)) / 35f;

        for (int i = 0; i < 200; i++)
        {
            //Find the current and next times
            float time = i / 200f;
            float timePlus = (i + 1) / 200f;

            //Get the positions from the path
            Vector3 current = path.path.GetPointAtTime(time) + new Vector3(0, 0.5f, 0);
            Vector3 next = path.path.GetPointAtTime(timePlus) + new Vector3(0, 0.5f, 0);
            
            //Calculate arrow direction from positions
            Vector3 direction = (next - current).normalized;

            //Get opacity from sined player distance
            float playerDistance = Vector3.Distance(current, vehicle.transform.position + (vehicle.transform.forward*15));
            float opacity = 1f - Mathf.Clamp(playerDistance / 70f, 0,1);

            //Create Color from speed and opacity
            Color col = Color.Lerp(Color.cyan, Color.red, p);
            if(distanceAlpha) col.a = opacity * 0.5f;

            //Update Line Material
            lineMaterial.SetPass(0);

            //Draw Triangle
            GL.Begin(GL.TRIANGLES);

            GL.Color(col);

            //Plus Direction goes clockwise
            GL.Vertex(current + direction);
            GL.Vertex(current + (Quaternion.Euler(0, 90, 0) * direction));
            GL.Vertex(current + (Quaternion.Euler(0, -90, 0) * direction));

            GL.End();
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void GenerateMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;

            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 1);
        }
    }

    public float GetTargetSpeed(float Time, float bScale = 1f, float bTime = 0f)
    {
        //Get Surrounding Nodes
        (PathNode pPrev, PathNode pNext) = GetSurroundingNodes(Time);

        //Calculate Time Differences
        float nodeTimeDiff = (pNext.Time - pNext.TurningPoint - bTime) - (pPrev.Time + pPrev.TurningPoint + bTime);
        float targetTimeDiff = (pNext.Time - pNext.TurningPoint) - Time;

        //Calculate Percentage between Points
        float percentage = targetTimeDiff / nodeTimeDiff;

        //Calculate bell acceleration
        float bell = Mathf.Sin(Mathf.Min((1f - percentage) * 3.14f, 3.14f)) * Mathf.Abs(pNext.Speed - pPrev.Speed);

        //Calculate Speed Based on Percentage
        float speed = Mathf.Lerp(pPrev.Speed, pNext.Speed, 1f - percentage) + Mathf.Clamp(bell * bScale, 0,200);

        return speed;
    }

    public float GetVisualSpeed(float Time)
    {
        //Get Surrounding Nodes
        (PathNode pPrev, PathNode pNext) = GetSurroundingNodes(Time);

        //Calculate Time Differences
        float nodeTimeDiff = pNext.Time - pPrev.Time;
        float targetTimeDiff = (pNext.Time - pNext.TurningPoint) - Time;

        //Calculate Percentage between Points
        float percentage = targetTimeDiff / nodeTimeDiff;

        //Calculate Speed Based on Percentage
        float speed = Mathf.Lerp(pPrev.Speed, pNext.Speed, 1f - percentage);

        return speed;
    }

    private (PathNode, PathNode) GetSurroundingNodes(float time) 
    {
        //Find Next Node
        int next = 0;
        for (int i = 1; i <= Nodes.Count; i++)
        {
            //Calculate index
            int index = i % (Nodes.Count - 1);

            //Find first node past given time
            if (Nodes[index].Time > time)
            {
                next = i;
                break;
            }
        }

        //Get Surrounding Nodes
        PathNode pNext = Nodes[next];
        PathNode pPrev = Nodes[(next == 0) ? Nodes.Count - 1 : next - 1];

        return (pPrev,pNext);
    }

}

[System.Serializable]
public class NodeArray : ReorderableArray<PathNode> {}