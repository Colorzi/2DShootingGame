/* 
 * @file : Item_HP.cs 
 * @date : 2021.05.07
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 회복아이템
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_HP : MonoBehaviour
{
    [SerializeField] public float healAmount;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Camera.main.GetComponent<MainCamera>().Play_GetHpAudio();
            collision.GetComponent<Player>().hp += healAmount;
            Destroy(gameObject);
        }
    }
}
