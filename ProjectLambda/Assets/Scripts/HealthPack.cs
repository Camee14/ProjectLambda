using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour {

    static Player p;

    public GameObject HealthOrb;
    public int NumOrbsToSpawn = 4;

    SpriteRenderer sprite;
    BoxCollider2D box;
    bool isEnabled() {
        return sprite.enabled;
    }
    void Awake() {
        if (p == null)
        {
            p = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }
        p.onPlayerRespawnChanged += remove;
        p.onPlayerDeath += reset;

        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();

    }
    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player") {
            for (int i = 0; i < NumOrbsToSpawn; i++) {
                Instantiate(HealthOrb, transform.position, Quaternion.identity);
            }
            setEnabled(false);
        }
    }
    void setEnabled(bool state) {
        sprite.enabled = state;
        box.enabled = state;
    }
    void reset() {
        setEnabled(true);
    }
    void remove() {
        if (!isEnabled())
        {
            p.onPlayerRespawnChanged -= remove;
            p.onPlayerDeath -= reset;
            Destroy(gameObject);
        }
    }

}
