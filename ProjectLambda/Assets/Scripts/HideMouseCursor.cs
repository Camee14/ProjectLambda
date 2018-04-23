using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class HideMouseCursor : MonoBehaviour {

	void Start () {
        Cursor.visible = (InputManager.ActiveDevice.Name == "Keyboard & Mouse");
        InputManager.OnActiveDeviceChanged += deviceChanged;
	}
    void deviceChanged(InputDevice active)
    {
        Cursor.visible = (active.Name == "Keyboard & Mouse");
    }
}
