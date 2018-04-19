using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anima2D;

public class Walker : CustomPhysicsObject, IAttackable, ISpawnable {
    enum State{
        SLEEPING,
        IDLE,
        WALKING,
        MANOEUVRING,
        FIRING,
        STUNNED,
        KNOCKED_BACK,
        DEAD
    };

    public float VisibilityRange = 10f;
    public float WalkSpeed = 12f;
    public float MaxGunElevation = 45f;
    public float MaxGunDepression = -20f;
    public float MinFiringPause = 3f;
    public float MaxFiringPause = 5f;
    public float RateOfFire = 0.25f;
    public short NumShotsInBurst = 3;
    public Animator Anim;
    public GameObject BulletPrefab;
    public Transform GunBarrel;
    public Transform AimIK;
    public bool DrawAIDebug = false;

    IEnumerator current_coroutine;
    State current_state = State.SLEEPING;
    State prev_anim_state = State.SLEEPING;

    //transition booleans
    bool in_combat = false;
    bool is_timer_complete = false;
    bool was_attacked = false;

    float max_idle = 8f;
    float min_idle = 1f;
    float max_walk = 2f;
    float min_walk = 0.5f;
    float current_timer_interval = 0f;

    float dir = -1f;
    bool is_walking = false;
    bool is_on_edge = false;
    bool is_stunned = false;

    AIFlag PatrolBoundary;

    LayerMask walkable_mask;
    LayerMask visibility_mask;

    GameObject player;
    ParticleSystem HitParticles;

    Health health;
    ProximitySwitch detector;
    

    Vector2 aim;
    Vector2 respawn_point;
    Vector2 default_gun_pos;

