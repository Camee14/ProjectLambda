using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour {
    [DraggablePoint(false)]
    public Vector2[] points;

    public float speed = 5f;

    Rigidbody2D rb2d;

    int index = 0;


	// Use this for initialization
	void Start () {
        rb2d = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        rb2d.position = Vector2.MoveTowards(transform.position, points[index], speed * Time.deltaTime);
        if (transform.position.x == points[index].x && transform.position.y == points[index].y)
        {
            index++;
            if (index >= points.Length)
            {
                index = 0;
            }
        }
    }
}
