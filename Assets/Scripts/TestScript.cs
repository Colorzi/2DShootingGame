using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            Debug.Log("!");
        }
    }
}
