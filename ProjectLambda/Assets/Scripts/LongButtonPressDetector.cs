using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongButtonPressDetector {
    public float threshold = 0.3f;
    public string[] Axies = { "Attack 1", "Attack 3" };

    Dictionary<string, float> timers;

    public LongButtonPressDetector() {
        timers = new Dictionary<string, float>();

        foreach (string s in Axies) {
            timers.Add(s, 0f);
        }
    }
	public void Update () {
        for(int i = 0; i < Axies.Length; i++) {
            if (Input.GetButton(Axies[i])) {
                timers[Axies[i]] += Time.unscaledDeltaTime;
            }

            if (Input.GetButtonUp(Axies[i])) {
                timers[Axies[i]] = 0f;
            }
        }
	}
    public bool longPress(string s) {
        return timers[s] > threshold;
    }
    public bool shortPress(string s) {
        return Input.GetButtonUp(s) && timers[s] < threshold;
    }
}
