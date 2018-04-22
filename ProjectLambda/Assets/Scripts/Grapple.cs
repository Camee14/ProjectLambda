using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class Grapple : MonoBehaviour {

    public float MinWireLength = 0.5f;
    public float MaxWireLength = 20f;
    public Color CanGrappleIndicator = Color.green;
    public Color CannotGrappleIndicator = Color.red;
    public float SoftLockOnAngle = 30f;
    public bool SoftLockOn = true;
   
    public GameObject AimIndicatior;

    public bool DrawDebug;

    Player parent;

    LineRenderer line;
    SpriteRenderer indicator;
    LayerMask grapple_mask;

    Transform grapple_target;
    Vector2 grapple_target_point;
    Vector2 man_aim_dir;


    float current_wire_length = 9f;
    bool grapple_connected = false;

    public bool isGrappleConnected {
        get { return grapple_connected; }
    }
    public bool isGrappleConnectedToEnemy {
        get { return grapple_connected && grapple_target.tag == "Enemy"; }
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

    public void setGrappleLength(float new_len) {
        current_wire_length = Mathf.Clamp(new_len, MinWireLength, MaxWireLength);
    }
    public void setParent(Player p) {
        parent = p;
    }

    public void aim(Vector2 dir) {
        man_aim_dir = dir;
    }
    public bool fire() {
        man_aim_dir = Vector2.zero;
        if (GrappleTarget != null) {
            grapple_connected = true;

            line.SetPosition(0, transform.position);
            line.SetPosition(1, GrapplePoint);

            line.enabled = true;
            return true;
        }
        return false;
    }
    public void detach()
    {
        grapple_connected = false;
        line.enabled = false;
    }

    void Awake () {
        grapple_mask = LayerMask.GetMask("Grappleable", "DynamicPlatform");

        line = GetComponent<LineRenderer>();
        line.enabled = false;

        indicator = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (grapple_connected)
        {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, GrapplePoint);
        }
    }
    void FixedUpdate()
    {
         if (!grapple_connected)
         {
            Vector2 dir = getAimDir();
            Transform target;
            Vector2 point;

             if (getTarget(dir, out point, out target))
             {
                 grapple_target = target;
                 grapple_target_point = grapple_target.InverseTransformPoint(point);

                 current_wire_length = (point - (Vector2)transform.position).magnitude;
                 if (current_wire_length > 6f)
                 {
                     current_wire_length *= 0.5f;
                 }
                 AimIndicatior.transform.position = point;
                indicator.color = CanGrappleIndicator;
            }
             else
             {
                AimIndicatior.transform.position = transform.position + ((Vector3)dir * MaxWireLength);
                indicator.color = CannotGrappleIndicator;
                grapple_target = null;
             }
         }
    }
    Vector2 getAimDir() {
        return (man_aim_dir != Vector2.zero ? man_aim_dir : Vector2.up + (Vector2.right * parent.Facing)).normalized;
    }
    bool getTarget(Vector2 dir, out Vector2 point, out Transform target) {
        point = Vector2.zero;
        target = null;
        RaycastHit2D hit;
        if (SoftLockOn)
        {
            hit = Physics2D.Raycast(transform.position, dir, MaxWireLength, grapple_mask);
            if (hit.collider == null)
            {
                dir = dir.normalized;

                Vector2 n_dir = RotateVector(dir, -(SoftLockOnAngle / 2)) * MaxWireLength;
                Vector2 p_dir = RotateVector(dir, SoftLockOnAngle / 2) * MaxWireLength;

                Vector2 p1 = (Vector2)transform.position + n_dir;
                Vector2 p2 = (Vector2)transform.position + p_dir;

                float max_rad = (p2 - p1).magnitude / 2;
                float min_rad = 0.1f;
                float offset = 1;
                while (offset < MaxWireLength)
                {
                    float rad = Mathf.Lerp(min_rad, max_rad, offset / MaxWireLength);
                    Vector2 center = (Vector2)transform.position + dir.normalized * offset;
                    Collider2D[] cols = Physics2D.OverlapCircleAll(center, rad, grapple_mask);

                    bool found = false;
                    foreach (Collider2D col in cols) {
                        Vector2 p = col.bounds.ClosestPoint(center); // this doesnt work for anything other than AABB's
                        if (!found)
                        {
                            point = p;
                            target = col.transform;
                            if ((p - (Vector2)transform.position).magnitude < MaxWireLength)
                            {
                                found = true;
                            }
                        }
                        else
                        {
                            if ((p - center).sqrMagnitude <= (point - center).sqrMagnitude && (p - (Vector2)transform.position).magnitude < MaxWireLength)
                            {
                                point = p;
                            }
                        }
                    }
                    if (found) {
                        return true;
                    }

                    offset += rad;
                }
            }
        }
        else
        {
            hit = Physics2D.Raycast(transform.position, dir, MaxWireLength, grapple_mask);
        }

        if (hit.collider != null)
        {
            point = hit.point;
            target = hit.transform;
            return true;
        }
        return false;
    }
    Vector2 RotateVector(Vector2 v, float a) {
        float x = v.x;
        float y = v.y;
        a = a * Mathf.Deg2Rad;
        v.x = (x * Mathf.Cos(a)) - (y * Mathf.Sin(a));
        v.y = (x * Mathf.Sin(a)) + (y * Mathf.Cos(a));

        return v;
    }
    void OnDrawGizmos()
    {
        if (!DrawDebug)
        {
            return;
        }
        if (GrappleTarget != null && !grapple_connected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, grapple_target.TransformPoint(grapple_target_point));
        }

        Vector2 dir = getAimDir();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, MaxWireLength, grapple_mask);
        if (hit.collider == null)
        {
            dir = dir.normalized;

            Vector2 n_dir = RotateVector(dir, -(SoftLockOnAngle / 2)) * MaxWireLength;
            Vector2 p_dir = RotateVector(dir, SoftLockOnAngle / 2) * MaxWireLength;

            Vector2 p1 = (Vector2)transform.position + n_dir;
            Vector2 p2 = (Vector2)transform.position + p_dir;

            float max_rad = (p2 - p1).magnitude / 2;
            float min_rad = 0.1f;
            float offset = 1f;
            while (offset < MaxWireLength)
            {
                float rad = Mathf.Lerp(min_rad, max_rad, offset / MaxWireLength);
                Gizmos.DrawWireSphere((Vector2)transform.position + dir * offset, rad);
                offset += rad;
            }
        }
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir.normalized * MaxWireLength);
    }
}
