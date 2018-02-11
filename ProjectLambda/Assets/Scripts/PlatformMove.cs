using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMove : MonoBehaviour {
    [DraggablePoint(false)]
    public Vector2[] points;

    public float speed = 5f;

    int index = 0;

	void FixedUpdate () {
        transform.position = Vector2.MoveTowards(transform.position, points[index], speed * Time.deltaTime);
        if (transform.position.x == points[index].x && transform.position.y == points[index].y) {
            index++;
            if (index >= points.Length) {
                index = 0;
            }
        }
	}
}
