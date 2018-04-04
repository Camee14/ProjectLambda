using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**Tutorial from: https://unity3d.com/learn/tutorials/topics/2d-game-creation/intro-and-session-goals?playlist=17093
 * 
 * **/
public class CustomPhysicsObject : MonoBehaviour {
    const float MIN_MOVE_DISTANCE = 0.001f;
    const float SHELL_RADIUS = 0.02f;

    public float Mass = 1f;
    public float min_ground_normal_y = 0.65f;
    public float MaxVelocity = 10f;
    bool override_auto_facing = false;
    bool override_velx = true;
    bool override_gravity = false;
    bool override_physics = false;
    public bool DrawDebug;

    protected Rigidbody2D rb2d;

    Vector2 input;
    Vector2 sum_of_forces;
    Vector2 velocity;
    Vector2 external_forces;
    Vector2 surface_normal;
    Vector2 tether_point;
    ContactFilter2D contact_filter;
    RaycastHit2D[] hits;
    List<RaycastHit2D> hit_list;

    float max_tether_length;
    float facing = -1f;
    bool is_tethered;
    bool is_grounded;
    bool is_sliding;

    //Vector2 db_result_force;
    //Vector2 db_velocity;
    //Vector2 db_tether_force;
    //Vector2 db_vel_proj;
    //List<RaycastHit2D> db_hit_list;

    public Vector2 Velocity {
        get { return velocity; }
        set { velocity = value; }
    }
    public Vector2 ExternalForces
    {
        get { return external_forces; }
        set { external_forces = value; }
    }

    public float Facing {
        get { return facing; }
        set { facing = value; }
    }

    public bool IsGrounded{
        get { return is_grounded; }
    }
    public bool OverrideVelocityX {
        get { return override_velx; }
        set { override_velx = value; }
    }
    public bool OverrideGravity {
        get { return override_gravity; }
        set { override_gravity = value; }
    }
    public bool OverrideAutoFacing {
        get { return override_auto_facing; }
        set { override_auto_facing = value; }
    }

    public void setTetherPoint(Vector2 point, float max) {
        tether_point = point;
        max_tether_length = max;
        is_tethered = true;
    }
    public void releaseTether() {
        is_tethered = false;
    }

    public void OverridePhysics(bool over_ride) {
        override_physics = over_ride;
        if (over_ride) {
            velocity = Vector2.zero;
        }
    }

    protected virtual Vector2 setInputAcceleration() {
        return Vector2.zero;
    }
    protected virtual void awake(){}
    protected virtual void update(){}
    protected virtual void fixedUpdate(){}
    protected virtual void onDrawGizmos() {}

    void Awake() {
        hits = new RaycastHit2D[16];
        hit_list = new List<RaycastHit2D>(16);
        //db_hit_list = new List<RaycastHit2D>(16);

        rb2d = GetComponent<Rigidbody2D>();

        contact_filter.useTriggers = false;
        contact_filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contact_filter.useLayerMask = true;

        is_tethered = false;

        awake();
    }

    void Update() {
        update();
        input = setInputAcceleration();
        if (!OverrideAutoFacing && input.x != 0)
        {
            facing = input.x > 0f ? 1f : -1f;
        }
    }

