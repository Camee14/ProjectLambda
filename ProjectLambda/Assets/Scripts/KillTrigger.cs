using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D col)
    {
        Health h = col.GetComponent<Health>();
        if (h != null) {
            h.instakill();
        }
    }
}
