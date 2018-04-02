using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {
    public int MaxHealth = 100;
    public bool Invincibility = false;

    public int current_health;
    bool is_invincible = false;

    public delegate void HealthDamagedEvent(int hp, int max);
    public delegate void DeathEvent();

    public event HealthDamagedEvent OnHealthDamaged;
    public event DeathEvent OnCharacterDeath;

    void Awake() {
        current_health = MaxHealth;
    }

    public void apply(int ammount) {
        if (Invincibility || is_invincible) {
            return;
        }
        current_health = Mathf.Clamp(current_health += ammount, 0, MaxHealth);
        if (OnCharacterDeath != null && current_health == 0)
        {
            OnCharacterDeath();
        }
        else if (OnHealthDamaged != null) {
            OnHealthDamaged(current_health, MaxHealth);
        }

    }
    public void reset() {
        current_health = MaxHealth;
    }
    public void instakill() {
        current_health = 0;
        OnCharacterDeath();
    }
    public void setInvincible(bool active) {
        is_invincible = active;
    }
}
