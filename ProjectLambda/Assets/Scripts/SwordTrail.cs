using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTrail : MonoBehaviour {

    LineRenderer line;
    List<Vector3> points;

	void Awake () {
        line = GetComponent<LineRenderer>();
        line.positionCount = 50;

        points = new List<Vector3>();
	}

	void Update () {
        points.Insert(0, transform.position);
        while(points.Count > 50)
        {
            points.RemoveAt(points.Count - 1);
        }
        line.SetPositions(points.ToArray());
	}
}
