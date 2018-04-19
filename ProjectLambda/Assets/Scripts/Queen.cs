using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
 * sin wave tutorial from: https://answers.unity.com/questions/803434/how-to-make-projectile-to-shoot-in-a-sine-wave-pat.html
 * **/

public class Queen : MonoBehaviour, IAttackable, ISpawnable
{
    enum State
    {
        SLEEPING,
        IDLE,
        SPAWNING,
        FIGHTING,
        STUNNED,
        DEAD
    };

    public float VisibilityRange = 10f;
    public float Speed = 5f;
    public float Frequency = 3f;
    public float Magnitude = 1.5f;
    public float MinClearance = 1f;
    public int MaxNumChildren = 3;
    public GameObject ShooterDrone;
    public GameObject BulletPrefab;
    public bool DrawAIDebug = false;

    bool in_combat = false;
    bool spawn_drone = false;

    IEnumerator current_coroutine;
    State current_state = State.SLEEPING;

    GameObject player;
    Transform children;

    AIFlag PatrolBoundary;
    Health health;
    ProximitySwitch detector;

    LayerMask visibility_mask;
    LayerMask walkable_mask;

    Rigidbody2D rb2d;
    Vector2 ground_pos;
    Vector2 respawn_point;

    List<Transform> active_children;
    List<float> rotations;

    int facing = -1;
    float local_time = 0;
    bool do_spin = true;
    bool reset_spawn_timer = false;
    bool is_shielded = false;

