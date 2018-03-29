using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawnerBox : MonoBehaviour {
    public GameObject Spawn;
    public Texture Icon;

    void Awake() {
        GameObject g = Instantiate(Spawn, transform.position, Quaternion.identity);
        ISpawnable sp = g.GetComponent(typeof(ISpawnable)) as ISpawnable;
        sp.spawn(gameObject);
    }
}
