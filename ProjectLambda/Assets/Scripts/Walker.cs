using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : CustomPhysicsObject, IAttackable {
    enum State{
        START,
        IDLE,
        WALKING,
        MANOEUVRING,
        FIRING,
        STUNNED,
        DEAD
    };

    public AIFlag PatrolBoundary;

    public float VisibilityRange = 10f;
    public float WalkSpeed = 12f;
    public float MaxGunElevation = 45f;
    public float FiringPause = 5f;
    public float RateOfFire = 0.25f;
    public short NumShotsInBurst = 3;

    IEnumerator current_coroutine;
    State current_state = State.START;

    //transition booleans
    bool in_combat = false;
    bool is_timer_complete = false;
    bool was_attacked = false;
    bool is_dead = false;

    float max_idle = 8f;
    float min_idle = 1f;
    float max_walk = 2f;
    float min_walk = 0.5f;
    float current_timer_interval = 0f;

    float dir = -1f;

    LayerMask ground_mask;

    GameObject player;

    Health health;

    protected override void awake()
    {
        ground_mask = LayerMask.GetMask("Grappleable");
        transitionToState(doIdleState(current_state));

        player = GameObject.Find("Player");

        health = GetComponent<Health>();
        health.OnCharacterDeath += die;

    }
    protected override void update()
    {

    }
    protected override void fixedUpdate()
    {
        updateTransitionBools();
        checkStateTransitions();
        checkBoundaries();
    }
    protected override Vector2 setInputAcceleration()
    {
        Vector2 input = Vector2.zero;

        if (current_state == State.WALKING && IsGrounded)
        {
            input.x = dir;
        }

        return input * WalkSpeed;
    }
    public bool isStunned()
    {
        return current_state == State.STUNNED;
    }
    public bool isInvincible()
    {
        return false;
    }
    public void attack(int dmg, Vector2 dir, float pow, float stun_time = 0f)
    {
        if (current_state == State.DEAD) {
            return;
        }
        transitionToState(doStunnedState(current_state, dir, pow, stun_time));

        health.apply(-dmg);
    }
    void die() {
        if (current_state == State.STUNNED) {
            OverrideGravity = false;
            OverrideVelocityX = true;
        }
        transitionToState(doDeathState(current_state));
    }
    void updateTransitionBools()
    {
        in_combat = (player.transform.position - transform.position).magnitude <= VisibilityRange;
    }
    void checkStateTransitions() {
        if (current_state == State.IDLE)
        {
            if (in_combat)
            {
                transitionToState(doManoeuvringState(current_state));
            }
            else if (is_timer_complete)
            {
                transitionToState(doWalkingState(current_state));
            }
        }
        else if (current_state == State.WALKING)
        {
            if (in_combat)
            {
                transitionToState(doManoeuvringState(current_state));
            }
            else if (is_timer_complete)
            {
                transitionToState(doIdleState(current_state));
            }
        }
        else if (current_state == State.MANOEUVRING)
        {
            if (is_timer_complete) {
                transitionToState(doShootingState(current_state));
            }
            else if (!in_combat)
            {
                transitionToState(doIdleState(current_state));
            }
        }
        else if (current_state == State.STUNNED) {
            if (was_attacked) {
                was_attacked = false;
                transitionToState(doManoeuvringState(current_state));
            }
        }
    }
    void transitionToState(IEnumerator next_coroutine) {
        if (current_coroutine != null)
        {
            StopCoroutine(current_coroutine);
        }

        current_coroutine = next_coroutine;
        StartCoroutine(current_coroutine);
    }
    void checkBoundaries() {
        Vector2 proj = rb2d.position + Vector2.right * Facing;
        RaycastHit2D hit = Physics2D.Raycast(proj, Vector2.down, 3f, ground_mask);
        if (hit.collider == null || !PatrolBoundary.isInBoundary(proj))
        {
            dir *= -1f;
        }
    }
    IEnumerator doIdleState(State prev) {
        current_state = State.IDLE;

        is_timer_complete = false;

        current_timer_interval = Random.Range(min_idle, max_idle);
        yield return new WaitForSecondsRealtime(current_timer_interval);

        is_timer_complete = true;
    }
    IEnumerator doWalkingState(State prev) {
        current_state = State.WALKING;

        is_timer_complete = false;

        dir = (Random.value <= 0.5f ? -1f : 1f);

        current_timer_interval = Random.Range(min_walk, max_walk);
        yield return new WaitForSecondsRealtime(current_timer_interval);

        is_timer_complete = true;
    }
    IEnumerator doManoeuvringState(State prev) {
        current_state = State.MANOEUVRING;

        is_timer_complete = false;

        float timer = 0f;

        while (true)
        {
            dir = -transform.position.x * player.transform.position.y + transform.position.y * player.transform.position.x;
            if (dir >= 0)
            {
                dir = 1f;
            }
            else {
                dir = -1f;
            }

            timer += Time.deltaTime;
            if (timer >= FiringPause)
            {
                is_timer_complete = true;
            }

            yield return null;
        }
    }
    IEnumerator doShootingState(State prev) {


        yield return null;
    }
    IEnumerator doStunnedState(State prev, Vector2 dir, float pow, float stun_time) {
        current_state = State.STUNNED;

        rb2d.position += dir;
        Velocity = dir * pow;

        Debug.Log(stun_time);

        if (stun_time > 0f)
        {
            OverrideGravity = true;
            OverrideVelocityX = false;
            while (stun_time > 0f)
            {
                Velocity = Vector2.Lerp(Velocity, Vector2.zero, Velocity.magnitude * Time.deltaTime);
                stun_time -= Time.deltaTime;
                yield return null;
            }
        }

        OverrideGravity = false;
        OverrideVelocityX = true;

        was_attacked = true;
    }
    IEnumerator doDeathState(State prev) {
        current_state = State.DEAD;

        yield return null;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * dir);

        switch (current_state) {
            case State.IDLE: Gizmos.color = Color.white;
                break;
            case State.WALKING: Gizmos.color = Color.green;
                break;
            case State.MANOEUVRING: Gizmos.color = Color.yellow;
                break;
            case State.STUNNED: Gizmos.color = Color.blue;
                break;
            case State.DEAD: Gizmos.color = Color.black;
                break;
        }

        Gizmos.DrawSphere(transform.position + Vector3.up * 1.25f, 0.25f);

    }
}
