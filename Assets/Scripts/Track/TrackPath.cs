using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class TrackPath : MonoBehaviour {

    [SerializeField]
	public List<TrackNode> Nodes = new List<TrackNode>();

    public TrackShape shapeGen;
    public TrackSegment[] segments;

    public LocRot[] vertexTransforms;
    public float[] vertexTimes;

    public LocRot[] Shape;

    public int TrackSize = 9;
    public int TrackWidth = 10;
    public float TrackScale = 1;
    public float CurveStrength;

    public float TrackStart = 0.25f;

    public GUIState GUI;
    public bool showHandles = false;


	void OnDrawGizmos() 
    {
        if (!Selection.Contains(gameObject)) return;

        if (GUI == GUIState.none) return;

        if (GUI == GUIState.FullTrack)
        {
            //Draw Sphere at every node
            Gizmos.color = Color.grey;
            for (int i = 0; i < Nodes.Count; i++)
            {
                Gizmos.DrawSphere(Nodes[i].transform.position, 2);
            }

            //Draw Line between every vertexTime
            for (int i = 1; i < vertexTransforms.Length; i++)
            {
                Gizmos.color = Color.Lerp(Color.green,Color.red, vertexTimes[i]);
                Gizmos.DrawLine(vertexTransforms[i].position + Vector3.up, vertexTransforms[i - 1].position + Vector3.up);
            }

            if (showHandles)
            {
                Gizmos.color = Color.green;
                
                //Draw Line from position to handle
                Gizmos.DrawLine(Nodes[0].HandleA, Nodes[0].transform.position);
                Gizmos.DrawLine(Nodes[0].HandleB, Nodes[0].transform.position);

                //draw cube at handle
                Gizmos.DrawCube(Nodes[0].HandleA, Vector3.one * 2);
                Gizmos.DrawCube(Nodes[0].HandleB, Vector3.one * 2);

                //Repeat for all other handles
                for (int i = 1; i < Nodes.Count; i++)
                {
                    Gizmos.DrawLine(Nodes[i].HandleA, Nodes[i].transform.position);
                    Gizmos.DrawLine(Nodes[i].HandleB, Nodes[i].transform.position);

                    Gizmos.DrawCube(Nodes[i].HandleA, Vector3.one * 2);
                    Gizmos.DrawCube(Nodes[i].HandleB, Vector3.one * 2);
                }
            }
        } else if(GUI == GUIState.Segments){
            
            for (int i = 0; i < segments.Length; i++){

                for (int j = 1; j < segments[i].points.Length; j++){
                    
                    //Switch colors based on Turn Intesegment Speed
                    switch (segments[i].Intensity)
                    {
                        case 0:
                            Gizmos.color = Color.green;
                            break;
                        case 1:
                            Gizmos.color = Color.yellow;
                            break;
                        case 2:
                            Gizmos.color = Color.red;
                            break;
                        case 3:
                            Gizmos.color = Color.magenta;
                            break;
                    }

                    //Draw Path Segment
                    Gizmos.DrawLine(segments[i].points[j].position + Vector3.up, segments[i].points[j-1].position + Vector3.up);
                }

                //Close Segment Loop
                if(i == segments.Length-1)
                    Gizmos.DrawLine(segments[i].points[segments[i].points.Length - 1].position + Vector3.up, segments[0].points[0].position + Vector3.up);
                else
                    Gizmos.DrawLine(segments[i].points[segments[i].points.Length - 1].position + Vector3.up, segments[i + 1].points[0].position + Vector3.up);
            }

        }
    }

    Transform GetPoint(float value) { return transform; }

    TrackNode GetNode(int index) { return Nodes[index]; }

    public LocRot GetTime(float time) 
    {
        int index = 0;
        float lastTime = 1000;
        for (int i = 0; i < vertexTimes.Length; i++) 
        {
            if (Mathf.Abs(vertexTimes[i] - time) < Mathf.Abs(lastTime - time)) 
            {
                lastTime = vertexTimes[i];
                index = i;
            }
        }

        return vertexTransforms[index];
    }

    public (float, LocRot) SnapTo(Vector3 pos) 
    {
        //Find the nearest node
        int closest = 0;
        float cDist = 10000;
        for (int i = 0; i < Nodes.Count; i++) 
        {
            float d = Vector3.Distance(pos, Nodes[i].transform.position);

            if (d < cDist)
            {
                closest = i;
                cDist = d;
            }
        }

        //Find surrounding nodes
        int nodeA = (closest == Nodes.Count-1) ? 0 : closest + 1;
        int nodeB = (closest == 0) ? Nodes.Count - 1 : closest - 1;

        //Get direction to surrounding nodes
        Vector3 dirA = Nodes[nodeA].transform.position - Nodes[closest].transform.position;
        Vector3 dirB = Nodes[nodeB].transform.position - Nodes[closest].transform.position;
        Vector3 dirC = pos - Nodes[closest].transform.position;

        //Get Dot Products for nodes
        float dotA = Vector3.Dot(dirA,dirC);
        float dotB = Vector3.Dot(dirB,dirC);

        //Find the second node
        int second = (dotA > dotB) ? nodeA: closest-1;

        //Find first node
        int start = (second > closest) ? (second * (int)CurveStrength) : (closest * (int)CurveStrength);

        //Correct for edgecase N-0
        if (closest == Nodes.Count - 1 && second == 0)  start = 0;

        //Find nearest vertice between 2 nodes
        int vert = start;
        float vDist = Vector3.Distance(pos, vertexTransforms[vert].position);
        for (int i = start; i < start + CurveStrength; i++) 
        {
            float d = Vector3.Distance(pos, vertexTransforms[i].position);

            if (d < vDist) 
            {
                vert = i;
                vDist = d;
            }
        }

        //Return time of the closest node
        return (vertexTimes[vert], vertexTransforms[vert]);
    }

    public float getDuration() 
    {
        //Create New Time Variable
        vertexTimes = new float[vertexTransforms.Length];
        
        //Store Track distance value
        float duration = 0;

        //Loop threw every point on the track
        for (int i = 1; i < vertexTransforms.Length; i++) 
        {
            //Calculate curent distance from start
            duration += Vector3.Distance(vertexTransforms[i-1].position, vertexTransforms[i].position);
            
            //Store current distance from start
            vertexTimes[i] = duration;
        }

        //Normalize all vertix times
        for (int i = 0; i < vertexTimes.Length; i++)
            vertexTimes[i] /= duration;

        //Offset Track Start
        for (int i = 0; i < vertexTimes.Length; i++)
        {
            vertexTimes[i] += TrackStart;
            if (vertexTimes[i] > 1) vertexTimes[i] -= 1;
        }


        //Return final duration
        return duration;
    }

    public LocRot[] GetPath() { return vertexTransforms; }

    public TrackSegment[] SegmentTrack(LocRot[] dat)
    {    
        List<TrackSegment> Segs = new List<TrackSegment>();
        List<LocRot> activeList = new List<LocRot>();
        int intensity = 0;

        for (int i = 0; i < dat.Length; i++)
        {
            int iNext = (i == dat.Length - 1) ? 0 : i + 1;
            int iLast = (i == 0) ? dat.Length - 1 : i - 1;

            //Convert position into 2D
            Vector2 current = new Vector2(dat[i].position.x,dat[i].position.z);
            Vector2 next = new Vector2(dat[iNext].position.x, dat[iNext].position.z);
            Vector2 last = new Vector2(dat[iLast].position.x, dat[iLast].position.z);

            float angle = VMath.AngleDir(current, next, Vector3.up);
            bool direction = ((angle) >= 0) ? true:false;

            //Check if segments intensity matches
            if(checkInt(last,current,next) == intensity)
            {
                activeList.Add(dat[i]);
            }
            
            //Check if Intensity doesn't match
            else if(checkInt(last, current, next) != intensity)
            {
                //Complete current Segment
                if (activeList.Count > 0)
                {
                    TrackSegment t = new TrackSegment();
                    t.points = activeList.ToArray();
                    t.Intensity = intensity;
                    t.rightTurn = direction;

                    Segs.Add(t);
                }
    
                //Start New List with new node
                activeList.Clear();
                intensity = checkInt(last, current, next);
                activeList.Add(dat[i]);
            }

        }
        return Segs.ToArray();
    }

    public int checkInt(Vector2 last, Vector2 current, Vector2 next) {

        Vector2 to = last-current;
        Vector2 from = current-next;
        float ang = Vector2.Angle(to,from);

        return (int)(ang);
    }

    public LocRot[] GeneratePath()
    {
        GenerateNodes();

        List<LocRot> trans = new List<LocRot>();
        List<LocRot> t2 = new List<LocRot>();

        //TODO: THIS IS THE SAME AS THE GenerateLine() IN RaceLine.cs
        //Generate List of LocRots
        for (int i = 0; i < Nodes.Count; i++){

            for (int j = 0; j < CurveStrength; j++)
            {
                int val = i * (int)CurveStrength + j;
                TrackNode p = (i >= 1) ? Nodes[i - 1] : Nodes[Nodes.Count - 1];
                LocRot t = Nodes[i].GetPoint(p, j / CurveStrength);
                
                trans.Add(t);
            }
        }

        //Add Closing Node
        TrackNode p1 = Nodes[Nodes.Count - 1];
        LocRot t1 = Nodes[0].GetPoint(p1, 0);
        trans.Add(t1);

        t2.Add(trans[0]);
        LocRot last = trans[0];

        //Create new list of evenly spaced points
        for (int i = 1; i < trans.Count; i++){
            float angle =  trans[i].rotation.y - last.rotation.y;
            float dist = Vector3.Distance(trans[i].position,last.position);

            if(Mathf.Abs(angle) < 4 || dist < 0.5f){
                t2.Add(trans[i]);
                last = trans[i];
            }
        }

        vertexTransforms = t2.ToArray();

        //Recalculate track length
        getDuration();

        return t2.ToArray();
    }

    public void GenerateNodes()
    {
        Nodes.Clear();

        for (int i = 0; i < Shape.Length; i++)
        {
            TrackNode s = new TrackNode(Shape[i]);

            if (i == Shape.Length-1)    s.CalculateHandles(Shape[0],        Shape[i-1]              );
            else if(i == 0)             s.CalculateHandles(Shape[i + 1],    Shape[Shape.Length - 1] );
            else                        s.CalculateHandles(Shape[i+1],      Shape[i-1]              );

            Nodes.Add(s);
        }

    }

    [SerializeField]
    public enum GUIState {
        none,
        FullTrack,
        Segments
    }

}

[System.Serializable]
public struct TrackSegment
{
    public int Intensity;
    public bool rightTurn;
    public LocRot[] points;
}