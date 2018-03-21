using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempAttackedBehaviour : CustomPhysicsObject {
    bool is_stunned = false;
    float timer;

    public bool IsStunned{
        get { return is_stunned; }
    }
    protected override void awake()
    {
        base.awake();

        OverrideVelocityX = false;
    }
    protected override void fixedUpdate() {
        base.fixedUpdate();
        if (is_stunned)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;

            }
            else
            {
                is_stunned = false;
                OverrideGravity = false;
            }
            Velocity = Vector2.Lerp(Velocity, Vector2.zero, Velocity.magnitude * Time.deltaTime);
        }
    }
    public void knockBack(Vector2 dir, float pow, float stun_time = 0f) {
        is_stunned = (stun_time > 0);

        if (is_stunned)
        {
            timer = stun_time;
            OverrideGravity = true;
        }
        else {
            OverrideGravity = false;
        }

        rb2d.position += dir;
        Velocity = dir * pow;
    }
}
