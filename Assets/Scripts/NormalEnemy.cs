/* 
 * @file : NormalEnemy.cs 
 * @date : 2021.04.26
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 기본타입의 적 공격패턴
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEnemy : Enemy
{
    [SerializeField] private int fireBulletCount;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackCoolTime;

    protected override void Attack()
    {
        if(!isFire)
        {
            isFire = true;
            StartCoroutine(AttackPattern());
        }
    }

    //플레이어에게 fireBulletCount의 수만큼 주기적으로 발사
    IEnumerator AttackPattern()
    {
        for (int i = 0; i < fireBulletCount; i++)
        {
            shotAudio.Play();
            poolingBullet = PoolingManager.instance.GetObject("Enemy_Bullet(Normal)");
            poolingBullet.transform.position = muzzle.transform.position;
            poolingBullet.GetComponent<Bullet>().direction = muzzleToPlayer_Direction;
            poolingBullet.transform.up = muzzleToPlayer_Direction;

            yield return new WaitForSeconds(attackDelay);
        }

        yield return new WaitForSeconds(attackCoolTime);
        isFire = false;
    }
}