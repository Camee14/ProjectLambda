using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public bool DrawDebug;

    LayerMask grapple_mask;

    Transform grapple_target;
    Vector2 grapple_target_point;

    List<Vector2> collision_points;

    float max_wire_length = 6f;
    bool has_grapple_target;
    bool grapple_connected;

    public bool isGrappleConnected{
        get { return grapple_connected; }
    }
    public float MaxLength {
        get { return max_wire_length; }
    }
    public Vector2 GrapplePoint {
        get { return grapple_target.TransformPoint(grapple_target_point); }
    }

    void Start () {
        collision_points = new List<Vector2>();

        grapple_mask = LayerMask.GetMask("Grappleable");
        has_grapple_target = false;
        grapple_connected = false;
    }

    void Update()
    {
        
        if (Input.GetButtonDown("Fire1") && has_grapple_target)
        {
            grapple_connected = true;
        }
        if (Input.GetButtonDown("Fire2") && grapple_connected)
        {
            grapple_connected = false;
        }
    }

    void FixedUpdate () {
        Vector2 pos = transform.position;

        if (grapple_connected) {
            Vector2 world_point = grapple_target.TransformPoint(grapple_target_point);

            RaycastHit2D hit = Physics2D.Raycast(world_point + (pos - world_point).normalized * 0.001f, pos - world_point, 200f, grapple_mask);
            if (hit.collider != null)
            {
                //collision_points.Add(hit.point);
            }
            
        }
        else
        {
            Vector2 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, mouse_pos - pos, 200f, grapple_mask);
            if (hit.collider != null && !grapple_connected)
            {
                has_grapple_target = true;
                grapple_target = hit.transform;
                grapple_target_point = grapple_target.InverseTransformPoint(hit.point);

            }
            else
            {
                has_grapple_target = false;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!DrawDebug)
        {
            return;
        }
        if (grapple_connected)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, grapple_target.TransformPoint(grapple_target_point));
        }
        else if (has_grapple_target)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, grapple_target.TransformPoint(grapple_target_point));
        }
        if (collision_points != null)
        {
            foreach (Vector2 point in collision_points)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(point, 0.1f);
            }
        }
    }
}