    public void attack(int dmg, Vector2 dir, float pow, float stun_time = 0)
    {
        if (current_state == State.DEAD || is_shielded)
        {
            return;
        }
        health.apply(-dmg);
    }
    public void knockback(Vector2 dir, float pow, float hang_time = 0f)
    {

    }
    public bool isInvincible()
    {
        return is_shielded;
    }
    public bool isStunned()
    {
        return false;
    }
    public void spawn(GameObject spawner) {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10, walkable_mask);
        if (hit.collider != null)
        {
            transform.position = new Vector2(hit.point.x, hit.point.y + MinClearance);
            ground_pos = hit.point;
        }
        respawn_point = transform.position;
        PatrolBoundary = spawner.transform.parent.GetComponent<AIFlag>();
    }
    public void destroyChild(Transform t) {
        int index = active_children.FindIndex(id => id.GetInstanceID() == t.GetInstanceID());
        active_children.RemoveAt(index);
        rotations.RemoveAt(index);
        Destroy(t.gameObject);

        is_shielded = (active_children.Count != 0);
        reset_spawn_timer = true;
    }
    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();

        health = GetComponent<Health>();
        health.OnCharacterDeath += die;

        detector = GetComponent<ProximitySwitch>();
        detector.onSwitchOn += activateAI;
        detector.onSwitchOff += deactivateAI;

        player = GameObject.Find("Player");
        Player p = player.GetComponent<Player>();
        p.onPlayerDeath += playerDeath;
        p.onPlayerRespawnChanged += playerRespawnChanged;

        visibility_mask = LayerMask.GetMask("Grappleable", "DynamicPlatform");
        walkable_mask = LayerMask.GetMask("Grappleable");

        children = transform.GetChild(0);
        active_children = new List<Transform>();
        rotations = new List<float>();
    }
    void FixedUpdate() {
        if (!detector.IsSwitchedOn)
        {
            return;
        }
        updateTransitionBools();
        checkStateTransitions();
        checkBoundaries();

        if (do_spin)
        {
            for (int i = 0; i < active_children.Count; i++)
            {
                doChildRotation(i, 90, true);
            }
        }
    }
    void activateAI()
    {
        if (current_state == State.DEAD)
        {
            return;
        }
        transitionToState(doIdleState(current_state));
    }
    void deactivateAI()
    {
        if (current_state == State.DEAD)
        {
            return;
        }
        StopAllCoroutines();
        current_state = State.SLEEPING;
    }
    void die()
    {
        transitionToState(doDeathState(current_state));
    }
    void playerDeath() {
        health.reset();
        transform.position = respawn_point;

        detector.reset();

        StopAllCoroutines();
        current_state = State.SLEEPING;
    }
    void playerRespawnChanged() {
        if (current_state == State.DEAD) {
            Player p = player.GetComponent<Player>();
            p.onPlayerDeath -= playerDeath;
            p.onPlayerRespawnChanged -= playerRespawnChanged;
            Destroy(gameObject);
        }
    }
    void doChildRotation(int index, float base_speed, bool do_offset) {
        float speed = base_speed;

        if (rotations.Count > 1 && index != 0)
        {
            float prev_a = 180 - Mathf.Abs(Mathf.Abs(rotations[index] - rotations[index - 1]) - 180);
            float next_a = 180 - Mathf.Abs(Mathf.Abs(rotations[index] - rotations[(index + 1) % rotations.Count]) - 180);

            if (rotations.Count == 2) {
                next_a = 360 - next_a;
            }

            float diff = prev_a - next_a;

            if (diff > 5 || diff < -5)
            {
                speed = base_speed + (prev_a - next_a);
            }
        }

        rotations[index] += speed * Time.deltaTime;
        rotations[index] = rotations[index] % 360;
        setChildPosition(active_children[index], rotations[index], do_offset);
    }
    void setChildPosition(Transform c, float a, bool do_offset) {
        float radius = 1.5f;
        if (do_offset)
        {
            local_time += Time.deltaTime;
            radius += Mathf.Sin(local_time * (30 * Time.unscaledDeltaTime)) * 0.5f;
        }
        a = a * Mathf.Deg2Rad;
        Vector3 pos = new Vector3();
        pos.x = (Mathf.Cos(a) - Mathf.Sin(a)) * radius;
        pos.y = (Mathf.Sin(a) + Mathf.Cos(a)) * radius;
        c.position = children.TransformPoint(pos);
    }
    void updateTransitionBools()
    {
        if ((player.transform.position - transform.position).magnitude <= VisibilityRange)
        {
            in_combat = Physics2D.Linecast(transform.position, player.transform.position, visibility_mask).collider == null;
        }
        else {
            in_combat = false;
        }
    }
    void checkStateTransitions()
    {
        if (current_state == State.IDLE)
        {
            if (in_combat)
            {
                if (active_children.Count == 0)
                {
                    transitionToState(doSpawningState(current_state));
                }
                else
                {
                    transitionToState(doFightingState(current_state));
                }
            }
        }
        else if (current_state == State.SPAWNING) {
            if (!spawn_drone) {
                transitionToState(doFightingState(current_state));
            }
        }
        else if (current_state == State.FIGHTING)
        {
            if (spawn_drone) {
                transitionToState(doSpawningState(current_state));
            }
            if (!in_combat)
            {
                transitionToState(doIdleState(current_state));
            }
        }
        else if (current_state == State.SLEEPING)
        {
            transitionToState(doIdleState(current_state));
        }
    }
    void transitionToState(IEnumerator next_coroutine)
    {
        if (current_coroutine != null)
        {
            StopCoroutine(current_coroutine);
        }

        current_coroutine = next_coroutine;
        StartCoroutine(current_coroutine);
    }
    void checkBoundaries()
    {
        Vector2 proj = rb2d.position + Vector2.right * facing;
        RaycastHit2D hit = Physics2D.Raycast(proj, Vector2.down, 10f, walkable_mask);
        if (hit.collider == null || !PatrolBoundary.isInBoundary(proj))
        {
            if (current_state == State.IDLE)
            {
                facing *= -1;
            }

        }
        else if (hit.collider != null)
        {
            ground_pos =  hit.point;
        }

    }
    IEnumerator doIdleState(State p_state)
    {
        current_state = State.IDLE;

        do_spin = true;

        while (true)
        {
            float offsetY = Mathf.Sin(Time.time * Frequency) * Magnitude;
            Vector2 next = rb2d.position + Vector2.right * facing;
            next.y = ground_pos.y + MinClearance + offsetY;

            rb2d.position = Vector2.MoveTowards(rb2d.position, next, Speed * Time.deltaTime);

            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator doSpawningState(State p_state) {
        spawn_drone = true;

        current_state = State.SPAWNING;
        
        do_spin = false;
        health.setInvincible(true);

        if (p_state == State.IDLE)
        {
            for (int i = 0; i < MaxNumChildren; i++)
            {
                GameObject g = Instantiate(ShooterDrone, children);
                
                active_children.Add(g.transform);
                rotations.Add((360 / MaxNumChildren) * i);

                setChildPosition(g.transform, (360 / MaxNumChildren) * i, false);

                yield return new WaitForSeconds(0.5f);
            }
            
        }
        else
        {
            float a = 0;
            if (active_children.Count > 0)
            {
                a = rotations[rotations.Count - 1] - ((360 / (active_children.Count + 1)) * active_children.Count);
            }
            
            GameObject g = Instantiate(ShooterDrone, children);
            active_children.Add(g.transform);
            rotations.Add(a);

            setChildPosition(active_children[active_children.Count - 1], a, false);
        }
        is_shielded = true;
        do_spin = true;
        health.setInvincible(false);

        spawn_drone = false;
    }
    IEnumerator doFightingState(State p_state)
    {
        current_state = State.FIGHTING;

        if (p_state == State.IDLE) {
            Vector2 attack_pos = ground_pos;
            attack_pos.y += MinClearance;
            while (rb2d.position != attack_pos)
            {
                rb2d.position = Vector2.MoveTowards(rb2d.position, attack_pos, Speed * Time.deltaTime);
                yield return new WaitForFixedUpdate();
            }
        }

        float shoot_delay = 0;
        float spawn_delay = 5f;
        while (true)
        {
            if (detector.IsVisible)
            {
                if (shoot_delay <= 0)
                {
                    Vector2 player_dir = (player.transform.position - transform.position);
                    if (player_dir.magnitude >= 3)
                    {
                        player_dir.Normalize();
                        foreach (Transform child in active_children)
                        {

                            Vector2 child_dir = (child.position - transform.position).normalized;
                            float sim = Vector2.Dot(player_dir, child_dir);

                            if (sim > 0.99f)
                            {
                                Instantiate(BulletPrefab, child.position, Quaternion.LookRotation(Vector3.forward, (player.transform.position - child.position)));
                                shoot_delay = 0.4f;
                            }
                        }
                    }
                }
                else
                {
                    shoot_delay -= Time.deltaTime;
                }
                if (active_children.Count < MaxNumChildren)
                {
                    if (reset_spawn_timer)
                    {
                        reset_spawn_timer = false;
                        spawn_delay = 5f;
                    }
                    if (spawn_delay <= 0)
                    {
                        spawn_drone = true;
                        spawn_delay = 5f;
                    }
                    else
                    {
                        spawn_delay -= Time.deltaTime;
                    }
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator doStunnedState(State p_state)
    {
        current_state = State.STUNNED;
        yield return new WaitForFixedUpdate();
    }
    IEnumerator doDeathState(State p_state)
    {
        current_state = State.DEAD;
        Vector2 velocity = new Vector2();
        RaycastHit2D[] hits = new RaycastHit2D[16];

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = true;
        filter.layerMask = walkable_mask;

        while (true)
        {
            velocity += rb2d.mass * Physics2D.gravity * Time.deltaTime;
            rb2d.position += velocity * Time.deltaTime;

            int count = rb2d.Cast(velocity, filter, hits, velocity.magnitude);
            if (count > 0) {
                break;
            }

            rb2d.position += velocity * Time.deltaTime;

            yield return new WaitForFixedUpdate();
        }
        while (true) {
            yield return new WaitForFixedUpdate();
        }
    }
    void OnDrawGizmos() {
        if (is_shielded) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, .7f);
        }
    }
}
