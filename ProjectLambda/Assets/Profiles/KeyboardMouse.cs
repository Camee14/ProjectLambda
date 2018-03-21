using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class KeyboardMouse : UnityInputDeviceProfile {
    public KeyboardMouse() {
        Name = "Keyboard & Mouse";
        Meta = "profile for keyboard and mouse users";

        SupportedPlatforms = new[] {
            "Windows",
            "Mac",
            "Linux"
        };

        Sensitivity = 1.0f;
        LowerDeadZone = 0.0f;
        UpperDeadZone = 1.0f;

        ButtonMappings = new[] {
            new InputControlMapping{
                Handle = "Jump",
                Target = InputControlType.Action1,
                Source = KeyCodeButton(KeyCode.Space)
            },
            new InputControlMapping{
                Handle = "Grapple - Mouse",
                Target = InputControlType.Action2,
                Source = MouseButton1
            },
            new InputControlMapping{
                Handle = "Attack - Mouse",
                Target = InputControlType.Action3,
                Source = MouseButton0
            },
            new InputControlMapping{
                Handle = "Slam",
                Target = InputControlType.Action4,
                Source = KeyCodeButton(KeyCode.F)
            },
        };

        AnalogMappings = new[] {
            new InputControlMapping{
                Handle = "Move X",
                Target = InputControlType.LeftStickX,
                Source = KeyCodeAxis(KeyCode.A, KeyCode.D)
            },
            new InputControlMapping{
                Handle = "Move Y",
                Target = InputControlType.LeftStickY,
                Source = KeyCodeAxis(KeyCode.W, KeyCode.S)
            }
        };
    }
}
