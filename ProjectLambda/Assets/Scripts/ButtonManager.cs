using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using InControl;

public class ButtonManager : MonoBehaviour {

    private InputDevice Controller;
    public GameObject canvas;

    private void Update()
    {
        Controller = InputManager.ActiveDevice;

        if (Controller.MenuWasPressed)
        {
            if(canvas.activeInHierarchy == false)
            {
                canvas.SetActive(true);
                Time.timeScale = 0;
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
        canvas.SetActive(false);
    }

}
