using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InControl;

public class Player : CustomPhysicsObject, IAttackable
{
    public float MovementSpeed = 7f;
    public float JumpForce = 7f;
    public float BasicAttackRate = 3f;
    public float MaxHangTime = 1f;
    public float MaxDashHoldTime = 3f;
    public float MaxGrappleVelocity = 35;
    public float RespawnY = -250f;

    Health health;
    Energy energy;
    public Grapple grapple;
    public GameObject DashIndicatorPrefab;
    public GameObject AITriggerPrefab;

    public Vector3 respawn_point;
    //made that public so I could make sure the checkpoints were working

    public delegate void PlayerDeathEvent();
    public delegate void PlayerRespawnChangedEvent();
    public delegate void PlayerRespawnEvent();

    public event PlayerDeathEvent onPlayerDeath;
    public event PlayerRespawnChangedEvent onPlayerRespawnChanged;
    public event PlayerRespawnChangedEvent onPlayerRespawn;

    LongButtonPressDetector detector;

    ContactFilter2D dash_contact_filter;
    LayerMask enemy_mask;

    SpriteRenderer DashIndicator;

    float attack_timer = 0f;
    float hang_timer = 0f;
    float stun_timer = 0f;
    float dash_timer = 0f;
    short basic_attack_count = 0;
    short attack_charges = 0;

    //bool did_grapple_jump = false;
    bool movement_enabled = true;
    bool jump_enabled = true;
    bool jump_down_on_unpause = false;
    bool interupt_action = false;
    bool basic_attack_enabled = true;
    bool is_paused = false;

    public void attack(int dmg, Vector2 dir, float pow, float stun_time) {
        if (grapple.isGrappleConnected) {
            grapple.detach();
        }
        
        if (health.apply(-dmg))
        {
            return;
        }

        health.setInvincible(true);
        basic_attack_enabled = false;
        OverrideVelocityX = false;

        Velocity = dir.normalized * pow;
        stun_timer = stun_time;
    }
    public void knockback(Vector2 dir, float pow, float hang_time = 0f)
    {

    }
    public bool isStunned() {
        return stun_timer > 0;
    }
    public bool isInvincible() {
        return health.IsInvincible;
    }
    protected override void awake()
    {
        base.awake();

        detector = new LongButtonPressDetector(InputControlType.Action3, InputControlType.Action2);

        health = GetComponent<Health>();
        energy = GetComponent<Energy>();

        enemy_mask = LayerMask.GetMask("Enemy");

        dash_contact_filter.useTriggers = false;
        dash_contact_filter.SetLayerMask(enemy_mask);
        dash_contact_filter.useLayerMask = true;

        health.OnHealthDamaged += healthDamaged;
        health.OnCharacterDeath += die;

        GameObject.FindGameObjectWithTag("Menu").GetComponent<ButtonManager>().onMenuDisplayChanged += onMenuDisplayChanged;

        InputManager.OnActiveDeviceChanged += onActiveDeviceChanged;

        respawn_point = transform.position;

        grapple.setParent(this);

        GameObject dash = Instantiate(DashIndicatorPrefab);
        DashIndicator = dash.GetComponent<SpriteRenderer>();

        GameObject ai_trigger = Instantiate(AITriggerPrefab);
        ai_trigger.GetComponent<ProximityTrigger>().Target = transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //just sets the respawn point to the position of the last checkpoint reached
        if (other.tag == "CheckPoint" && other.transform.position != respawn_point)
        {
            respawn_point = other.transform.position;
            if (onPlayerRespawnChanged != null) {
                onPlayerRespawnChanged();
            }
        }
    }

