using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityDetector : MonoBehaviour {
    Collider2D col;

    bool prev_visible = false;
    bool is_visible = false;

    public bool IsVisible {
        get { return is_visible; }
    }

    public delegate void IsVisibleEvent();
    public delegate void IsNotVisibleEvent();

    public event IsVisibleEvent onCameraEnter;
    public event IsNotVisibleEvent onCameraExit;

    void Start() {
        col = GetComponent<Collider2D>();
    }
    void FixedUpdate()
    {
        prev_visible = is_visible;
        is_visible = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), col.bounds);

        if (is_visible && !prev_visible && onCameraEnter != null) {
            onCameraEnter();
        } else if (!is_visible && prev_visible && onCameraExit != null) {
            onCameraExit();
        }
    }
}
