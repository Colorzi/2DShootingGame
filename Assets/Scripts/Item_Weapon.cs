/* 
 * @file : Item_Weapon.cs 
 * @date : 2021.05.07
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 무기아이템
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Weapon : MonoBehaviour
{
    [SerializeField] public GameObject weapon;

    private GameObject weaponObject;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Destroy(collision.GetComponent<Player>().weapon.gameObject);    //현재 플레이어가 사용하는 무기 제거
            weaponObject = Instantiate(weapon, collision.transform);    //무기 오브젝트 생성
            collision.GetComponent<Player>().weapon = weaponObject.GetComponent<Weapon>();  //생성한 무기 오브젝트를 플레이어에게 소지시킴
            Camera.main.GetComponent<MainCamera>().Play_GetWeaponAudio();
            Destroy(gameObject);
        }
    }
}