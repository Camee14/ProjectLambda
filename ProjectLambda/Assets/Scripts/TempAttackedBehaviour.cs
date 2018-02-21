using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempAttackedBehaviour : CustomPhysicsObject {
    bool is_stunned = false;
    float timer;

    public bool IsStunned{
        get { return is_stunned; }
    }
    protected override void fixedUpdate() {
        base.fixedUpdate();
        if (is_stunned)
        {
            if (timer >= 0.8f)
            {
                is_stunned = false;
                timer = 0f;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
    }
    public void doAttackBehaviour(Vector2 attacker_pos, float power) {
        Vector2 dir = new Vector2(transform.position.x - attacker_pos.x, transform.position.y - attacker_pos.y);
        dir.y += 3f;
        dir.Normalize();

        Velocity = dir * power;

        is_stunned = true;
    }
}