	void FixedUpdate () {
        if (override_physics) {
            return;
        }
        sum_of_forces = (override_gravity ? Vector2.zero : Physics2D.gravity);

        if (is_tethered) {
            Vector2 tether = tether_point - rb2d.position;

            if (tether.magnitude > max_tether_length)
            {
                float grav_angle = Vector2.Angle(Physics2D.gravity, rb2d.position - tether_point) * Mathf.Deg2Rad;
                float tangent_g = Physics2D.gravity.magnitude * Mass * Mathf.Sin(grav_angle);

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

        /*if (is_sliding && velocity.y < 0)
        {
            velocity = Vector2.ClampMagnitude(velocity, 4);
        }*/

        if (override_velx)
        {
            if ((is_tethered && !is_grounded) || is_sliding)
            {

                velocity += input * Mass * Time.deltaTime;
            }
            else
            {
                velocity.x = input.x;
            }
        }
        else {
            if (is_grounded) {
                velocity.x = velocity.x * 0.95f;
            }
        }

        //correct surface normal when transitioning from slope to grapple;
        if (!is_grounded)
        {
            surface_normal = Vector2.up;
        }

        //db_velocity = velocity;
        //db_result_force = sum_of_forces;

        is_grounded = false;
        is_sliding = false;

        Vector2 x_movement = new Vector2(surface_normal.y, -surface_normal.x); //surface tangent

        Vector2 delta_pos = velocity * Time.deltaTime;

        //db_hit_list.Clear();

        Vector2 dir = x_movement * delta_pos.x;
        move(dir, false);

        dir = Vector2.up * delta_pos.y;
        move(dir, true);

        resolveOverlaps(dir);

        fixedUpdate();
    }

    void move(Vector2 dir, bool move_y) {
        float dist = dir.magnitude;
        Vector2 offset = Vector2.zero;
        if (dist >= MIN_MOVE_DISTANCE) {
            int count = rb2d.Cast(dir, contact_filter, hits, dist + SHELL_RADIUS);

            hit_list.Clear();
            for (int i = 0; i < count; i++) {
                hit_list.Add(hits[i]);
                //db_hit_list.Add(hits[i]);
            }

            foreach (RaycastHit2D hit in hit_list) {
                Vector2 normal = hit.normal;
                if (normal.y > min_ground_normal_y)
                {
                    is_grounded = true;
                    if (move_y)
                    {
                        surface_normal = normal;
                        normal.x = 0;
                    }
                }
                else {
                    is_sliding = true;
                }

                if (hit.collider.tag == "MovingTerrain")
                {
                    PlatformMove moving_p = hit.collider.transform.GetComponent<PlatformMove>();
                    if (moving_p != null)
                    {
                        offset += moving_p.getVelocity();
                    }
                }

                //slows down velocity when object is hit
                float projection = Vector2.Dot(velocity, normal);
                if (projection < 0) {
                    velocity = velocity - normal * projection;
                }

                float mod_dist = hit.distance - SHELL_RADIUS;
                dist = mod_dist < dist ? mod_dist : dist;
            }
        }
        Vector2 next_pos = rb2d.position + (dir.normalized * dist) + offset;
        rb2d.position = next_pos;
    }

    void resolveOverlaps(Vector2 dir) {
        if (dir.magnitude < MIN_MOVE_DISTANCE)
        {
            return;
        }
            int count = 0;
        int loops = 0;
        do
        {
            count = rb2d.Cast(Vector2.zero, contact_filter, hits, SHELL_RADIUS);
            hit_list.Clear();
            for (int i = 0; i < count; i++)
            {
                hit_list.Add(hits[i]);
                //db_hit_list.Add(hits[i]);
            }

            foreach (RaycastHit2D hit in hit_list)
            {
                Vector2 offset;
                offset.x = hit.normal.x;// * (0.5f - Mathf.Abs(transform.InverseTransformPoint(hit.point).x) + SHELL_RADIUS);
                offset.y = hit.normal.y;// * (0.5f - Mathf.Abs(transform.InverseTransformPoint(hit.point).y) + SHELL_RADIUS);

                rb2d.position += offset * 0.05f;
            }
            loops++;
        } while (count != 0 && loops < 30);
    }

    void OnDrawGizmos() {
        if (rb2d != null && DrawDebug)
        {
           // Gizmos.color = Color.red;
           // Gizmos.DrawLine(rb2d.position, rb2d.position + Physics2D.gravity);

            //Gizmos.color = Color.blue;
           // Gizmos.DrawLine(rb2d.position, rb2d.position + db_tether_force);

           // Gizmos.color = Color.green;
           // Gizmos.DrawLine(rb2d.position, rb2d.position + db_result_force);

           // Gizmos.color = Color.yellow;
           // Gizmos.DrawLine(rb2d.position, rb2d.position + db_velocity);

            //Gizmos.color = Color.magenta;
            //Gizmos.DrawLine(rb2d.position, rb2d.position + db_vel_proj);

            
            /*foreach (RaycastHit2D hit in db_hit_list)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(hit.collider.transform.position, hit.collider.bounds.extents * 2f);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(hit.point, hit.point + hit.normal);
            }*/

            //Gizmos.color = IsGrounded ? Color.green : Color.red;
           // Gizmos.DrawCube(transform.position, new Vector3(0.4f, 0.4f, 1f));
        }

        onDrawGizmos();
    }
}
