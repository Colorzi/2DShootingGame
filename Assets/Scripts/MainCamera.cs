/* 
 * @file : MainCamera.cs 
 * @date : 2021.04.18
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 플레이어를 따라다니는 기능, 아이템 획득시 사운드 재생
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField] private float followSpeed;
    [SerializeField] private AudioSource getWeapon_Audio;
    [SerializeField] private AudioSource getHp_Audio;

    [HideInInspector] public GameObject player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    private void FixedUpdate()
    {
        //플레이어 따라다니는 기능
        if(player != null)
            transform.position = Vector3.Lerp(transform.position, new Vector3(player.transform.position.x, player.transform.position.y, -10), followSpeed);
    }   

    //플레이어가 무기아이템 획득 시 재생
    public void Play_GetWeaponAudio()
    {
        getWeapon_Audio.Play();
    }

    //플레이어가 회복아이템 획득 시 재생
    public void Play_GetHpAudio()
    {
        getHp_Audio.Play();
    }
}