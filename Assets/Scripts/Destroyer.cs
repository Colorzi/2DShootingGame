/* 
 * @file : Destroyer.cs 
 * @date : 2021.05.12
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 통로를 막는 오브젝트의 애니메이션 이벤트
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    public void Destroy()
    {
        Destroy(gameObject);
    }
}