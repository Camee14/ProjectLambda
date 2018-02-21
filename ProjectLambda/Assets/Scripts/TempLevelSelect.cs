using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempLevelSelect : MonoBehaviour {
    int num_scenes;
    int current_scene;
    void Awake() {
        num_scenes =  SceneManager.sceneCountInBuildSettings;
        current_scene = SceneManager.GetActiveScene().buildIndex;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            current_scene++;
            if (current_scene == num_scenes) {
                current_scene = 0;
            }
            StartCoroutine(LoadYourAsyncScene(current_scene));
        }
    }

    IEnumerator LoadYourAsyncScene(int index)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
