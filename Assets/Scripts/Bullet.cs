/* 
 * @file : Bullet.cs 
 * @date : 2021.04.18
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 총알의 이동 및 충돌처리
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] public float damage;
    [SerializeField] private float speed;
    [SerializeField] private GameObject miniMapIcon;
    
    [HideInInspector] public Vector2 direction;

    private Rigidbody2D myRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        miniMapIcon.SetActive(true);
    }

    void FixedUpdate()
    {
        myRigidbody.velocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        gameObject.SetActive(false);
    }
}
