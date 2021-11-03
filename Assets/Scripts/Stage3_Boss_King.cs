/* 
 * @file : Stage3_Boss_King.cs 
 * @date : 2021.06.06
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 스테이지3 보스 본체의 애니메이션 이벤트
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage3_Boss_King : MonoBehaviour
{
    [SerializeField] private Stage3_Boss stage3_Boss;
    [SerializeField] private Animator throneAnimator;

    private Animator animator;
   
    void Start()
    {
        animator = GetComponent<Animator>();    
    }

    void Pattern1()
    {
        stage3_Boss.Pattern1();
    }

    void Pattern2()
    {
        stage3_Boss.Pattern2();
    }

    void Pattern3()
    {
        stage3_Boss.Pattern3();
    }

    void Pattern4()
    {
        stage3_Boss.Pattern4();
    }

    void NextPattern()
    {
        stage3_Boss.NextPattern();
    }

    void IdleTrigger()
    {
        animator.SetTrigger("Idle");
    }

    void Destory()
    {
        Destroy(stage3_Boss.gameObject);
    }
}