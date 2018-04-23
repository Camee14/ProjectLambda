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
    public float ComboWindowDuration = 0.2f;
    public float MaxDashHoldTime = 3f;
    public float MaxGrappleVelocity = 35;
    public float RespawnY = -250f;

    Health health;
    Energy energy;
    public Grapple grapple;
    public Animator SwordEffectAnim;
    public Animator Body;
    public GameObject DashIndicatorPrefab;
    public GameObject AITriggerPrefab;

    Vector3 respawn_point;

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
    float combo_timer = 0f;
    float stun_timer = 0f;
    float dash_timer = 0f;
    short basic_attack_count = 0;
    short attack_charges = 0;

    bool movement_enabled = true;
    bool jump_enabled = true;
    bool jump_down_on_unpause = false;
    bool interupt_action = false;
    bool basic_attack_enabled = true;
    bool is_paused = false;

    private AudioSource soundEffect;
    public AudioClip SwordSlashSound;
    public AudioClip JumpSound;
    public AudioClip SwordDashSound;
    public AudioClip GrappleFireSound;
    public AudioClip GroundSlamSound;
    public AudioClip NoImpactSwingSound;

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

        dash_contact_filter.useTriggers = true;
        dash_contact_filter.SetLayerMask(LayerMask.GetMask("Enemy", "Projectile"));
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

        soundEffect = GetComponent<AudioSource>();
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

    public void PlaySoundEffect(AudioClip sound, float volume)
    {
        soundEffect.Stop();
        soundEffect.PlayOneShot(sound, volume);
    }
    void animateBody() {
        Body.SetBool("is_grounded", IsGrounded);
        Body.SetBool("is_falling", !IsGrounded && Velocity.y < 0);
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

       // if (!basic_attack_enabled && (IsGrounded || grapple.isGrappleConnected)) {
           // basic_attack_enabled = true;
           // basic_attack_count = 0;
        //}

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

        if (detector.shortPress(InputControlType.Action3) && attack_timer <= 0 && basic_attack_enabled)
        {
            bool special_attack = false;

            basic_attack_count++;
            if (basic_attack_count == 3)
            {
                basic_attack_enabled = false;
            }

            if (attack_charges == 2)
            {
                attack_charges = 0;
                special_attack = true;
            }

            attack_timer = BasicAttackRate;
            combo_timer = BasicAttackRate + ComboWindowDuration;

            Vector2 dir = getAimDir();
            StartCoroutine(doHangTime(dir));
            doBasicAttack(dir, special_attack);
        }
        
        if (combo_timer > 0)
        {
            combo_timer -= Time.deltaTime;
            attack_timer -= Time.deltaTime;
            if (InputManager.ActiveDevice.Action1.WasPressed)
            {
                attack_timer = 0f;
            }
        }
        else {
            basic_attack_enabled = true;
            basic_attack_count = 0;

            attack_charges = 0;
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
            if (grapple.isGrappleConnected == true)
                PlaySoundEffect(GrappleFireSound, 0.5f);
        }
        if (InputManager.ActiveDevice.Action2.WasReleased) {
            grapple.detach();
            Velocity = Velocity * 1.15f;
        }

        if (OverrideAutoFacing)
        {
            float dir = getAimDir().x;
            if (dir > 0)
            {
                Facing = 1;
            }
            else if(dir < 0)
            {
                Facing = -1;
            }
        }

        if (grapple.isGrappleConnected) {
            Velocity = Vector2.ClampMagnitude(Velocity, MaxGrappleVelocity);
        }

        if (transform.position.y < RespawnY) {
            health.instakill();
        }

        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * Facing, transform.localScale.y, transform.localScale.z);

        animateBody();

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
            Body.SetBool("is_walking", move.x != 0 && IsGrounded);
        }
        else {
            Body.SetBool("is_walking", false);
        }
        if (jump_enabled)
        {
            if (InputManager.ActiveDevice.Action1.IsPressed)
            {
                if (IsGrounded)
                {
                    Velocity = new Vector2(Velocity.x, JumpForce);
                    Body.SetTrigger("jump");
                }
            }
            else if (InputManager.ActiveDevice.Action1.WasReleased)
            {
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
        Body.SetBool("is_walking", move.x != 0);
        return move * MovementSpeed;
    }
    protected override void onDrawGizmos()
    {
        base.onDrawGizmos();

        if (!DrawDebug) {
            return;
        }
        Gizmos.DrawWireCube(transform.position + new Vector3(Facing * 1.5f, 0.5f), new Vector3(2f, 2f, 1f));
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

        Body.SetBool("is_hitting_ground", false);
        Mass = Mass / 10f;
        canUseAllControls(true);

        stun_timer = 0f;
        combo_timer = 0f;
        attack_timer = 0;
        attack_charges = 0;
        basic_attack_count = 0;
        StopAllCoroutines();
        Velocity = Vector2.zero;

        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        for (int i = 0; i < projectiles.Length; i++) {
            Destroy(projectiles[i]);
        }

        transform.position = respawn_point;

        health.reset();
        energy.reset();

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
    void doBasicAttack(Vector2 dir, bool special)
    {
        SwordEffectAnim.Play("SwordSlashDown", 0);
        Body.SetTrigger("basic_attack");
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position + new Vector3(Facing * 1.5f, 0.5f), new Vector2(2f, 2f), 0f, enemy_mask);
        if (cols.Length != 0)
        {
            attack_charges++;
            foreach (Collider2D col in cols)
            {
                IAttackable ab = col.GetComponent(typeof(IAttackable)) as IAttackable;
                if (ab != null)
                {
                    PlaySoundEffect(SwordSlashSound, 0.3f);
                    if (dir == Vector2.zero) {
                        dir = Vector2.right * Facing;
                    }
                    if (special)
                    {
                        ab.attack(40, (dir + Vector2.up).normalized, 60f, BasicAttackRate + ComboWindowDuration);
                        //PlaySoundEffect(SwordSlashSound, 0.5f);
                    }
                    else {
                        ab.attack(20, dir, 60f, BasicAttackRate + ComboWindowDuration);
                        //PlaySoundEffect(SwordSlashSound, 0.5f);
                    }
                }
            }
        }
    }
    IEnumerator doHangTime(Vector2 dir) {

        OverrideGravity = true;
        OverrideVelocityX = false;
        OverrideAutoFacing = true;
        canUseAllControls(false);

        dir.y = 0f;

        float speed = 60f;
        while (combo_timer > 0)
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
        OverrideAutoFacing = false;
        canUseAllControls(true);
    }
    IEnumerator doDashAttack(Vector2 dir) {
        OverrideGravity = true;
        health.setInvincible(true);
        Velocity = dir.normalized * 80f;

        PlaySoundEffect(SwordDashSound, 0.5f);

        Collider2D[] cols = new Collider2D[16];
        float timer = 0f;
        while (timer <= 0.14f) {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, 1f, cols, dash_contact_filter.layerMask);
            for (int i = 0; i < count; i++) {
                if (cols[i].tag == "Projectile")
                {
                    Bullet b = cols[i].gameObject.GetComponent<Bullet>();
                    if (b != null)
                    {
                        b.changeTarget();
                        b.rb2d.velocity = dir.normalized * b.rb2d.velocity.magnitude;
                        b.transform.rotation = Quaternion.LookRotation(Vector3.forward, b.rb2d.velocity);
                    }
                }
                else
                {
                    IAttackable ab = cols[i].GetComponent(typeof(IAttackable)) as IAttackable;
                    if (ab != null)
                    {
                        if (!ab.isStunned())
                        {
                            ab.knockback(Vector2.up + ((Vector2.right * Facing) * 0.5f), 18f, 1f);
                        }
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

        Body.SetBool("is_hitting_ground", true);

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
                    PlaySoundEffect(GroundSlamSound, 0.8f);
                }
            }
        }
        else {
            Velocity = Vector2.zero;
        }
        Body.SetBool("is_hitting_ground", false);
        Mass = Mass / multiplier;
        canUseAllControls(true);
    }
}

