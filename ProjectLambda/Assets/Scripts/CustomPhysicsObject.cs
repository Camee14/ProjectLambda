using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**Tutorial from: https://unity3d.com/learn/tutorials/topics/2d-game-creation/intro-and-session-goals?playlist=17093
 * 
 * **/
public class CustomPhysicsObject : MonoBehaviour {
    const float MIN_MOVE_DISTANCE = 0.001f;
    const float SHELL_RADIUS = 0.01f;

    public float Mass = 1f;
    public float min_ground_normal_y = 0.65f;
    public float MaxVelocity = 10f;
    public bool DrawDebug;

    protected Rigidbody2D rb2d;

    Vector2 input;
    Vector2 sum_of_forces;
    Vector2 velocity;
    Vector2 surface_normal;
    Vector2 tether_point;
    ContactFilter2D contact_filter;
    RaycastHit2D[] hits;
    List<RaycastHit2D> hit_list;

    float max_tether_length;
    bool is_tethered;
    bool is_grounded;

    Vector2 db_result_force;
    Vector2 db_velocity;
    Vector2 db_tether_force;
    Vector2 db_vel_proj;

    public Vector2 Velocity {
        get { return velocity; }
        set { velocity = value; }
    }

    public bool isGrounded{
        get { return is_grounded; }
    }

    public void setTetherPoint(Vector2 point, float max) {
        tether_point = point;
        max_tether_length = max;
        is_tethered = true;
    }
    public void releaseTether() {
        db_result_force = Vector2.zero;
        db_tether_force = Vector2.zero;
        //db_velocity = Vector2.zero;
        db_vel_proj = Vector2.zero;

        is_tethered = false;
    }

    protected virtual Vector2 setInputAcceleration() {
        return Vector2.zero;
    }

    protected virtual void update(){
        
    }

    protected virtual void fixedUpdate(){

    }

    void Awake() {
        hits = new RaycastHit2D[16];
        hit_list = new List<RaycastHit2D>(16);

        rb2d = GetComponent<Rigidbody2D>();

        contact_filter.useTriggers = false;
        contact_filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contact_filter.useLayerMask = true;

        is_tethered = false;
    }

    void Update() {
        input = setInputAcceleration();

        update();
    }

	void FixedUpdate () {
        sum_of_forces = Physics2D.gravity;

        if (is_tethered) {
            Vector2 tether = tether_point - rb2d.position;
            if (tether.magnitude > max_tether_length)
            {
                float angle = Vector2.Angle(Physics2D.gravity, rb2d.position - tether_point) * Mathf.Deg2Rad;
                float tangent_g = Physics2D.gravity.magnitude * Mass * Mathf.Sin(angle);

                Vector2 tangent = new Vector2(tether.y, -tether.x);

                rb2d.position = tether_point - tether.normalized * Mathf.Lerp(tether.magnitude, max_tether_length, 5f * Time.deltaTime); //restrain movement to radius

                Vector2 g_proj = (Vector2.Dot(Physics2D.gravity, tangent) / Vector2.Dot(tangent, tangent)) * tangent; //re-direct gravity along tangent
                g_proj = g_proj.normalized * tangent_g;

                Vector2 vel_proj = (Vector2.Dot(velocity, tangent) / Vector2.Dot(tangent, tangent)) * tangent; //re-direct velocity along tangent

                sum_of_forces = g_proj;
                velocity = vel_proj;
            }
        }

        velocity += sum_of_forces * Mass * Time.deltaTime;

        if (is_tethered && !is_grounded)
        {
            velocity += input * Mass * Time.deltaTime;
        }
        else
        {
            velocity.x = input.x;
        }

        db_velocity = velocity;
        db_result_force = sum_of_forces;

        is_grounded = false;

        Vector2 x_movement = new Vector2(surface_normal.y, -surface_normal.x); //surface tangent

        Vector2 delta_pos = velocity * Time.deltaTime;

        Vector2 dir = x_movement * delta_pos.x;
        move(dir, false);

        dir = Vector2.up * delta_pos.y;
        move(dir, true);

        fixedUpdate();
    }

    void move(Vector2 dir, bool move_y) {
        float dist = dir.magnitude;
        if (dist >= MIN_MOVE_DISTANCE) {
            int count = rb2d.Cast(dir, contact_filter, hits, dist + SHELL_RADIUS);
            hit_list.Clear();

            for (int i = 0; i < count; i++) {
                hit_list.Add(hits[i]);
            }

            foreach (RaycastHit2D hit in hit_list) {
                Vector2 normal = hit.normal;
                if (normal.y > min_ground_normal_y) {
                    is_grounded = true;
                    if (move_y) {
                        surface_normal = normal;
                        normal.x = 0;
                    }
                }
                float projection = Vector2.Dot(velocity, normal);
                if (projection < 0) {
                    velocity = velocity - normal * projection;
                }

                float mod_dist = hit.distance - SHELL_RADIUS;
                dist = mod_dist < dist ? mod_dist : dist;
            }
        }
        Vector2 next_pos = rb2d.position + dir.normalized * dist;
        if (is_tethered)
        {
            Vector2 tether = tether_point - next_pos;
            if (tether.magnitude > max_tether_length)
            {
                //rb2d.position = tether_point - tether.normalized * Mathf.Lerp(tether.magnitude, max_tether_length, 5f * Time.deltaTime);
                //return;
            }
        }
        rb2d.position = next_pos;
    }

    void OnDrawGizmos() {
        if (rb2d != null && DrawDebug)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(rb2d.position, rb2d.position + Physics2D.gravity);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(rb2d.position, rb2d.position + db_tether_force);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(rb2d.position, rb2d.position + db_result_force);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(rb2d.position, rb2d.position + db_velocity);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(rb2d.position, rb2d.position + db_vel_proj);
        }
    }
}
