using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Health : MonoBehaviour {
    public int MaxHealth = 100;
    public bool Invincibility = false;

    public int current_health;
    bool is_invincible = false;

    public delegate void HealthDamagedEvent(int hp, int max);
    public delegate void DeathEvent();

    public event HealthDamagedEvent OnHealthDamaged;
    public event DeathEvent OnCharacterDeath;

    public Image HealthBar;

    public bool IsInvincible {
        get { return Invincibility || is_invincible; }
    }

    void Awake() {
        current_health = MaxHealth;
    }
    public bool isAlive() {
        return current_health > 0;
    }
    public bool isMaxHealth() {
        return current_health == MaxHealth;
    }

    public bool apply(int ammount) {
        if (IsInvincible) {
            return false;
        }
        current_health = Mathf.Clamp(current_health += ammount, 0, MaxHealth);
        if (OnCharacterDeath != null && current_health == 0)
        {
            OnCharacterDeath();
            return true;
        }
        else if (OnHealthDamaged != null) {
            OnHealthDamaged(current_health, MaxHealth);
        }
        return false;

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
    public void healthBar()
    {
        HealthBar.fillAmount = (float)current_health / (float)MaxHealth;
    }
}
