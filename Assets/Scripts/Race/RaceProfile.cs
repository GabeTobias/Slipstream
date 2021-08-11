using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RacingProfile", menuName = "RacingProfile", order = 1)]
public class RaceProfile : ScriptableObject
{
    [Range(0,1)] public float corneringSpeed    = 1.0f;
    [Range(0,1)] public float bellAcceleration  = 1.0f;
    [Range(0,1)] public float breakingDistance  = 1.0f;
    [Range(0,1)] public float pathAdhearance    = 1.0f;
    [Range(0,1)] public float contactAvoidance  = 1.0f;
}
