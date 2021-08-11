//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackShape : MonoBehaviour {

    [SerializeField]
    public LocRot[] Nodes;

    public TerrainData _data;

	public Vector3 RandomCircle ( Vector3 center ,float radius, float ang){
		Vector3 pos;
		pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
		pos.z = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.y = 0;

		return pos;
	}

    public LocRot[] GetPath() { return Nodes; }

    public LocRot[] GenerateShape(int size)
    {
        LocRot[] Points = new LocRot[size];

		for(int i = 0; i < size; i++)
        {
            //Create LocRot from Random Circle
            Points[i] = new LocRot();
            Points[i].position = RandomCircle(Vector3.zero, UnityEngine.Random.Range(10,100),i*(360/size));
            Points[i].rotation = Quaternion.FromToRotation(Vector3.right, Points[i].position);

            //Regenerate if point is too far from previous point
            if (i == 0) continue;
            if (i == size - 1 && Vector3.Distance(Points[i].position, Points[0].position) < 40) i--;
            if (Vector3.Distance(Points[i].position, Points[i - 1].position) < 40) i--;
		}

        Nodes = Points;
		return Points;
	}
}
