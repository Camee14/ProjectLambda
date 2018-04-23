using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour {

    private GameObject Music;

	// Use this for initialization
	void Start () {
        if (this.gameObject.name == "Background Music")
        {
            GameObject[] musicObj = GameObject.FindGameObjectsWithTag("Music");
            if (musicObj.Length > 1)
                Destroy(this.gameObject);

            DontDestroyOnLoad(this.gameObject);
        }

        /*if(this.gameObject.name == "InControl")
        {
            GameObject[] InputObj = GameObject.FindGameObjectsWithTag("InControl");
            if (InputObj.Length > 1)
                Destroy(this.gameObject);

            DontDestroyOnLoad(this.gameObject);
        }*/
    }

}
