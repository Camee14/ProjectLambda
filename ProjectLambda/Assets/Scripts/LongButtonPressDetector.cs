using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class LongButtonPressDetector {
    public float threshold = 0.5f;

    List<InputControlType> listen_to;
    Dictionary<InputControlType, float> timers;

    public LongButtonPressDetector(params InputControlType[] controls) {
        timers = new Dictionary<InputControlType, float>();
        listen_to = new List<InputControlType>();

        foreach (InputControlType c in controls) {
            listen_to.Add(c);
            timers.Add(c, 0f);
        }
    }
	public void Update () {
        foreach (InputControlType c in listen_to) {
            if (InputManager.ActiveDevice.GetControl(c).IsPressed) {
                timers[c] += Time.unscaledDeltaTime;
            }

            if (InputManager.ActiveDevice.GetControl(c).WasReleased) {
                timers[c] = 0f;
            }
        }
	}
    public bool longPress(InputControlType c) {
        return timers[c] > threshold;
    }
    public bool shortPress(InputControlType c) {
        return InputManager.ActiveDevice.GetControl(c).WasReleased && timers[c] < threshold;
    }
}
