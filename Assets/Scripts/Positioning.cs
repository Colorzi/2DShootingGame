/* 
 * @file : Positioning.cs 
 * @date : 2021.05.19
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 방에 오브젝트, 적, 아이템 스폰시 서로 위치가 겹쳐질 경우 다시 배치
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Positioning : MonoBehaviour
{
    private RoomManager roomManager;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Object") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            roomManager = transform.parent.GetComponent<RoomManager>();
            transform.localPosition = new Vector2(Random.Range(roomManager.spawnAreaMinX, roomManager.spawnAreaMaxX),
                   Random.Range(roomManager.spawnAreaMinY, roomManager.spawnAreaMaxY));

            RoomTemplates.instance.destroyPositioningTime += 0.01f;
        }
    }
}