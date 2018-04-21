using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour {
    public int EnergyCharges = 3;
    public double BaseRechargeRate = 5;
    public bool InfiniteCharges = false;

    public delegate void ChargesUsedEvent(int index, int count);
    public delegate void MaxChargesChangedEvent(int old_count, int new_count);
    public delegate void ChargingEvent(int index, double value);

    public event ChargesUsedEvent onUsedCharge;
    public event MaxChargesChangedEvent onMaxChargesChanged;
    public event ChargingEvent onChargingUp;
  
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
        return full_charges > 0 || InfiniteCharges;
    }
    public bool consumeCharges(int num) {
        if (InfiniteCharges) {
            return true;
        }
        if (num > full_charges) {
            return false;
        }
        int index = (full_charges - 1) - (num - 1);
        charges.RemoveRange(index, num);
        full_charges -= num;

        if (onUsedCharge != null) {
            onUsedCharge(index, num);
        }

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
            if (onChargingUp != null) {
                onChargingUp(last_index, charges[last_index]);
            }
        }
    }
    void OnDrawGizmos() {
        if (charges == null) {
            return;
        }
        Gizmos.color = Color.blue;
        for(int i = 0; i < charges.Count; i++) {
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2 + Vector3.left + Vector3.right * i, new Vector3(0.5f, 0.25f, 1f));
            Gizmos.DrawCube(transform.position + Vector3.up * 2 + Vector3.left + Vector3.right * i, new Vector3((float)(0.5 * (charges[i] / 100)), 0.25f , 1f));
        }
    }
}
