using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using InControl;

public class ButtonManager : MonoBehaviour {
    public Canvas PauseMenu;
    public Canvas EndOfLevel;
    public bool CanvasVisibleAtStart = true;

    public delegate void MenuDisplayEvent(bool state);

    public event MenuDisplayEvent onMenuDisplayChanged;

    bool can_pause = true;
    bool p_state;

    void Awake() {
        if (PauseMenu == null)
        {
            return;
        }
        PauseMenu.enabled = CanvasVisibleAtStart;
        p_state = false;
    }

    void Update()
    {
        if (PauseMenu == null) {
            return;
        }
        if (onMenuDisplayChanged != null && (p_state != PauseMenu.enabled))
        {
            onMenuDisplayChanged(PauseMenu.enabled);
            p_state = PauseMenu.enabled;
        }


        if (InputManager.ActiveDevice.MenuWasPressed && can_pause)
        {
            PauseMenu.enabled = !PauseMenu.enabled;
            Time.timeScale = PauseMenu.enabled ? 0 : 1;
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
        PauseMenu.enabled = false;
    }
    public void showEndOfLevelUI() {
        can_pause = false;
        EndOfLevel.enabled = true;

        if (onMenuDisplayChanged != null)
        {
            onMenuDisplayChanged(true);
        }

        Time.timeScale = 0;
    }
}
