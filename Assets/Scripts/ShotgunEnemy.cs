/* 
 * @file : ShotgunEnemy.cs 
 * @date : 2021.04.28
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 샷건타입의 적 공격패턴
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunEnemy : Enemy
{
    [SerializeField] private int onceFireBulletCount;   //발사수
    [SerializeField] private float angleBetweenBullet;  //총알 사이 각도
    [SerializeField] private float attackCoolTime;

    private Vector2 bulletDirection;

    protected override void Attack()
    {
        if (!isFire)
        {
            isFire = true;
            StartCoroutine(AttackPattern());
        }
    }

    IEnumerator AttackPattern()
    {
        shotAudio.Play();
        for (int i = 0; i < onceFireBulletCount; i++)
        {
            poolingBullet = PoolingManager.instance.GetObject("Enemy_Bullet(Shotgun)");
            poolingBullet.transform.position = muzzle.transform.position;
            bulletDirection = Quaternion.Euler(new Vector3(0.0f, 0.0f, -((angleBetweenBullet * (onceFireBulletCount - 1)) / 2.0f)
                        + (angleBetweenBullet * i))) * muzzleToPlayer_Direction;    //각도조정
            poolingBullet.GetComponent<Bullet>().direction = bulletDirection;   //나아갈 방향
            poolingBullet.transform.up = bulletDirection;   //총알의 roation 조정
        }

        yield return new WaitForSeconds(attackCoolTime);
        isFire = false;
    }
}
