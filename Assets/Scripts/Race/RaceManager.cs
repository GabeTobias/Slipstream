using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public PathCreation.PathCreator path;
    public RacePath racePath;

    public List<RaceTracker> placements;

    public int totalLaps = 4;

    private void Update()
    {
        CalculatePlacements();
    }

    public void ToggleCoastMode() 
    {
        foreach (Vehicle v in FindObjectsOfType<Vehicle>())
        {
            v.ToggleAutoDrive();
        }
    }

    public void CalculatePlacements() 
    {
        List<RaceTracker> vs = new List<RaceTracker>();
        vs.AddRange(FindObjectsOfType<RaceTracker>());
        placements = vs.OrderBy(x => x.time - x.laps).ToList<RaceTracker>();
    }

    public int GetPlacement(RaceTracker r) 
    {
        float tt = r.time;
        int place = 1;

        foreach (RaceTracker tracker in FindObjectsOfType<RaceTracker>()) 
        {
            if (tracker.time > tt && tracker.laps == r.laps) place++;
            if (tracker.laps > r.laps) place++;
        }

        return place;
    }

    public int GetPlacement(int index)
    {
        foreach (RaceTracker tracker in placements)
        {
            if (tracker.seed == index) return tracker.position;
        }

        return 0;
    }

    public float GetTargetSpeed(float time) { return racePath.GetTargetSpeed(time); }

    public float GetLapPercent(Vector3 p) { return racePath.path.path.GetClosestTimeOnPath(p); }
}
