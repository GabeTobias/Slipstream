using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VMath
{
    static Vector2 FindIntersection(Vector2 s1, Vector2 e1, Vector2 s2, Vector2 e2)
    {
        float a1 = e1.y - s1.y;
        float b1 = s1.x - e1.x;
        float c1 = a1 * s1.x + b1 * s1.y;

        float a2 = e2.y - s2.y;
        float b2 = s2.x - e2.x;
        float c2 = a2 * s2.x + b2 * s2.y;

        float delta = a1 * b2 - a2 * b1;
        //If lines are parallel, the result will be (NaN, NaN).
        return delta == 0 ? new Vector2(float.NaN, float.NaN)
            : new Vector2((b2 * c1 - b1 * c2) / delta, (a1 * c2 - a2 * c1) / delta);
    }

    static Vector3 FindIntersection(Vector3 s1, Vector3 e1, Vector3 s2, Vector3 e2)
    {
        Vector2 v = FindIntersection(
            new Vector2(s1.x, s1.z), 
            new Vector2(e1.x, e1.z), 
            new Vector2(s2.x, s2.z), 
            new Vector2(e2.x, e2.z) 
        );

        return new Vector3(v.x, 0, v.y);
    }

    public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
    {
        return x * Vector3.Normalize(B - A) + A;
    }

    public static Vector3 GetPoint(Vector3[] pts, float t)
    {
        float omt = 1f - t;
        float omt2 = omt * omt;
        float t2 = t * t;
        return pts[0] * (omt2 * omt) +
            pts[1] * (3f * omt2 * t) +
            pts[2] * (3f * omt * t2) +
            pts[3] * (t2 * t);
    }

    public static Vector3 GetTangent(Vector3[] pts, float t)
    {
        float omt = 1f - t;
        float omt2 = omt * omt;
        float t2 = t * t;
        Vector3 tangent =
            pts[0] * (-omt2) +
            pts[1] * (3 * omt2 - 2 * omt) +
            pts[2] * (-3 * t2 + 2 * t) +
            pts[3] * (t2);
        return tangent.normalized;
    }

    public static Vector3 GetNormal3D(Vector3[] pts, float t, Vector3 up)
    {
        Vector3 tng = GetTangent(pts, t);
        Vector3 binormal = Vector3.Cross(up, tng).normalized;
        return Vector3.Cross(tng, binormal);
    }

    public static Quaternion GetOrientation3D(Vector3[] pts, float t, Vector3 up)
    {
        Vector3 tng = GetTangent(pts, t);
        Vector3 nrm = GetNormal3D(pts, t, up);
        return Quaternion.LookRotation(tng, nrm);
    }

    //returns -1 when to the left, 1 to the right, and 0 for forward/backward
    public static float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0f)
            return 1.0f;
        else if (dir < 0.0f)
            return -1.0f;
        else
            return 0.0f;

    }
}

public class Utils : MonoBehaviour
{
    public static Transform CreateTransform()
    {
        return new GameObject().transform;
    }

    public static Transform CreateTransform(Vector3 Pos, Quaternion Rot)
    {
        Transform t = new GameObject().transform;

        t.position = Pos;
        t.rotation = Rot;

        return t;
    }

    public static Vector3 Average(Vector3 a, Vector3 b)
    {
        return new Vector3(
            (a.x + b.x) / 2f,
            (a.y + b.y) / 2f,
            (a.z + b.z) / 2f
        );
    }
}

[System.Serializable]
public class LocRot
{
    [SerializeField]
    public Vector3 position = new Vector3();
    
    [SerializeField]
    public Quaternion rotation = Quaternion.identity;

    public LocRot() { }
    public LocRot(Vector3 loc, Quaternion rot)
    {
        position = loc;
        rotation = rot;
    }

    public Vector3 LocalToWorld(Vector3 point)
    {
        return position + (rotation * point);
    }

    public Vector3 right()
    {
        return rotation * Vector3.right;
    }
    public Vector3 forward()
    {
        return rotation * Vector3.forward;
    }

    public Transform toTransform()
    {
        return Utils.CreateTransform(position, rotation);
    }
}