using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempAttackedBehaviour : MonoBehaviour {
    Vector2 vel;
    Rigidbody2D rb2d;

    bool is_stunned = false;
    float timer;

    public bool IsStunned{
        get { return is_stunned; }
    }
    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate() {
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
        rb2d.AddForce(dir * power, ForceMode2D.Impulse);

        is_stunned = true;
    }
}
