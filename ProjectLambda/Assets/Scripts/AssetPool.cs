using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetPool : MonoBehaviour {
    Dictionary<string, List<GameObject>> pools;
    void Awake() {

    }
    public static void addToPool(GameObject g) {
        Debug.Log(g.name);
    }
}
