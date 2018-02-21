using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : ScriptablePhysicsObject
{
    public float move_speed = 7f;
    public float jump_force = 7f;

    public float grapple_speed = 10f;
    public float swing_force = 20f;

    public Grapple grapple;

    Health health;

    bool did_grapple_jump = false;
    bool movement_enabled = true;
    bool jump_enabled = true;

    protected override void awake()
    {
        base.awake();

        addAction("Basic Attack", new BasicAttack(this));
        addAction("Grapple Leap Attack", new GrappleLeapAttack(this));
        addAction("Ground Slam Attack", new GroundSlamAttack(this));

        health = GetComponent<Health>();
        health.OnHealthDamaged += healthDamaged;
        health.OnCharacterDeath += die;

        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 30;
    }
    protected override void update()
    {
        base.update();

        if (Input.GetButtonDown("Attack 1"))
        {
            if (grapple.isGrappleConnectedToEnemy)
            {
                startAction("Grapple Leap Attack", false);
            }
            else
            {
                startAction("Basic Attack", false);
               
            }
        }
        if (Input.GetButtonDown("Attack 2")) {
            if (!IsGrounded && !grapple.isGrappleConnected)
            {
                startAction("Ground Slam Attack", false);
            }
            else if (grapple.isGrappleConnectedToEnemy) {
                
            }
        }

        if (transform.position.y < -15f) {
            health.instakill();
        }
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
            move.y = Input.GetAxis("Vertical");
        }
        if (jump_enabled)
        {
            if (Input.GetButtonDown("Jump"))
            {
                if (IsGrounded)
                {
                    Velocity = new Vector2(Velocity.x, jump_force);
                }
                else if (grapple.isGrappleConnected)
                {
                    grapple.detachGrapple();
                    releaseTether();
                    if (Velocity.magnitude <= 1f)
                    {
                        Velocity = new Vector2(Velocity.x, jump_force);
                    }
                    else
                    {
                        //Velocity += move * jump_force;
                    }
                    did_grapple_jump = true;
                }
            }
            else if (Input.GetButtonUp("Jump"))
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
        return move * move_speed;
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
        transform.position = new Vector3(-20, 10, 0);
        health.reset();
    }
    /*
    WIP: function slows down game speed to the value of param floor.
    better alternative may be to slow down only player and affected entities.
    */
    IEnumerator doHitPause(float floor, float rate) {
        while (Time.timeScale > floor)
        {
            Time.timeScale = Mathf.Clamp(Time.timeScale - rate * Time.unscaledDeltaTime, floor, 1f);
            yield return null;
        }
        Time.timeScale = 1f;
    }
    protected override void onDrawGizmos()
    {
        base.onDrawGizmos();

        //Gizmos.color = Color.red;
       // Gizmos.DrawWireCube(transform.position + transform.right * Facing, new Vector3(3f, 1f, 1f));
    }

    private class BasicAttack : ScriptableAction {
        public BasicAttack(ScriptablePhysicsObject p) : base(p)
        {

        }
        public override void startAction(){

        }
        public override bool continueAction() {
            LayerMask mask = LayerMask.GetMask("Enemy");
            Collider2D[] cols = Physics2D.OverlapBoxAll(parent.transform.position + parent.transform.right * parent.Facing, new Vector2(3f, 1), 0f, mask);
            if (cols.Length != 0)
            {
                foreach (Collider2D col in cols)
                {
                    TempAttackedBehaviour tab = col.GetComponent<TempAttackedBehaviour>();
                    if (tab != null)
                    {
                        if (!tab.IsStunned)
                        {
                            tab.doAttackBehaviour(parent.transform.position, 8f);
                        }
                    }
                }
                ((Player)parent).StartCoroutine(((Player)parent).doHitPause(0.1f, 3f));
            }
            return true;
        }
        public override void endAction() {

        }
        public override void interuptAction() {

        }
    }
    private class GrappleLeapAttack : ScriptableAction
    {
        public GrappleLeapAttack(ScriptablePhysicsObject p) : base(p)
        {

        }
        public override void startAction()
        {
            ((Player)parent).canUseAllControls(false);
        }
        public override bool continueAction()
        {
            ((Player)parent).rb2d.position = Vector3.Lerp(parent.transform.position, ((Player)parent).grapple.GrappleTarget.position, 14f * Time.deltaTime);

            if ((((Player)parent).grapple.GrappleTarget.position - parent.transform.position).magnitude <= 1f)
            {
                ((Player)parent).grapple.detachGrapple();
                TempAttackedBehaviour tab = ((Player)parent).grapple.GrappleTarget.GetComponent<TempAttackedBehaviour>();
                if (tab != null)
                {
                    if (!tab.IsStunned)
                    {
                        tab.doAttackBehaviour(parent.transform.position, 12f);
                    }
                }
                return true;
            }
            return false;
        }
        public override void endAction()
        {
            ((Player)parent).canUseAllControls(true);
        }
        public override void interuptAction()
        {
            ((Player)parent).canUseAllControls(true);
        }
    }
    private class GroundSlamAttack : ScriptableAction
    {
        int multiplier = 10;
        public GroundSlamAttack(ScriptablePhysicsObject p) : base(p)
        {

        }
        public override void startAction()
        {
            parent.Mass *= multiplier;
        }
        public override bool continueAction()
        {
            if (!parent.IsGrounded) {
                return false;
            }
            //do ground attack;
            LayerMask mask = LayerMask.GetMask("Enemy");
            Collider2D[] cols = Physics2D.OverlapCircleAll(parent.transform.position, 10f, mask);
            foreach (Collider2D col in cols) {
                TempAttackedBehaviour tab = col.GetComponent<TempAttackedBehaviour>();
                if (tab != null) {
                    float mag = 1f - ((col.transform.position - parent.transform.position).magnitude / 10f);
                    tab.doAttackBehaviour(parent.transform.position, 16f * mag);
                }
            }
            return true;
        }
        public override void endAction()
        {
            parent.Mass = parent.Mass / multiplier;
        }
        public override void interuptAction()
        {
            parent.Mass = parent.Mass / multiplier;
        }
    }
    private class GrappleSlam : ScriptableAction
    {
        public GrappleSlam(ScriptablePhysicsObject p) : base(p)
        {
        }
        public override void startAction()
        {
            
        }
        public override bool continueAction()
        {
            return true;
        }
        public override void endAction()
        {
            
        }
        public override void interuptAction()
        {
            
        }
    }
}

