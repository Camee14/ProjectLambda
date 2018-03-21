using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFlag : MonoBehaviour {
    public float Radius = 10f;

    public Color Colour = Color.cyan;
    public Texture Icon;


    public bool isInBoundary(Vector2 pos) {
        return (pos - (Vector2)transform.position).magnitude < Radius;
    }
}
