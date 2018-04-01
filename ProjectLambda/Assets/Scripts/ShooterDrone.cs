﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterDrone : MonoBehaviour, IAttackable {
    Queen queen;
    public void attack(int dmg, Vector2 dir, float pow, float stun_time = 0)
    {
        if (pow >= 18f || dmg > 0) {
            queen.destroyChild(transform);
        }
    }
    public bool isInvincible()
    {
        return false;
    }
    public bool isStunned()
    {
        return false;
    }
    void Start() {
        queen = transform.parent.transform.parent.GetComponent<Queen>();
    }
}