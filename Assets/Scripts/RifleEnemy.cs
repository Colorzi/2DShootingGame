/* 
 * @file : RifleEnemy.cs 
 * @date : 2021.04.27
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 소총타입의 적 공격패턴
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleEnemy : Enemy
{
    [SerializeField] private float attackDelay;

    protected override void Attack()
    {
        if(!isFire)
        {
            isFire = true;
            StartCoroutine(AttackPattern());
        }
    }

    IEnumerator AttackPattern()
    {
        shotAudio.Play();
        poolingBullet = PoolingManager.instance.GetObject("Enemy_Bullet(Rifle)");
        poolingBullet.transform.position = muzzle.transform.position;
        poolingBullet.GetComponent<Bullet>().direction = muzzleToPlayer_Direction;
        poolingBullet.transform.up = muzzleToPlayer_Direction;

        yield return new WaitForSeconds(attackDelay);
        isFire = false;
    }
}