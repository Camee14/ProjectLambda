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
    short wait_for_reset;

    bool p_state;

    void Awake() {
        canvas = GetComponent<Canvas>();

        canvas.enabled = false;
        p_state = false;
    }

    void Update()
    {
        if (onMenuDisplayChanged != null && (p_state != canvas.enabled))
        {
            onMenuDisplayChanged(canvas.enabled);
            p_state = canvas.enabled;
        }


        if (InputManager.ActiveDevice.MenuWasPressed)
        {
            canvas.enabled = !canvas.enabled;
            Time.timeScale = canvas.enabled ? 0 : 1;
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
        wait_for_reset = 3;
    }
}
