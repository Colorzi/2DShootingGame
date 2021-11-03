/* 
 * @file : AfterImage.cs 
 * @date : 2021.05.18
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 플레이어의 대쉬 기능 조작시 잔상효과 출력을 위한 텍스쳐
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    [SerializeField] public ParticleSystemRenderer afterImage;
    [SerializeField] public Texture2D Idle_BackDash;
    [SerializeField] public Texture2D Idle_FrontDash;
    [SerializeField] public Texture2D Idle_UpDash;
    [SerializeField] public Texture2D Idle_DownDash;
    [SerializeField] public Texture2D LookAtUp_LeftDash;
    [SerializeField] public Texture2D LookAtUp_RightDash;
    [SerializeField] public Texture2D LookAtUp_UpDash;
    [SerializeField] public Texture2D LookAtUp_DownDash;

    //-----------------  애니메이션 이벤트 -------------------
    public void Idle_BackDash_AfterImage()
    {
        afterImage.material.mainTexture = Idle_BackDash;
    }

    public void Idle_FrontDash_AfterImage()
    {
        afterImage.material.mainTexture = Idle_FrontDash;
    }

    public void Idle_UpDash_AfterImage()
    {
        afterImage.material.mainTexture = Idle_UpDash;
    }

    public void Idle_DownDash_AfterImage()
    {
        afterImage.material.mainTexture = Idle_DownDash;
    }

    public void LookAtUp_LeftDash_AfterImage()
    {
        afterImage.material.mainTexture = LookAtUp_LeftDash;
    }

    public void LookAtUp_RightDash_AfterImage()
    {
        afterImage.material.mainTexture = LookAtUp_RightDash;
    }

    public void LookAtUp_UpDash_AfterImage()
    {
        afterImage.material.mainTexture = LookAtUp_UpDash;
    }

    public void LookAtUp_DownDash_AfterImage()
    {
        afterImage.material.mainTexture = LookAtUp_DownDash;
    }
    //-----------------  애니메이션 이벤트 -------------------
}
