using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftFollow : MonoBehaviour {

    public Transform Target;
    public float Threshold = 0.1f;
    public float height = -10;
    Vector3 p_pos;
    Vector3 target_pos;

    void Start() {
        p_pos = Target.position;
        p_pos.z = height;
        transform.position = p_pos;
        target_pos = p_pos;
    }
	void Update () {

        if ((Target.position - p_pos).magnitude > Threshold) {
            p_pos = Target.position;
            p_pos.z = height;
            target_pos = p_pos;
        }
        p_pos = Target.position;

        transform.position = Vector3.Lerp(transform.position, target_pos, 30 * Time.deltaTime);
    }
}
