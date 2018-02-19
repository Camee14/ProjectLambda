using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
 * sin wave tutorial from: https://answers.unity.com/questions/803434/how-to-make-projectile-to-shoot-in-a-sine-wave-pat.html
 * **/
public class Queen : MonoBehaviour {

    public bool Alert = false;
    public float VisibilityRange = 10f;
    public float Speed = 5f;
    public float Frequency = 3f;
    public float Magnitude = 1.5f;

    //public float MaxChildren = 3;

    GameObject player;
    //GameObject[] children;

    Rigidbody2D rb2d;
    Vector2 target_pos;
    Vector2 true_pos;

    LayerMask floor_mask;
    LayerMask player_mask;

    int facing = -1;
    bool attacking = false;

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        target_pos = true_pos = rb2d.position;

        floor_mask = LayerMask.GetMask("Grappleable");
        player_mask = LayerMask.GetMask("Player");

        player = GameObject.Find("Player");
	}
    void Update() {

    }
    void FixedUpdate() {
        if (!Alert)
        {
            move();
        }
        else if(Alert && !attacking) {
            //setupAttack();
        }

        if (player != null)
        {
            if (!Alert)
            {
                detect();
            }
        }
        else {
            Debug.LogWarning("no player in scene");
        }
    }
	void move() {
        true_pos = Vector2.MoveTowards(true_pos, target_pos, Speed * Time.deltaTime);
        if (true_pos.x == target_pos.x)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.right * facing, Vector2.down, 10f, floor_mask);
            if (hit.collider != null)
            {
                target_pos = hit.point;
                target_pos.y += 3.5f;
            }
            else
            {
                facing *= -1;
            }
        }
        rb2d.position = true_pos;

        Vector2 offsetY = new Vector2(0, Mathf.Sin(Time.time * Frequency) * Magnitude);
        rb2d.position += offsetY;
    }
    void detect() {
        RaycastHit2D hit = Physics2D.Raycast(rb2d.position, ((Vector2)player.transform.position) - rb2d.position, VisibilityRange, player_mask);
        if (hit.collider != null) {
            if (hit.transform.gameObject == player) {
                Alert = true;
                StartCoroutine(setupAttack());
            }
        }
    }
    IEnumerator setupAttack() {
        RaycastHit2D hit_up = Physics2D.Raycast(rb2d.position, Vector2.up, 10f, floor_mask);
        RaycastHit2D hit_down = Physics2D.Raycast(rb2d.position, -Vector2.up, 10f, floor_mask);

        Vector2 ground = rb2d.position;

        float top_dist, bottom_dist;
        top_dist = bottom_dist = 7.5f;

        if (hit_up.collider != null) {
            top_dist = hit_up.distance;
        }
        if (hit_down.collider != null)
        {
            bottom_dist = hit_down.distance;
            ground.y = hit_down.point.y;
        }

        ground.y += (top_dist + bottom_dist) / 2;
        while (rb2d.position != ground)
        {
            rb2d.position = Vector2.MoveTowards(rb2d.position, ground, Speed * 1.5f * Time.deltaTime);
            yield return null;
        }
    }
}
