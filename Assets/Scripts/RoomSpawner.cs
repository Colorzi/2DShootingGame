/* 
 * @file : RoomSpawner.cs 
 * @date : 2021.05.01
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 방을 생성하는 오브젝트
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{ 
    [SerializeField] public int openingDirection;    
    /*1 --> 아랫쪽 통로가 필요한 방
      2 --> 위쪽 통로가 필요한 방 
      3 --> 왼쪽 통로가 필요한 방
      4 --> 오른쪽 통로가 필요한 방*/

    private GameObject tile;
    private GameObject roomObject;
    private int rand;
    public bool spawned;

    void Start()
    {
        tile = GameObject.FindGameObjectWithTag("Tile");
        //처음시작하는 네 방향통로가 존재하는 방에는 생성하지않음
        if (transform.localPosition == Vector3.zero)
            spawned = true;
        Invoke("Spawn", 0.1f);  //0.1초후에 방 생성
        RoomTemplates.instance.bossRoomSpawnTime += 0.1f;    //방이 생성되면 보스방을 생성할 시간을 늘린다.
    }

    private void Update()
    {
        if (RoomTemplates.instance.bossRoomSpawnTime <= 0.0f)
            Destroy(gameObject);
    }

    /*openingDirection(반드시 존재해야할 통로)에 따라서 방 하나를 생성함
      ex) openingDirection이 1이라면 아랫쪽 통로가 필요한 방을 스폰해야되므로 
      아래, 위아래, 왼쪽아래, 오른쪽아래 통로를 가진 방중에 랜덤하게 하나 생성*/
    void Spawn()
    {
        if(!spawned)
        {
            spawned = true;
            if (openingDirection == 1)
            {
                rand = Random.Range(0, RoomTemplates.instance.bottomRooms.Length);
                roomObject = Instantiate(RoomTemplates.instance.bottomRooms[rand], transform.position, Quaternion.identity, tile.transform);
            }
            else if (openingDirection == 2)
            {
                rand = Random.Range(0, RoomTemplates.instance.topRooms.Length);
                roomObject = Instantiate(RoomTemplates.instance.topRooms[rand], transform.position, Quaternion.identity, tile.transform);
            }
            else if (openingDirection == 3)
            {
                rand = Random.Range(0, RoomTemplates.instance.leftRooms.Length);
                roomObject = Instantiate(RoomTemplates.instance.leftRooms[rand], transform.position, Quaternion.identity, tile.transform);
            }
            else if (openingDirection == 4)
            {
                rand = Random.Range(0, RoomTemplates.instance.rightRooms.Length);
                roomObject = Instantiate(RoomTemplates.instance.rightRooms[rand], transform.position, Quaternion.identity, tile.transform);
            }

            RoomTemplates.instance.rooms.Add(roomObject);     //RoomTemplates의 방리스트에 추가
            roomObject.GetComponentInChildren<RoomManager>().openingDirection = openingDirection;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        spawned = true;

        /*만약 RoomSpawner끼리 서로 겹쳤을 경우, 겹쳐진 RoomSpawner끼리 필요한 openingDirection(반드시 존재해야 할 통로)에 따라 
          방을 생성함. ex)겹쳐진 두개의 RoomSpanwer의 openingDirection이 1(아랫쪽통로가 존재하는 방을 생성)과 2(위쪽통로가 존재하는 방을 생성)이라면
          위쪽과 아랫쪽 통로를 가진 방을 생성함*/
        if (other.CompareTag("SpawnPoint"))
        {
            if(!other.GetComponent<RoomSpawner>().spawned && !spawned)
            {
                if (openingDirection == 1 && other.GetComponent<RoomSpawner>().openingDirection == 2)
                {
                    roomObject = Instantiate(RoomTemplates.instance.tB_Room, transform.position, Quaternion.identity, tile.transform);
                }
                else if (openingDirection == 1 && other.GetComponent<RoomSpawner>().openingDirection == 3)
                {
                    roomObject = Instantiate(RoomTemplates.instance.bL_Room, transform.position, Quaternion.identity, tile.transform);
                }
                else if (openingDirection == 1 && other.GetComponent<RoomSpawner>().openingDirection == 4)
                {
                    roomObject = Instantiate(RoomTemplates.instance.bR_Room, transform.position, Quaternion.identity, tile.transform);
                }
                else if (openingDirection == 2 && other.GetComponent<RoomSpawner>().openingDirection == 1)
                {
                    roomObject = Instantiate(RoomTemplates.instance.tB_Room, transform.position, Quaternion.identity, tile.transform);
                }
                else if (openingDirection == 2 && other.GetComponent<RoomSpawner>().openingDirection == 3)
                {
                    roomObject = Instantiate(RoomTemplates.instance.tL_Room, transform.position, Quaternion.identity, tile.transform);
                }
                else if (openingDirection == 2 && other.GetComponent<RoomSpawner>().openingDirection == 4)
                {
                    roomObject = Instantiate(RoomTemplates.instance.tR_Room, transform.position, Quaternion.identity, tile.transform);
                }
                else if (openingDirection == 3 && other.GetComponent<RoomSpawner>().openingDirection == 1)
                {
                    roomObject = Instantiate(RoomTemplates.instance.bL_Room, transform.position, Quaternion.identity, tile.transform);
                }
                else if (openingDirection == 3 && other.GetComponent<RoomSpawner>().openingDirection == 2)
                {
                    roomObject = Instantiate(RoomTemplates.instance.tL_Room, transform.position, Quaternion.identity, tile.transform);
                }
                else if (openingDirection == 3 && other.GetComponent<RoomSpawner>().openingDirection == 4)
                {
                    roomObject = Instantiate(RoomTemplates.instance.lR_Room, transform.position, Quaternion.identity, tile.transform);
                }
                else if (openingDirection == 4 && other.GetComponent<RoomSpawner>().openingDirection == 1)
                {
                    roomObject = Instantiate(RoomTemplates.instance.bR_Room, transform.position, Quaternion.identity, tile.transform);
                }
                else if (openingDirection == 4 && other.GetComponent<RoomSpawner>().openingDirection == 2)
                {
                    roomObject = Instantiate(RoomTemplates.instance.tR_Room, transform.position, Quaternion.identity, tile.transform);
                }
                else if (openingDirection == 4 && other.GetComponent<RoomSpawner>().openingDirection == 3)
                {
                    roomObject = Instantiate(RoomTemplates.instance.lR_Room, transform.position, Quaternion.identity, tile.transform);
                }

                RoomTemplates.instance.rooms.Add(roomObject);                
                roomObject.transform.Find("RoomManager").GetComponent<RoomManager>().openingDirection = openingDirection;
                Destroy(gameObject);
            }
        }
    }
}