using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public float MinWireLength = 0.5f;
    public float MaxWireLength = 20f;
    public bool SoftLockOn = true;
   
    public GameObject AimIndicatior;

    public bool DrawDebug;

    Player parent;

    LineRenderer line;
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
        grapple_mask = LayerMask.GetMask("Grappleable", "Enemy");

        line = GetComponent<LineRenderer>();
        line.enabled = false;
    }

    void Update()
    {
        if (grapple_connected)
        {
            line.SetPosition(1, GrapplePoint - (Vector2)transform.position);
        }
    }
    void FixedUpdate()
    {
         if (!grapple_connected)
         {
             Vector2 dir = getAimDir();

             RaycastHit2D hit;
             if (SoftLockOn)
             {
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
             }
             else
             {
                 hit = Physics2D.Raycast(transform.position, dir, MaxWireLength, grapple_mask);
             }

             if (hit.collider != null)
             {
                 grapple_target = hit.transform;
                 grapple_target_point = grapple_target.InverseTransformPoint(hit.point);

                 current_wire_length = (hit.point - (Vector2)transform.position).magnitude;
                 if (current_wire_length > 6f)
                 {
                     current_wire_length *= 0.5f;
                 }
                 AimIndicatior.transform.position = hit.point;
             }
             else
             {
                AimIndicatior.transform.position = transform.position + ((Vector3)dir * MaxWireLength);
                grapple_target = null;
             }
         }
    }
    Vector2 getAimDir() {
        return (man_aim_dir != Vector2.zero ? man_aim_dir : Vector2.up + (Vector2.right * parent.Facing)).normalized;
    }
    IEnumerator doReelIn() {
        parent.OverridePhysics(true);

        yield return null;

        parent.OverridePhysics(false);
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
    }
}
