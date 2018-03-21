using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**IAttackable : interface for allowing one character to attack another, regardless of whether it inherits from custom physics object of monobehaviour
 *  - attack : the character has attacked and hit the implementing object
 *      - dmg : the damage to apply
 *      - dir : the direction the hit came from
 *      - pow : the strength of the hit
 *      - stun_time : variable ammount of stun to apply
 * **/
public interface IAttackable {
    bool isStunned();
    bool isInvincible();
    void attack(int dmg, Vector2 dir, float pow, float stun_time = 0f);
}