    protected override void update()
    {
        if (is_paused)
        {
            return;
        }

        base.update();

        if (stun_timer > 0) {
            stun_timer -= Time.deltaTime;
            if (stun_timer <= 0) {
                health.setInvincible(false);
                OverrideVelocityX = true;
            }
        }

        if (!basic_attack_enabled && (IsGrounded || grapple.isGrappleConnected)) {
            basic_attack_enabled = true;
        }

        if (detector.longPress(InputControlType.Action3) && energy.hasCharge())
        {
            if (dash_timer == 0f)
            {
                setBulletTime(true);
                OverrideVelocityX = false;
            }

            if (dash_timer < MaxDashHoldTime)
            {
                Vector2 aim = getAimDir();
                if (aim.sqrMagnitude != 0)
                {
                    DashIndicator.enabled = true;
                    DashIndicator.transform.rotation = Quaternion.LookRotation(Vector3.forward, aim);
                    DashIndicator.transform.position = (Vector2)transform.position + aim * 12f;
                }
                else
                {
                    DashIndicator.enabled = false;
                }
            }else {
                DashIndicator.enabled = false;
                setBulletTime(false);
                OverrideVelocityX = true;
            }

            dash_timer += Time.unscaledDeltaTime;

            if (InputManager.ActiveDevice.Action3.WasReleased)
            {
                if (dash_timer < MaxDashHoldTime)
                {
                    DashIndicator.enabled = false;

                    energy.consumeCharges(1);
                    StartCoroutine(doDashAttack(getAimDir()));

                    setBulletTime(false);
                }

                dash_timer = 0f;
            }
        }
        else if (detector.shortPress(InputControlType.Action3) && attack_timer <= 0 && basic_attack_count < 4 && basic_attack_enabled) {
            basic_attack_count++;

            if (hang_timer > 0 && hang_timer - (MaxHangTime * 0.75) <= 0)
            {
                attack_charges++;
            }

            attack_timer = BasicAttackRate;
            hang_timer = BasicAttackRate + MaxHangTime;

            interupt_action = false;

            Vector2 dir = getAimDir();
            StartCoroutine(doHangTime(dir));
            doBasicAttack(dir);
        }

        if (attack_timer > 0) {
            attack_timer -= Time.deltaTime;
        }
        if (hang_timer > 0) {
            hang_timer -= Time.deltaTime;
            if (InputManager.ActiveDevice.Action1.WasPressed) {
                hang_timer = 0f;
            }
            if (hang_timer <= 0) {
                if (basic_attack_count == 4)
                {
                    basic_attack_enabled = false;
                }
                interupt_action = true;

                basic_attack_count = 0;
                attack_charges = 0;
            }
        }
        if (InputManager.ActiveDevice.Action4.WasPressed && !IsGrounded && energy.consumeCharges(1)) {
            interupt_action = false;
            StartCoroutine(doGroundSlam());
        }
        if (InputManager.ActiveDevice.Action4.WasReleased) {
            interupt_action = true;
        }

        if (InputManager.ActiveDevice.Action2.WasPressed)
        {
            grapple.fire();
        }
        if (InputManager.ActiveDevice.Action2.WasReleased) {
            grapple.detach();
            Velocity = Velocity * 1.15f;
        }

        if (OverrideAutoFacing)
        {
            float dir = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x;
            if (dir >= 0)
            {
                dir = 1f;
            }
            else
            {
                dir = -1f;
            }
            Facing = dir;
        }

        if (grapple.isGrappleConnected) {
            Velocity = Vector2.ClampMagnitude(Velocity, MaxGrappleVelocity);
        }

        if (transform.position.y < RespawnY) {
            health.instakill();
        }

        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * Facing, transform.localScale.y, transform.localScale.z);

