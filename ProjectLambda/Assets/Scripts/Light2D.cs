using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light2D : MonoBehaviour {
    public float MinVariance = 0.1f;
    public float MaxVariance = 0.2f;

    public float MinTransitionTime = 1f;
    public float MaxTransitionTime = 6f;

    SpriteRenderer sr;
    bool reduce = false;
    Color target;
    Color original;

    float init_val;
    float end_time;
    float timer;

	void Start () {
        sr = GetComponent<SpriteRenderer>();
        target = sr.color;
        original = sr.color;

        init_val = sr.color.a;
    }
	void Update () {
        if (Mathf.Approximately(sr.color.a, target.a))
        {
            if (reduce)
            {
                target = new Color(sr.color.r, sr.color.g, sr.color.b, init_val - Random.Range(MinVariance, MaxVariance));
            }
            else {
                target = new Color(sr.color.r, sr.color.g, sr.color.b, init_val + Random.Range(MinVariance, MaxVariance));
            }
            reduce = !reduce;

            original = sr.color;
            end_time = Random.Range(MinTransitionTime, MaxTransitionTime);
            timer = 0;
        }
        sr.color = Color.Lerp(original, target, timer / end_time);
        timer += Time.deltaTime;
	}
}
