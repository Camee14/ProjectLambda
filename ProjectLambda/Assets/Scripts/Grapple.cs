using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public float MinWireLength = 0.5f;
    public float MaxWireLength = 20f;
    public bool DrawDebug;

    CircleCollider2D range_sensor;
    LineRenderer line;

    LayerMask grapple_mask;

    Transform grapple_target;
    Vector2 grapple_target_point;

    Dictionary<int, Collider2D>platforms;

    float current_wire_length = 9f;
    bool has_grapple_target;
    bool grapple_connected;

    bool controller_active = false;

    public bool isGrappleConnected{
        get { return grapple_connected; }
    }
    public bool isGrappleConnectedToEnemy {
        get { return grapple_connected && grapple_target.tag == "Enemy"; }
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
    public Transform GrappleTarget {
        get { return grapple_target; }
    }

    void Start () {
        grapple_mask = LayerMask.GetMask("Grappleable", "Enemy");
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
        float triggers = Input.GetAxis("Fire1");

        if (triggers < 0 && has_grapple_target)
        {
            grapple_connected = true;
            line.enabled = true;
        }
        if (triggers > 0 && grapple_connected)
        {
            grapple_connected = false;
            line.enabled = false;
        }

        Vector2 pos = transform.position;
        if (grapple_connected)
        {
            line.SetPosition(1, GrapplePoint - pos);
            if (controller_active) {
                //float inputY = Input.GetAxis("Vertical");
                //current_wire_length =  Mathf.Clamp(current_wire_length - inputY * 5f * Time.deltaTime, MinWireLength, MaxWireLength);
            }
        }

        if (controller_active)
        {
            Vector2 input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            if (input.magnitude > 0) {
                controller_active = false;
            }
        }
        else {
            Vector2 input = new Vector2(Input.GetAxis("Controller X"), Input.GetAxis("Controller Y"));
            if (input.magnitude > 0)
            {
                controller_active = true;
            }
        }
    }

    void FixedUpdate () {
        Vector2 pos = transform.position;
        if (!grapple_connected) {

            Vector2 dir = Vector2.up;
            if (controller_active)
            {
                dir.x = Input.GetAxis("Controller X");
                dir.y = Input.GetAxis("Controller Y");
            }
            else
            {
                dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            }

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

                current_wire_length = (hit.point - pos).magnitude;
                if (current_wire_length > 6f) {
                    current_wire_length *= 0.5f;
                }

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
