using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTracker : MonoBehaviour
{
    public float time = float.NaN;
    public int position;
    public int laps = 1;
    public int seed;

    ///////////////////////////////////////////////////////////////

    [HideInInspector]
    public Vehicle vehicle;

    [HideInInspector]
    public RaceManager race;

    [HideInInspector]
    public PathCreation.VertexPath trackPath;
    
    [HideInInspector]
    public List<float> lapTimes;

    ///////////////////////////////////////////////////////////////

    private bool RaceOver = false;
    private float startTime;
    private float lapTime;

    // Start is called before the first frame update
    void Start()
    {
        //Get Attached Vehicle
        vehicle = GetComponent<Vehicle>();
        
        //Find the track and the racepath
        race = FindObjectOfType<RaceManager>();
        trackPath = FindObjectOfType<RacePath>().path.path;

        //Set Start Time
        startTime = trackPath.GetClosestTimeOnPath(transform.position);

        //TODO: Adjust for start countdown
        //Set Lap Time
        lapTime = Time.time;

        //Reset current lap on play
        laps = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //Set Current Race Position
        position = race.GetPlacement(this);

        //Find the next potential time & add a lap if crossing 0
        float nextTime = trackPath.GetClosestTimeOnPath(transform.position);
        if (time < startTime && nextTime > startTime)
        {
            laps++;
            lapTimes.Add(getLapTime());
            lapTime = Time.time;
        }

        //Set the current time
        time = nextTime;

        //Toggle EndOfRace state after last lap
        if (laps > race.totalLaps && !RaceOver)
        {
            vehicle.ToggleAutoDrive();
            RaceOver = true;
        }
    }

    public float getLapTime() 
    {
        return Time.time - lapTime;
    }
}
