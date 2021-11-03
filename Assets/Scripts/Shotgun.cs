/* 
 * @file : Shotgun.cs 
 * @date : 2021.04.22
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 샷건무기의 발사기능
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Weapon
{
    [SerializeField] private int fireBulletCount;   //발사수
    [SerializeField] private float angleBetweenBullet;  //총알 사이 각도
    [SerializeField] private float fire_FirstDelay;    //발사후 첫번째 딜레이
    [SerializeField] private float fire_SecondDelay;   //두번째 딜레이

    private Vector2 bulletDirection;

    public override void Fire()
    {
        if (Input.GetMouseButton(0))
        {
            if (isFire == false)
            {
                isFire = true;
                shotAudio.Play();
                for (int i = 0; i < fireBulletCount; i++)
                {
                    poolingBullet = PoolingManager.instance.GetObject("Player_Bullet(Shotgun)");    //총알오브젝트 불러오기
                    poolingBullet.transform.position = muzzle.transform.position;   //총구위치에 총알배치
                    bulletDirection = Quaternion.Euler(new Vector3(0.0f, 0.0f, -((angleBetweenBullet * (fireBulletCount - 1)) / 2.0f)
                        + (angleBetweenBullet * i))) * muzzleToMouse_Direction;    //각도 조정,  -((angleBetweenBullet * (fireBulletCount - 1)) / 2.0f)는 맨 위에 위치한 총알의 각도
                    poolingBullet.GetComponent<Bullet>().direction = bulletDirection;   //나아갈 방향
                    poolingBullet.transform.up = bulletDirection;   //총알의 roation 조정
                }
                currentBulletCount -= fireBulletCount;   //총알 소비
                //탄창(남은 총알의 갯수 / 모든 총알의 갯수)UI 갱신
                UIManager.instance.MagazineUI_Update(currentBulletCount, bulletCount);

                //한번 발사하면 일정시간동안은 발사불가
                if (currentBulletCount > 0)   //총알이 남아있을 경우에만 통상 딜레이 적용
                    StartCoroutine(FireDelay());
                else     //그 외에는 장전
                    isFire = false;
            }
        }
    }

    IEnumerator FireDelay()
    {
        yield return new WaitForSeconds(fire_FirstDelay);  //발사 후 장전시작하기까지 걸리는 시간
        reloadAudio.Play(); //장전소리 재생
        yield return new WaitForSeconds(fire_SecondDelay); //장전에 걸리는 시간
        isFire = false;     //발사가능
    }
}
