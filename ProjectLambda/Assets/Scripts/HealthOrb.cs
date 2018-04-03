using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthOrb : MonoBehaviour {

    public float Speed = 10;
    public float TrackingDelay = 1;
    public int HealthAmmount = 5;

    Transform target;
    Vector2 velocity;
    float timer = 0;
	void Start () {
        velocity = Random.insideUnitCircle * Speed;
        velocity.y = Mathf.Abs(velocity.y);
        target = GameObject.FindGameObjectWithTag("Player").transform;
	}
    void FixedUpdate() {
        if (timer <= TrackingDelay)
        {
            velocity += Physics2D.gravity * Time.deltaTime;
        }
        else {
            Vector2 target_dir = target.position - transform.position;

            Quaternion current_rot = Quaternion.LookRotation(velocity, new Vector2(velocity.y, -velocity.x));
            Quaternion target_rot = Quaternion.LookRotation(target_dir, new Vector2(target_dir.y, -target_dir.x));

            float rot_boost = target_dir.magnitude <= 10 ? 10 - target_dir.magnitude : 1;
            rot_boost += timer - TrackingDelay;

            Quaternion q = Quaternion.RotateTowards(current_rot, target_rot, (120 * rot_boost) * Time.deltaTime);
            float dot = Vector2.Dot(velocity.normalized, target_dir.normalized);
            float boost = 0;
            if (dot >= 0) {
                boost = (12f * dot);
            }
            velocity = q * Vector3.forward * (Speed + boost);
        }
    
        transform.position = (Vector2)transform.position + velocity * Time.deltaTime;
        timer += Time.deltaTime;
    }
    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player" && timer > TrackingDelay) {
            Health h = col.transform.GetComponent<Health>();
            if (h != null) {
                h.apply(HealthAmmount);
            }
            Destroy(gameObject);
        }
    }
}
