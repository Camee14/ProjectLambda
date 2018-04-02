using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour {
    public int EnergyCharges = 3;
    public double BaseRechargeRate = 5;
    List<double> charges;
    int full_charges = 0;

    void Awake () {
        charges = new List<double>();
        for(int i = 0; i < EnergyCharges; i++) {
            charges.Add(100);
        }
        full_charges = 3;
	}
    public bool hasCharge() {
        return full_charges > 0;
    }
    public bool consumeCharges(int num) {
        if (num > full_charges) {
            return false;
        }
        int index = (full_charges - 1) - (num - 1);
        charges.RemoveRange(index, num);
        full_charges -= num;

        return true;
    }
    void Update() {
        if (full_charges == charges.Count && full_charges < EnergyCharges)
        {
            charges.Add(0);
        }
        else if (full_charges < charges.Count) {
            int last_index = charges.Count - 1;
            charges[last_index] += BaseRechargeRate * Time.deltaTime;
            if (charges[last_index] >= 100) {
                charges[last_index] = 100;
                full_charges++;
            }
        }
    }
    void OnDrawGizmos() {
        if (charges == null) {
            return;
        }
        Gizmos.color = Color.blue;
        for(int i = 0; i < charges.Count; i++) {
            Gizmos.DrawWireCube(transform.position + Vector3.up + Vector3.right * i, new Vector3(0.5f, 0.25f, 1f));
            Gizmos.DrawCube(transform.position + Vector3.up + Vector3.right * i, new Vector3((float)(0.5 * (charges[i] / 100)), 0.25f , 1f));
        }
    }
}
