/* 
 * @file : Pistol.cs 
 * @date : 2021.04.21
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 기본무기의 발사기능
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Weapon
{
    [SerializeField] private float fireDelay;   //발사 텀

    public override void Fire()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (isFire == false)
            {
                isFire = true;
                shotAudio.Play();
                poolingBullet = PoolingManager.instance.GetObject("Player_Bullet(Pistol)");     //풀링에서 총알오브젝트 불러오기
                poolingBullet.transform.position = muzzle.transform.position;   //총구위치에 총알배치
                poolingBullet.GetComponent<Bullet>().direction = muzzleToMouse_Direction;   //사격방향을 마우스방향으로 설정
                poolingBullet.transform.up = muzzleToMouse_Direction;   //총알을 마우스방향으로 회전시켜주기
                currentBulletCount--;    //총알 한개소비

                //탄창(남은 총알의 갯수 / 모든 총알의 갯수)UI 갱신
                UIManager.instance.MagazineUI_Update(currentBulletCount, bulletCount);

                if (currentBulletCount > 0)   //총알이 남아있을 경우에만 실행
                    StartCoroutine(FireDelay());
                else
                    isFire = false;
            }
        }
    }

    IEnumerator FireDelay()
    {
        yield return new WaitForSeconds(fireDelay);
        isFire = false;
    }
}