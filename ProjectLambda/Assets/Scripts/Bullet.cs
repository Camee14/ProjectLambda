using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public int damage = 20;
	void Start () {
        GetComponent<Rigidbody2D>().AddForce(transform.up * 20, ForceMode2D.Impulse);

        Destroy(gameObject, 5.0f);
	}
    void OnTriggerEnter2D(Collider2D col)
    {
        IAttackable ab = col.GetComponent(typeof(IAttackable)) as IAttackable;
        if (ab != null) {
            if (ab.isInvincible()) {
                return;
            }
            ab.attack(damage, transform.up, 10);
        }
        Destroy(gameObject);
    }
}
