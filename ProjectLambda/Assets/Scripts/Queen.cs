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
        FIGHTING,
        STUNNED,
        DEAD
    };

    public float VisibilityRange = 10f;
    public float Speed = 5f;
    public float Frequency = 3f;
    public float Magnitude = 1.5f;
    public float MinClearance = 1f;
    public bool DrawAIDebug = false;

    bool in_combat = false;

    IEnumerator current_coroutine;
    State current_state = State.SLEEPING;

    GameObject player;
    Transform children;
    
    AIFlag PatrolBoundary;
    Health health;
    ProximityDetector detector;
    LayerMask ground_mask;
    Rigidbody2D rb2d;
    Vector2 ground_pos;
    Vector2 respawn_point;
    List<Transform> active_children;

    int facing = -1;
    float angle = 0;

    public void attack(int dmg, Vector2 dir, float pow, float stun_time = 0)
    {
        
    }
    public bool isInvincible()
    {
        return false;
    }
    public bool isStunned()
    {
        return false;
    }
    public void spawn(GameObject spawner) {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10, ground_mask);
        if (hit.collider != null)
        {
            transform.position = new Vector2(hit.point.x, hit.point.y + MinClearance);
            ground_pos =  hit.point;
        }
        respawn_point = transform.position;
        PatrolBoundary = spawner.transform.parent.GetComponent<AIFlag>();
    }
    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();

        health = GetComponent<Health>();
        health.OnCharacterDeath += die;

        detector = GetComponent<ProximityDetector>();
        detector.onCameraEnter += activateAI;
        detector.onCameraExit += deactivateAI;

        player = GameObject.Find("Player");

        ground_mask = LayerMask.GetMask("Grappleable");

        children = transform.GetChild(0);
        active_children = new List<Transform>();
    }
    void FixedUpdate() {
        updateTransitionBools();
        checkStateTransitions();
        checkBoundaries();

        angle += 180 * Time.deltaTime;
        angle = angle % 360;

        float a = angle * Mathf.Deg2Rad;
        active_children.Clear();
        foreach (Transform child in children) {
            if (child.gameObject.activeInHierarchy) {
                active_children.Add(child);
            }
        }
        foreach (Transform child in active_children) {
            setChildPosition(child, a);
            a += (360 / active_children.Count) * Mathf.Deg2Rad;
        }
    }
    void activateAI()
    {
        transitionToState(doIdleState(current_state));
    }
    void deactivateAI()
    {
        StopAllCoroutines();
        current_state = State.SLEEPING;
    }
    void die()
    {
        transitionToState(doDeathState(current_state));
    }
    void setChildPosition(Transform c, float a) {
        float radius = 1.5f + Mathf.Sin(Time.time * (180 * Time.deltaTime)) * 0.5f;

        Vector3 pos = new Vector3();
        pos.x = (Mathf.Cos(a) - Mathf.Sin(a)) * radius;
        pos.y = (Mathf.Sin(a) + Mathf.Cos(a)) * radius;
        c.position = children.TransformPoint(pos);
    }
    void updateTransitionBools()
    {
        if ((player.transform.position - transform.position).magnitude <= VisibilityRange)
        {
            in_combat = Physics2D.Linecast(transform.position, player.transform.position, ground_mask).collider == null;
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
                transitionToState(doFightingState(current_state));
            }
        }
        else if (current_state == State.FIGHTING)
        {
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
        RaycastHit2D hit = Physics2D.Raycast(proj, Vector2.down, 10f, ground_mask);
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

        while (true)
        {
            float offsetY = Mathf.Sin(Time.time * Frequency) * Magnitude;
            Vector2 next = rb2d.position + Vector2.right * facing;
            next.y = ground_pos.y + MinClearance + offsetY;

            rb2d.position = Vector2.MoveTowards(rb2d.position, next, Speed * Time.deltaTime);

            yield return null;
        }
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
                yield return null;
            }
        }

        for (int i = 0; i < children.childCount; i++) {
            if (children.GetChild(i).gameObject.activeInHierarchy) {
                continue;
            }
            children.GetChild(i).gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
        }
        
        while (true)
        {
            yield return null;
        }
    }
    IEnumerator doStunnedState(State p_state)
    {
        current_state = State.STUNNED;
        yield return null;
    }
    IEnumerator doDeathState(State p_state)
    {
        current_state = State.DEAD;
        yield return null;
    }
}
/*public class Queen : MonoBehaviour, IAttackable {

    public float VisibilityRange = 10f;
    public float Speed = 5f;
    public float Frequency = 3f;
    public float Magnitude = 1.5f;
    public int MaxChildren = 3;

    public AIFlag PatrolBoundary;

    public GameObject satelite_prefab;

    GameObject player;

    int num_children;
    GameObject[] children;

    Rigidbody2D rb2d;
    Vector2 target_pos;
    Vector2 true_pos;

    LayerMask floor_mask;
    LayerMask player_mask;

    int facing = -1;
    float agro_timer;
    float child_spawn_timer;
    float vis_range;

    //states
    bool alert = false; //the enemy has seen the player
    bool attacking = false; //the enemy is in position to attack
    bool has_los = false; //enemy has line of sight

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        target_pos = true_pos = rb2d.position;

        floor_mask = LayerMask.GetMask("Grappleable");
        player_mask = LayerMask.GetMask("Player", "Grappleable");

        player = GameObject.Find("Player");

        children = new GameObject[MaxChildren];

        child_spawn_timer = 2f;
        num_children = 0;

        vis_range = VisibilityRange;
	}
    void Update() {
        if (alert && attacking)
        {
            if (child_spawn_timer > 3f && num_children < MaxChildren)
            {
                int index = num_children > 0 ? num_children - 1 : 0; 
                children[index] = Instantiate(satelite_prefab, transform.position + Vector3.left * 3f, Quaternion.identity, transform);
                num_children++;
                child_spawn_timer = 0f;
            }
            else
            {
                child_spawn_timer += Time.deltaTime;
            }
        }
    }
    void FixedUpdate() {
        if (!alert) //if the player hasnt been seen, do idle
        {
            move();
        }
        else if(alert && !has_los) { //if the player has been seen, but we have lost line of sight, tick down agro
            agro_timer += Time.deltaTime;
            if (agro_timer >= 3f) {
                alert = false;
                Debug.Log("go back to idle");
                agro_timer = 0f;
            }
        }

        if (player != null)
        {
            detect();
        }
        else {
            Debug.LogWarning("no player in scene");
        }
    }
	void move() {
        true_pos = Vector2.MoveTowards(true_pos, target_pos, Speed * Time.deltaTime);

        Vector2 proj = rb2d.position + Vector2.right * facing;
        if (true_pos.x == target_pos.x)
        {
            RaycastHit2D hit = Physics2D.Raycast(proj, Vector2.down, 10f, floor_mask);
            if (hit.collider != null)
            {
                target_pos = hit.point;
                target_pos.y += 3.5f;
            }
            else
            {
                facing *= -1;
            }
        }
        rb2d.position = true_pos;

        if (!PatrolBoundary.isInBoundary(proj)) {
            facing *= -1;
        }

        Vector2 offsetY = new Vector2(0, Mathf.Sin(Time.time * Frequency) * Magnitude);
        rb2d.position += offsetY;
    }
    void detect() {
        RaycastHit2D hit = Physics2D.Raycast(rb2d.position, ((Vector2)player.transform.position) - rb2d.position, vis_range, player_mask);
        if (hit.collider != null)
        {
            if (!alert && hit.transform.gameObject == player)
            {
                alert = true;
                has_los = true;
                vis_range = VisibilityRange * 1.3f;
                Debug.Log("target aquired");

                StartCoroutine(setupAttack());
            }
            else if (alert && hit.transform.gameObject != player)
            {
                has_los = false;
                attacking = false;
                vis_range = VisibilityRange / 1.3f;
                Debug.Log("target lost");
            }
        }
        else if(hit.collider == null && attacking) {
            has_los = false;
            attacking = false;
            vis_range = VisibilityRange / 1.3f;
            Debug.Log("cleanup");
        }
    }
    public bool isStunned() {
        return false;
    }
    public bool isInvincible() {
        return false;
    }
    public void attack(int dmg, Vector2 dir, float pow, float stun_time = 0f) {

    }
    IEnumerator setupAttack() {
        RaycastHit2D hit_up = Physics2D.Raycast(rb2d.position, Vector2.up, 10f, floor_mask);
        RaycastHit2D hit_down = Physics2D.Raycast(rb2d.position, -Vector2.up, 10f, floor_mask);

        Vector2 ground = rb2d.position;

        float top_dist, bottom_dist;
        top_dist = bottom_dist = 7.5f;

        if (hit_up.collider != null) {
            top_dist = hit_up.distance;
        }
        if (hit_down.collider != null)
        {
            bottom_dist = hit_down.distance;
            ground.y = hit_down.point.y;
        }

        ground.y += (top_dist + bottom_dist) / 2;
        while (rb2d.position != ground)
        {
            rb2d.position = Vector2.MoveTowards(rb2d.position, ground, Speed * 1.5f * Time.deltaTime);
            yield return null;
        }
        attacking = true;
        Debug.Log("ready to attack");
    }
}*/
