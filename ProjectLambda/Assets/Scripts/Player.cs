using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class Player : CustomPhysicsObject
{
    public float MovementSpeed = 7f;
    public float JumpForce = 7f;

    public float RespawnY = -250f;

    public Health health = new Health();
    public Grapple grapple;

    public Vector3 respawn_point;
    //made that public so I could make sure the checkpoints were working

    LongButtonPressDetector detector;

    ContactFilter2D dash_contact_filter;
    LayerMask enemy_mask;

    private InputDevice Controller;  

    bool did_grapple_jump = false;
    bool movement_enabled = true;
    bool jump_enabled = true;
    bool interupt_action = false;

    protected override void awake()
    {
        base.awake();

        detector = new LongButtonPressDetector();

        enemy_mask = LayerMask.GetMask("Enemy");

        dash_contact_filter.useTriggers = false;
        dash_contact_filter.SetLayerMask(enemy_mask);
        dash_contact_filter.useLayerMask = true;

        health.OnHealthDamaged += healthDamaged;
        health.OnCharacterDeath += die;

        respawn_point = transform.position;

        grapple.setParent(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //just sets the respawn point to the position of the last checkpoint reached
        if (other.tag == "CheckPoint")
        {
            respawn_point = other.transform.position;
        }
    }

    protected override void update()
    {
        base.update();

        Controller = InputManager.ActiveDevice;

        if (detector.longPress("Attack 1"))
        {
            setBulletTime(true);
            OverrideVelocityX = false;
            Vector2 aim = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (Input.GetButtonUp("Attack 1"))
            {
                StartCoroutine(doDashAttack(aim));
                setBulletTime(false);
            }

        }
        else if (detector.shortPress("Attack 1")) {
            basicAttack();
        }

        /*if (!grapple.isGrappleConnected)
        {
            if (detector.longPress("Attack 3"))
            {
                setBulletTime(true);
                OverrideVelocityX = false;

                grapple.aim(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
                if (Input.GetButtonUp("Attack 3"))
                {
                    grapple.fire();
                    setBulletTime(false);
                    OverrideVelocityX = true;
                }
            }
            else if (detector.shortPress("Attack 3"))
            {
                grapple.fire();
            }

        }*/
        if (/*Input.GetButtonDown("Attack 2"))*/ Controller.Action4.WasPressed) {
            interupt_action = false;
            StartCoroutine(doGroundSlam());
        }
        if (Input.GetButtonUp("Attack 2")) {
            interupt_action = true;
        }

        if (/*Input.GetButtonDown("Attack 3") ||*/ Controller.Action3.WasPressed)
        {
            if (grapple.isGrappleConnected)
            {
                grapple.detach();
            }
            else
            {
                grapple.fire();
            }
        }

        if (transform.position.y < RespawnY) {
            health.instakill();
        }
        detector.Update();
    }
    protected override void fixedUpdate()
    {
        base.fixedUpdate();

        if (grapple.isGrappleConnected)
        {
            setTetherPoint(grapple.GrapplePoint, grapple.MaxLength);
        }
        else
        {
            releaseTether();
        }

    }
    protected override Vector2 setInputAcceleration()
    {
        Vector2 move = Vector2.zero;

        if (movement_enabled)
        {
            move.x = Input.GetAxis("Horizontal");
        }
        if (jump_enabled)
        {
            if (/*Input.GetButtonDown("Jump") ||*/ Controller.Action1.IsPressed)
            {
                if (IsGrounded)
                {
                    Velocity = new Vector2(Velocity.x, JumpForce);
                }
                else if (grapple.isGrappleConnected)
                {
                    grapple.detach();
                    releaseTether();
                    if (Velocity.magnitude <= 1f)
                    {
                        Velocity = new Vector2(Velocity.x, JumpForce);
                    }
                    did_grapple_jump = true;
                }
            }
            else if (/*Input.GetButtonUp("Jump")*/ Controller.Action1.WasReleased)
            {
                if (did_grapple_jump)
                {
                    did_grapple_jump = false;
                }
                else if (Velocity.y > 0)
                {
                    Velocity = new Vector2(Velocity.x, Velocity.y * 0.5f);
                }
            }
        }
        return move * MovementSpeed;
    }
    protected override void onDrawGizmos()
    {
        base.onDrawGizmos();

        //Gizmos.color = Color.red;
        //Gizmos.DrawWireCube(transform.position + transform.right * Facing, new Vector3(3f, 1f, 1f));
    }
    void setBulletTime(bool enabled) {
        Time.timeScale = (enabled ? 0.05f : 1f);
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }
    void canUseAllControls(bool enabled) {
        canUseMovementControls(enabled);
        canUseJump(enabled);
    }
    void canUseMovementControls(bool enabled)
    {
        movement_enabled = enabled;
    }
    void canUseJump(bool enabled) {
        jump_enabled = enabled;
    }
    void healthDamaged(int hp, int max) {
        Debug.Log("current hp precentage: " + ((float)hp / max) * 100);
    }
    void die() {
        //triggered when player health reaches 0
        Debug.Log("you have died");
        transform.position = respawn_point;
        health.reset();
        GetComponent<TrailRenderer>().Clear();
    }
    void basicAttack()
    {
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position + transform.right * Facing, new Vector2(3f, 1), 0f, enemy_mask);
        if (cols.Length != 0)
        {
            foreach (Collider2D col in cols)
            {
                TempAttackedBehaviour tab = col.GetComponent<TempAttackedBehaviour>();
                if (tab != null)
                {
                    if (!tab.IsStunned)
                    {
                        tab.doAttackBehaviour(transform.position, 8f);
                    }
                }
            }
        }
    }
    IEnumerator doDashAttack(Vector2 dir) {
        OverrideGravity = true;

        Velocity = dir.normalized * 80f;

        Collider2D[] cols = new Collider2D[16];
        float timer = 0f;
        while (timer <= 0.14f) {
            int count = rb2d.OverlapCollider(dash_contact_filter, cols);
            for (int i = 0; i < count; i++) {
                TempAttackedBehaviour tab = cols[i].GetComponent<TempAttackedBehaviour>();
                if (tab != null)
                {
                    if (!tab.IsStunned)
                    {
                        tab.doAttackBehaviour(transform.position, 18f);
                    }
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }
        Velocity = dir.normalized * 10f;

        OverrideGravity = false;
        OverrideVelocityX = true;
    }
    IEnumerator doGroundSlam() {
        float multiplier = 10f;

        Mass *= multiplier;
        canUseAllControls(false);

        while (!IsGrounded && !interupt_action)
        {
            Collider2D[] cols = new Collider2D[16];
            int count = rb2d.OverlapCollider(dash_contact_filter, cols);
            for (int i = 0; i < count; i++)
            {
                TempAttackedBehaviour tab = cols[i].GetComponent<TempAttackedBehaviour>();
                if (tab != null)
                {
                    //pick up enemies
                }
            }
            yield return null;
        }

        if (!interupt_action)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, 10f, enemy_mask);
            foreach (Collider2D col in cols)
            {
                TempAttackedBehaviour tab = col.GetComponent<TempAttackedBehaviour>();
                if (tab != null)
                {
                    float mag = 1f - ((col.transform.position - transform.position).magnitude / 10f);
                    tab.doAttackBehaviour(transform.position, 16f * mag);
                }
            }
        }
        else {
            Velocity = Vector2.zero;
        }

        Mass = Mass / multiplier;
        canUseAllControls(true);
    }
    /*
        WIP: function slows down game speed to the value of param floor.
        better alternative may be to slow down only player and affected entities.
    */
    IEnumerator doHitPause(float floor, float rate)
    {
        while (Time.timeScale > floor)
        {
            Time.timeScale = Mathf.Clamp(Time.timeScale - rate * Time.unscaledDeltaTime, floor, 1f);
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
            yield return null;
        }
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }
}

