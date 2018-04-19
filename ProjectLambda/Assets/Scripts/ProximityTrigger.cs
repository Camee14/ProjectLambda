using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityTrigger : MonoBehaviour {
    public Transform Target;
    void Update() {
        transform.position = Target.position;
    }
    void OnTriggerEnter2D(Collider2D col) {
        ProximitySwitch ps = col.gameObject.GetComponent<ProximitySwitch>();
        if (ps != null)
        {
            ps.setSwitchState(true, false);
        }
    }
    void OnTriggerStay2D(Collider2D col) {
        ProximitySwitch ps = col.gameObject.GetComponent<ProximitySwitch>();
        if (ps != null)
        {
            ps.setSwitchState(true, GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), col.bounds));
        }
    }
    void OnTriggerExit2D(Collider2D col) {
        ProximitySwitch ps = col.gameObject.GetComponent<ProximitySwitch>();
        if (ps != null)
        {
            ps.setSwitchState(false, false);
        }
    }
}