    protected override void awake()
    {
        base.awake();

        visibility_mask = LayerMask.GetMask("Grappleable", "DynamicPlatform");
        walkable_mask = LayerMask.GetMask("Grappleable");

        player = GameObject.Find("Player");
        Player p = player.GetComponent<Player>();
        p.onPlayerDeath += playerDeath;
        p.onPlayerRespawnChanged += playerRespawnChanged;

        health = GetComponent<Health>();
        health.OnCharacterDeath += die;

        detector = GetComponent<ProximitySwitch>();
        detector.onSwitchOn += activateAI;
        detector.onSwitchOff += deactivateAI;

        HitParticles = transform.Find("SparkParticles").GetComponent<ParticleSystem>();

        OverrideAutoFacing = true;

        aim = Vector2.right * Facing;

        default_gun_pos = AimIK.position;

        transitionToState(doIdleState(current_state));
    }
    protected override void update()
    {
        base.update();

        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -Facing, transform.localScale.y, transform.localScale.z);
    }
    protected override void fixedUpdate()
    {
        base.fixedUpdate();

        if (!detector.IsSwitchedOn) {
            return;
        }
        Facing = aim.x >= 0 ? 1.0f : -1.0f;

        State prev = current_state;

        updateTransitionBools();
        checkStateTransitions();
        checkBoundaries();
        updateAnimations();
    }
    protected override Vector2 setInputAcceleration()
    {
        Vector2 input = Vector2.zero;

        if (is_walking && health.isAlive())
        {
            if (current_state == State.WALKING && IsGrounded)
            {
                input.x = dir * WalkSpeed;
            }
            else if (current_state == State.MANOEUVRING && IsGrounded)
            {
                input.x = dir * (WalkSpeed / 4);
            }
        }

        return input;
    }
    public bool isStunned()
    {
        return current_state == State.STUNNED || current_state == State.KNOCKED_BACK;
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
        is_walking = false;
        health.apply(-dmg);
        transitionToState(doAttackStunnedState(current_state, dir, pow, stun_time));
    }
    public void knockback(Vector2 dir, float pow, float hang_time = 0f) {
        if (current_state == State.DEAD)
        {
            return;
        }
        is_walking = false;
        transitionToState(doKnockBackStunnedState(current_state, dir, pow, hang_time));
    }
    public void spawn(GameObject spawner) {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10, walkable_mask);
        if (hit.collider != null) {
            transform.position = hit.point;
            
        }
        respawn_point = transform.position;
        PatrolBoundary = spawner.transform.parent.GetComponent<AIFlag>();
    }
    void die() {
        if (current_state == State.STUNNED) {
            OverrideGravity = false;
            OverrideVelocityX = true;
        }
        is_walking = false;
        transitionToState(doDeathState(current_state));
    }
    void playerDeath()
    {
        health.reset();
        transform.position = respawn_point;

        Anim.Rebind();

        detector.reset();

        StopAllCoroutines();
        current_state = State.SLEEPING;
    }
    void playerRespawnChanged()
    {
        if (current_state == State.DEAD)
        {
            Player p = player.GetComponent<Player>();
            p.onPlayerDeath -= playerDeath;
            p.onPlayerRespawnChanged -= playerRespawnChanged;
            Destroy(gameObject);
        }
    }
    void activateAI() {
        if (current_state == State.DEAD)
        {
            return;
        }
        transitionToState(doIdleState(current_state));
    }
    void deactivateAI() {
        if (current_state == State.DEAD)
        {
            return;
        }
        StopAllCoroutines();
        current_state = State.SLEEPING;
    }
    void updateTransitionBools()
    {
        if ((player.transform.position - transform.position).magnitude <= VisibilityRange) {
            in_combat = Physics2D.Linecast(transform.position, player.transform.position, visibility_mask).collider == null;
        }
        else
        {
            in_combat = false;
        }
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
            if (!in_combat)
            {
                transitionToState(doIdleState(current_state));
            }
            if (is_timer_complete)
            {
                transitionToState(doShootingState(current_state));
            }
        }
        else if (current_state == State.FIRING) {
            if (is_timer_complete) {
                transitionToState(doManoeuvringState(current_state));
            }
        }
        else if (current_state == State.STUNNED || current_state == State.KNOCKED_BACK)
        {
            if (was_attacked)
            {
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
        Vector2 proj = rb2d.position + Vector2.right * dir;
        RaycastHit2D hit = Physics2D.Raycast(proj, Vector2.down, 3f, walkable_mask);
        if (hit.collider == null || !PatrolBoundary.isInBoundary(proj))
        {
            is_on_edge = true;
            if (current_state == State.WALKING)
            {
                dir *= -1f;
                aim = Vector2.right * dir;
            }
        }
        else {
            is_on_edge = false;
        }
    }
    void updateAnimations() {
        Anim.SetBool("is_walking", is_walking);
        Anim.SetBool("is_stunned", current_state == State.STUNNED);
        Anim.SetBool("is_knockedback", current_state == State.KNOCKED_BACK);
        Anim.speed = (is_walking && current_state == State.MANOEUVRING) ? 0.25f : 1f;
        if (current_state == State.DEAD && prev_anim_state != State.DEAD) {
            Anim.SetTrigger("die");
        }
        prev_anim_state = current_state;
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

        is_walking = true;

        dir = (Random.value <= 0.5f ? -1f : 1f);
        aim = Vector2.right * dir;

        current_timer_interval = Random.Range(min_walk, max_walk);
        yield return new WaitForSecondsRealtime(current_timer_interval);

        is_walking = false;

        is_timer_complete = true;
    }
    IEnumerator doManoeuvringState(State prev) {
        current_state = State.MANOEUVRING;

        OverrideGravity = false;
        OverrideVelocityX = true;

        is_timer_complete = false;

        float timer = Random.Range(MinFiringPause, MaxFiringPause);

        while (true)
        {
            aim = (player.transform.position - transform.position);
           
            float side = aim.x >= 0 ? 1.0f : -1.0f;

            float angle = Vector2.SignedAngle(Vector2.right * side, aim) * side;
            if (angle > MaxGunElevation)
            {
                aim = Quaternion.Euler(0, 0, MaxGunElevation * Facing) * Vector2.right * Facing;
            }
            else if (angle < MaxGunDepression)
            {
                aim = Quaternion.Euler(0, 0, MaxGunDepression * Facing) * Vector2.right * Facing;
            }
            //AimIK.rotation = Quaternion.LookRotation(Vector3.forward, new Vector3(aim.y, -aim.x, 0) * Facing);
            //AimIK.position = (Vector2)transform.position + aim;
            
            if (!is_on_edge) {
                if (angle > MaxGunElevation || angle < MaxGunDepression)
                {
                    is_walking = true;
                    dir = -side;
                }
                else
                {
                    dir = side;
                    is_walking = (aim.magnitude > 10f);
                }

            } else {
                is_walking = false;
            }

            if (detector.IsVisible)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    is_timer_complete = true;
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator doShootingState(State prev) {
        is_timer_complete = false;
        current_state = State.FIRING;

        short bursts_fired = 0;

        while (bursts_fired < NumShotsInBurst)
        {
            Instantiate(BulletPrefab, GunBarrel.position, Quaternion.LookRotation(Vector3.forward, aim));
            bursts_fired++;

            yield return new WaitForSeconds(RateOfFire);
        }
        is_timer_complete = true;
    }
    IEnumerator doAttackStunnedState(State prev, Vector2 dir, float pow, float stun_time) {
        current_state = State.STUNNED;
        dir.Normalize();

        HitParticles.Play();
        HitParticles.transform.rotation = Quaternion.LookRotation(-dir, new Vector2(dir.y, -dir.x));

        OverrideVelocityX = false;
        if (stun_time > 0f && health.isAlive())
        {
            dir.y = 0;
            OverrideGravity = true;
        
            while (stun_time > 0f)
            {
                //Velocity = Vector2.Lerp(Velocity, Vector2.zero, Velocity.magnitude * Time.deltaTime);
                Velocity = dir * pow;
                pow -= 1200 * Time.deltaTime;
                if (pow < 0)
                {
                    pow = 0;
                }
                yield return new WaitForFixedUpdate();
                stun_time -= Time.deltaTime;
            }
        }
        else if (!health.isAlive()) {
            Velocity = dir * 10;
        }

        if (health.isAlive())
        {
            OverrideVelocityX = true;
        }
        else {
            current_state = State.DEAD;
        }

        OverrideGravity = false;
        was_attacked = true;
    }
    IEnumerator doKnockBackStunnedState(State prev, Vector2 dir, float pow, float knockback_time) {
        current_state = State.KNOCKED_BACK;

        dir.Normalize();
        dir *= pow;
        OverrideVelocityX = false;
        OverrideGravity = true;

        float timer = 0;
        Velocity = dir;
        while (timer < knockback_time)
        {
            Velocity = Vector2.Lerp(Velocity, Vector2.zero, (timer / knockback_time));
            yield return new WaitForFixedUpdate();
            timer += Time.deltaTime;
        }

        OverrideVelocityX = true;
        OverrideGravity = false;
        was_attacked = true;
    }
    IEnumerator doDeathState(State prev) {
        current_state = State.DEAD;

        yield return new WaitForSeconds(3.0f);
    }

    void OnDrawGizmos() {
        if (!DrawAIDebug) {
            return;
        }

        Gizmos.color = Color.blue;
        Vector2 max = Quaternion.Euler(0, 0, MaxGunElevation * Facing) * Vector2.right * Facing;
        Vector2 min = Quaternion.Euler(0, 0, MaxGunDepression * Facing) * Vector2.right * Facing;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + max * 5.0f);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + min * 5.0f);

        Gizmos.color = Color.red;
       // Gizmos.DrawLine(transform.position, (Vector2)transform.position + aim);

        switch (current_state) {
            case State.WALKING: Gizmos.color = Color.green;
                break;
            case State.MANOEUVRING: Gizmos.color = Color.yellow;
                break;
            case State.FIRING: Gizmos.color = Color.red;
                break;
            case State.STUNNED: Gizmos.color = Color.blue;
                break;
            case State.DEAD: Gizmos.color = Color.black;
                break;
            case State.SLEEPING: Gizmos.color = Color.grey;
                break;
            case State.IDLE: Gizmos.color = Color.cyan;
                break;
        }

        Gizmos.DrawSphere(transform.position + Vector3.up * 1.25f, 0.25f);

    }
}
