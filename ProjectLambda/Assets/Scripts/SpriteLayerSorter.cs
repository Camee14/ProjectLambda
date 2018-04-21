using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anima2D;

public class SpriteLayerSorter : MonoBehaviour {
    static List<string> LAYER_IDS;
    static List<int> LAYER_OFFSETS;
	void Start () {
        if (LAYER_IDS == null) {
            LAYER_IDS = new List<string>();
        }
        if (LAYER_OFFSETS == null) {
            LAYER_OFFSETS = new List<int>();
        }
        List<int> increment = new List<int>();
        SpriteMeshInstance[] instances = transform.GetComponentsInChildren<SpriteMeshInstance>();
        for (int i = 0; i < instances.Length; i++) {
            if (!LAYER_IDS.Contains(instances[i].sortingLayerName)) {
                LAYER_IDS.Add(instances[i].sortingLayerName);
                LAYER_OFFSETS.Add(0);
            }

            int index = LAYER_IDS.FindIndex(x => x == instances[i].sortingLayerName);
            int offset = LAYER_OFFSETS[index];

            instances[i].sortingOrder = instances[i].sortingOrder + offset;

            if (!increment.Contains(index))
            {
                increment.Add(index);
            }
        }
        for (int i = 0; i < increment.Count; i++) {
            LAYER_OFFSETS[increment[i]] += 5;
        }
	}
}
