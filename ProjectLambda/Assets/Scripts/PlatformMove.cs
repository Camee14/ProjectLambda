using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMove : MonoBehaviour {
    [DraggablePoint(false)]
    public Vector2[] points;

    public float speed = 5f;

    int index = 0;

    Vector2 velocity;
    public Vector2 getVelocity() {
        return velocity;
    }
	void FixedUpdate () {
        Vector2 next_pos = Vector2.MoveTowards(transform.position, points[index], speed * Time.deltaTime);
        velocity = next_pos - (Vector2)transform.position;
        transform.position = next_pos;
        if (transform.position.x == points[index].x && transform.position.y == points[index].y) {
            index++;
            if (index >= points.Length) {
                index = 0;
            }
        }
	}
}
