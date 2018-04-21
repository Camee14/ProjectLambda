using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public Rigidbody2D rb2d;
    public int damage = 20;
    public Sprite Alternate;

    bool player_is_target = true;
	void Start () {
        rb2d.AddForce(transform.up * 20, ForceMode2D.Impulse);

        Destroy(gameObject, 5.0f);
	}
    public void changeTarget() {
        player_is_target = false;
        damage = 50;

        GetComponent<SpriteRenderer>().sprite = Alternate;
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if ((col.tag == "Player" && !player_is_target) || (col.tag == "Enemy" && player_is_target)) {
            return;
        }
        IAttackable ab = col.GetComponent(typeof(IAttackable)) as IAttackable;
        if (ab != null) {
            if (ab.isInvincible()) {
                return;
            }
            ab.attack(damage, transform.up, 10, 0.4f);
        }
        Destroy(gameObject);
    }
}
