using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using InControl;

public class ButtonManager : MonoBehaviour {

    public delegate void MenuDisplayEvent(bool state);

    public event MenuDisplayEvent onMenuDisplayChanged;

    Canvas canvas;

    void Awake() {
        canvas = GetComponent<Canvas>();

        canvas.enabled = false;
    }

    void Update()
    {
        if (InputManager.ActiveDevice.MenuWasPressed)
        {
            canvas.enabled = !canvas.enabled;
            Time.timeScale = canvas.enabled ? 0 : 1;
            if (onMenuDisplayChanged != null) {
                onMenuDisplayChanged(canvas.enabled);
            }
        }
    }

    public void StartBtnPress(string level)
    {
        SceneManager.LoadScene(level);
    }
    public void ExitBtnPress()
    {
        Application.Quit();
        Debug.Log("You have quit the game");
    }
    public void resumeBtnPress()
    {
        Time.timeScale = 1;
        canvas.enabled = false;
        if (onMenuDisplayChanged != null)
        {
            onMenuDisplayChanged(canvas.enabled);
        }
    }

}
