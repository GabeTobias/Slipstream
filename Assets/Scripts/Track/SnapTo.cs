using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapTo : MonoBehaviour
{
    public TrackPath path;

    public float time = 0;

    private void OnDrawGizmos()
    {
        (float t, LocRot snapTranform) = path.SnapTo(transform.position);
        Gizmos.DrawCube(snapTranform.position, Vector3.one * 3);
        time = t;
    }
}
