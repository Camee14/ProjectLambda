using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEndLevel : MonoBehaviour {
    public ButtonManager button;
    void OnTriggerEnter2D(Collider2D col) {
        button.showEndOfLevelUI();
    }
}
