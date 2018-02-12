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

    bool did_grapple_jump = false;

    protected override void awake()
    {
        base.awake();

        addAction("Basic Attack", new BasicAttack(this));
        addAction("Grapple Leap Attack", new GrappleLeapAttack(this));
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

        move.x = Input.GetAxis("Horizontal");
        move.y = Input.GetAxis("Vertical");

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
        return move * move_speed;
    }
    bool SAGroundSlam(bool interupt) {
        if (interupt)
        {
            //interupt the attack
            return true;
        }
        return true;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + transform.right * Facing, new Vector3(3f, 1f, 1f));
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
                            tab.doAttackBehaviour(parent.transform.position, 19f);
                        }
                    }
                }
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
                        tab.doAttackBehaviour(parent.transform.position, 50f);
                    }
                }
                return true;
            }
            return false;
        }
        public override void endAction()
        {

        }
        public override void interuptAction()
        {

        }
    }
}

