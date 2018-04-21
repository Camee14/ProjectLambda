using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointColour : MonoBehaviour {

    public bool hasReachedCheckpoint;
    private SpriteRenderer checkPointRenderer;
    public Sprite CheckpointActivated;

    // Use this for initialization
    void Start()
    {
        checkPointRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            hasReachedCheckpoint = true;
            checkPointRenderer.sprite = CheckpointActivated;
            //checkPointRenderer.color = new Color(0, 204, 204);
        }
    }
}
