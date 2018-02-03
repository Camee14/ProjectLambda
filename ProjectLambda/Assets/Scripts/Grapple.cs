using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public float MaxWireLength = 20f;
    public bool DrawDebug;

    CircleCollider2D range_sensor;
    LineRenderer line;

    LayerMask grapple_mask;

    Transform grapple_target;
    Vector2 grapple_target_point;

    Dictionary<int, Collider2D>platforms;

    float current_wire_length = 6f;
    bool has_grapple_target;
    bool grapple_connected;

    public bool isGrappleConnected{
        get { return grapple_connected; }
    }
    public void detachGrapple() {
        grapple_connected = false;
        line.enabled = false;
    }
    public float MaxLength {
        get { return current_wire_length; }
    }
    public Vector2 GrapplePoint {
        get { return grapple_target.TransformPoint(grapple_target_point); }
    }

    void Start () {
        grapple_mask = LayerMask.GetMask("Grappleable");
        has_grapple_target = false;
        grapple_connected = false;

        range_sensor = GetComponent<CircleCollider2D>();
        range_sensor.radius = MaxWireLength;

        line = GetComponent<LineRenderer>();
        line.enabled = false;

        platforms = new Dictionary<int, Collider2D>();
    }

    void Update()
    {
        Vector2 pos = transform.position;
        line.SetPosition(1, GrapplePoint - pos);
        if (Input.GetButtonDown("Fire1") && has_grapple_target)
        {
            grapple_connected = true;
            line.enabled = true;
        }
        if (Input.GetButtonDown("Fire2") && grapple_connected)
        {
            grapple_connected = false;
            line.enabled = false;
        }
    }

    void FixedUpdate () {
        Vector2 pos = transform.position;
        if (!grapple_connected) { 
            Vector2 dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            RaycastHit2D hit;
            float angle = 0f;
            do
            {
                float rads = angle * Mathf.Deg2Rad;
                Vector2 positive_dir;
                positive_dir.x = dir.x * Mathf.Cos(rads) - dir.y * Mathf.Sin(rads);
                positive_dir.y = dir.x * Mathf.Sin(rads) + dir.y * Mathf.Cos(rads);

                hit = Physics2D.Raycast(transform.position, positive_dir, MaxWireLength, grapple_mask);
                if (hit.collider == null)
                {
                    Vector2 negative_dir;
                    negative_dir.x = dir.x * Mathf.Cos(-rads) - dir.y * Mathf.Sin(-rads);
                    negative_dir.y = dir.x * Mathf.Sin(-rads) + dir.y * Mathf.Cos(-rads);

                    hit = Physics2D.Raycast(transform.position, negative_dir, MaxWireLength, grapple_mask);
                }
                angle++;
            } while (hit.collider == null && angle <= 30f);

            if (hit.collider != null ) 
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
        if (has_grapple_target && !grapple_connected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, grapple_target.TransformPoint(grapple_target_point));
        }
    }
}
