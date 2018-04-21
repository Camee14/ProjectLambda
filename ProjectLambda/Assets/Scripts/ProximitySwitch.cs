using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximitySwitch : MonoBehaviour {

    bool is_visible = false;
    bool is_switched_on = false;

    public bool IsVisible
    {
        get { return is_visible; }
    }
    public bool IsSwitchedOn
    {
        get { return is_switched_on; }
    }

    public delegate void IsVisibleEvent();
    public delegate void IsNotVisibleEvent();

    public delegate void IsSwitchedOnEvent();
    public delegate void IsSwitchedOffEvent();

    public event IsVisibleEvent onCameraEnter;
    public event IsNotVisibleEvent onCameraExit;

    public event IsVisibleEvent onSwitchOn;
    public event IsNotVisibleEvent onSwitchOff;
    public void reset() {
        is_visible = false;
        is_switched_on = false;
    }
    public void setSwitchState(bool switch_state, bool on_screen)
    {
        if (switch_state && !is_switched_on)
        {
            if (onSwitchOn != null)
            {
                onSwitchOn();
            }
        }
        else if (!switch_state && is_switched_on)
        {
            if (onSwitchOff != null)
            {
                onSwitchOff();
            }
        }

        if (on_screen && !is_visible)
        {
            if (onCameraEnter != null)
            {
                onCameraEnter();
            }
        }
        else if(!on_screen && is_visible)
        {
            if (onCameraExit != null)
            {
                onCameraExit();
            }
        }

        is_switched_on = switch_state;
        is_visible = on_screen;
    }
}
