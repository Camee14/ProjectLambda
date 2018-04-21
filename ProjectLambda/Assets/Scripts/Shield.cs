using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {
    public float MaxScale = 5f;
    public float ToggleSpeed = 5f;

    Vector3 max;

    bool is_active = false;
    bool toggle = false;

    public bool isActive{
        get { return is_active; }
    }
    public void setActive(bool active) {
        if ((active & !is_active) || (!active && is_active)) {
            toggle = true;
        }
    }
    void Start() {
        max = new Vector3(MaxScale, MaxScale, MaxScale);
    }
    void Update() {
        transform.Rotate(new Vector3(0, 6 * Time.deltaTime, 0));
        if (!toggle) {
            return;
        }

        if (is_active)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, ToggleSpeed * Time.deltaTime);
            if (transform.localScale == Vector3.zero) {
                is_active = false;
                toggle = false;
            }
        }
        else
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, max, ToggleSpeed * Time.deltaTime);
            if (transform.localScale == max)
            {
                is_active = true;
                toggle = false;
            }
        }
    }
}
