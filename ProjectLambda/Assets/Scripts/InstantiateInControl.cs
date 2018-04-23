using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateInControl : MonoBehaviour {

    public GameObject Input;
    GameObject newInput;

	// Use this for initialization
	void Start () {
        GameObject[] InputObj = GameObject.FindGameObjectsWithTag("InControl");
        if (InputObj.Length < 1)
            newInput = Instantiate(Input);

        GameObject[] managerObj = GameObject.FindGameObjectsWithTag("InputManager");
        if (managerObj.Length > 1)
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
    }
}