        detector.Update();
    }
    protected override void fixedUpdate()
    {
        if (is_paused)
        {
            return;
        }

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
        if (is_paused)
        {
            return move;
        }

        if (movement_enabled)
        {
            move.x = InputManager.ActiveDevice.LeftStickX.Value;
        }
        if (jump_enabled)
        {
            if (InputManager.ActiveDevice.Action1.IsPressed)
            {
                if (IsGrounded)
                {
                    Velocity = new Vector2(Velocity.x, JumpForce);
                }
                /*else if (grapple.isGrappleConnected)
                {
                    grapple.detach();
                    releaseTether();
                    if (Velocity.magnitude <= 1f)
                    {
                        Velocity = new Vector2(Velocity.x, JumpForce);
                    }
                    did_grapple_jump = true;
                }*/
            }
            else if (InputManager.ActiveDevice.Action1.WasReleased)
            {
                /*if (did_grapple_jump)
                {
                    did_grapple_jump = false;
                }
                else*/
                if (Velocity.y > 0)
                {
                    Velocity = new Vector2(Velocity.x, Velocity.y * 0.5f);
                }
            }
        }
        else {
            if (jump_down_on_unpause && InputManager.ActiveDevice.Action1.WasReleased) {
                jump_enabled = true;
                jump_down_on_unpause = false;
            }
        }
        return move * MovementSpeed;
    }
    protected override void onDrawGizmos()
    {
        base.onDrawGizmos();

        if (!DrawDebug) {
            return;
        }

        float percentage = (attack_timer / BasicAttackRate);
        if (percentage < 0) {
            percentage = 0;
        }
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position + transform.up * 2, new Vector3(3f * percentage, 1f, 1f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + transform.up * 2, new Vector3(3f, 1f, 1f));
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

    }
    void die() {
        grapple.detach();

        OverrideGravity = false;
        OverrideVelocityX = true;
        canUseAllControls(true);

        if (onPlayerDeath != null) {
            onPlayerDeath();
        }

        stun_timer = 0f;
        Velocity = Vector2.zero;

        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        for (int i = 0; i < projectiles.Length; i++) {
            Destroy(projectiles[i]);
        }

        transform.position = respawn_point;

        health.reset();

        if (onPlayerRespawn != null) {
            onPlayerRespawn();
        }
    }
    void onActiveDeviceChanged(InputDevice active) {
        OverrideAutoFacing = (active.Name == "Keyboard & Mouse");
    }
    void onMenuDisplayChanged(bool enabled) {
        is_paused = enabled;
        if (!is_paused) {
            if (InputManager.ActiveDevice.Action1.IsPressed) {
                jump_down_on_unpause = true;
                jump_enabled = false;
            }
        }
    }
    Vector2 getAimDir() {
        return InputManager.ActiveDevice.Name == "Keyboard & Mouse" ? (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized : InputManager.ActiveDevice.LeftStick.Vector.normalized;
    }
    void doBasicAttack(Vector2 dir)
    {
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position + (Vector3.right * 2) * Facing, new Vector2(3f, 3), 0f, enemy_mask);
        if (cols.Length != 0)
        {
            foreach (Collider2D col in cols)
            {
                IAttackable ab = col.GetComponent(typeof(IAttackable)) as IAttackable;
                if (ab != null)
                {
                    if (dir == Vector2.zero) {
                        dir = Vector2.right * Facing;
                    }
                    if (attack_charges == 3)
                    {
                        ab.attack(40, (dir + Vector2.up).normalized, 60f, BasicAttackRate + MaxHangTime);
                    }
                    else {
                        ab.attack(20, dir, 60f, BasicAttackRate + MaxHangTime);
                    }
                }
            }
        }
    }
    IEnumerator doHangTime(Vector2 dir) {

        OverrideGravity = true;
        OverrideVelocityX = false;
        canUseAllControls(false);

        dir.y = 0f;

        float speed = 60f;
        while (!interupt_action)
        {
            Velocity = dir.normalized * speed;
            speed -= 1200 * Time.deltaTime;
            if (speed < 0) {
                speed = 0;
            }
            yield return new WaitForFixedUpdate();
        }

        OverrideGravity = false;
        OverrideVelocityX = true;
        canUseAllControls(true);
    }
    IEnumerator doDashAttack(Vector2 dir) {
        OverrideGravity = true;
        health.setInvincible(true);
        Velocity = dir.normalized * 80f;

        Collider2D[] cols = new Collider2D[16];
        float timer = 0f;
        while (timer <= 0.14f) {
            int count = rb2d.OverlapCollider(dash_contact_filter, cols);
            for (int i = 0; i < count; i++) {
                IAttackable ab = cols[i].GetComponent(typeof(IAttackable)) as IAttackable;
                if (ab != null)
                {
                    if (!ab.isStunned())
                    {
                        ab.knockback(Vector2.up + ((Vector2.right * Facing) * 0.5f), 18f, 1f);
                    }
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }
        Velocity = dir.normalized * 10f;

        OverrideGravity = false;
        OverrideVelocityX = true;
        health.setInvincible(false);
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

            }
            yield return null;
        }

        if (!interupt_action)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, 10f, enemy_mask);
            foreach (Collider2D col in cols)
            {
                IAttackable ab = col.GetComponent(typeof(IAttackable)) as IAttackable;
                if (ab != null)
                {
                    float mag = 1f - ((col.transform.position - transform.position).magnitude / 10f);
                    ab.knockback(Vector2.up, 20f * mag, 1f);
                }
            }
        }
        else {
            Velocity = Vector2.zero;
        }

        Mass = Mass / multiplier;
        canUseAllControls(true);
    }
}

