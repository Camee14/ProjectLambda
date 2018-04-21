using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerHUD : MonoBehaviour {
    public Image HealthBar;
    public Image[] EnergyBar;

    Health health;
    Energy energy;

    Coroutine health_routine;

	void Start () {
        health = GetComponent<Health>();
        energy = GetComponent<Energy>();

        health.OnHealthDamaged += healthUpdate;

        energy.onUsedCharge += energyUsed;
        energy.onChargingUp += energyUpdate;
        energy.onMaxChargesChanged += maxEnergyChanged;

        GetComponent<Player>().onPlayerRespawn += reset;
        reset();
    }
    void healthUpdate(int hp, int max) {
        if (health_routine != null)
        {
            StopCoroutine(health_routine);
        }
        health_routine = StartCoroutine(doHealthUpdate());
    }
    void energyUsed(int index, int count) {
        EnergyBar[index].fillAmount = 0;
        for (int i = index; i < energy.EnergyCharges; i++) {
            EnergyBar[i].fillAmount = 0;
        }
    }
    void energyUpdate(int index, double value) {
        EnergyBar[index].fillAmount = (float)value / 100;
    }
    void maxEnergyChanged(int old_count, int new_count) {
        for (int i = old_count; i < new_count; i++) {
            EnergyBar[i].transform.parent.gameObject.SetActive(true);
            EnergyBar[i].fillAmount = 0;
        }
    }
    void reset() {
        healthUpdate(health.MaxHealth, health.MaxHealth);
        maxEnergyChanged(0, energy.EnergyCharges);
        StartCoroutine(doEnergyStart());
    }
    IEnumerator doHealthUpdate() {
        float source = HealthBar.fillAmount;
        float target = health.Percentage;
        float timer = 0;

        while (HealthBar.fillAmount != target)
        {
            HealthBar.fillAmount = Mathf.Lerp(source, target, timer);
            timer += Time.unscaledDeltaTime;
            if (timer > 1) {
                timer = 1;
            }
            yield return null;
        }
    }
    IEnumerator doEnergyStart() {
        for (int i = 0; i < energy.EnergyCharges; i++) {
            float source = 0;
            float target = 1;
            float timer = 0;
            while (EnergyBar[i].fillAmount != target)
            {
                EnergyBar[i].fillAmount = Mathf.Lerp(source, target, timer);
                timer += Time.unscaledDeltaTime;
                if (timer > 1)
                {
                    timer = 1;
                }
                yield return null;
            }
        }
    }
}
