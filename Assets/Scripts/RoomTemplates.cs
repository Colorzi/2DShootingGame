/* 
 * @file : RoomTemplates.cs 
 * @date : 2021.05.03
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 모든 방과 스테이지를 구성
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoomTemplates : MonoBehaviour
{
    [SerializeField] public GameObject[] bottomRooms;
    [SerializeField] public GameObject[] topRooms;
    [SerializeField] public GameObject[] leftRooms;
    [SerializeField] public GameObject[] rightRooms;
    [SerializeField] public GameObject bL_Room;
    [SerializeField] public GameObject bR_Room;
    [SerializeField] public GameObject tL_Room;
    [SerializeField] public GameObject tR_Room;
    [SerializeField] public GameObject lR_Room;
    [SerializeField] public GameObject tB_Room;
    [SerializeField] public GameObject LR_bossRoom;
    [SerializeField] public GameObject TB_bossRoom;
    [SerializeField] private NavMeshSurface2d navMeshSurface2d;
    [SerializeField] public List<GameObject> rooms = new List<GameObject>();    //스폰된 방들이 담겨질 List
    [SerializeField] public float bossRoomSpawnTime;     //보스방이 스폰되는시간
    public float destroyPositioningTime;    //Positioning스크립트를 제거할 타이밍

    private Player player;
    public static RoomTemplates instance;
    private RoomManager roomManager;
    private RoomManager startingRoomManager;    
    private RoomManager lastRoomManager;
    private List<int> roomNumbersForSpawnItem = new List<int>();    //아이템을 스폰할 방 지정
    private Animator fadeAnimator;
    private GameObject bossRoomObject;
    private GameObject tile;
    private GameObject lastRoom;
    private Vector2 lastRoomPosition;
    private int rand;
    private int spawn_ItemCount;
    [HideInInspector] public bool spawnedBoss = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.transform.position = Vector3.zero;
        tile = GameObject.FindGameObjectWithTag("Tile");
        startingRoomManager = GameObject.FindGameObjectWithTag("LRBT_Room").transform.Find("RoomManager").GetComponent<RoomManager>();
        fadeAnimator = GameObject.FindWithTag("FadeManager").GetComponent<Animator>();
        rooms.Insert(0, GameObject.FindGameObjectWithTag("LRBT_Room"));     //첫 시작방
        player.state = Player.State.Stop;   //스테이지 구성이 완료되기 전까지 player의 움직임을 정지시킨다
    }

    private void Update()
    {
        if(!spawnedBoss)
        {
            bossRoomSpawnTime -= Time.deltaTime;

            if (bossRoomSpawnTime <= 0)
			{
                spawnedBoss = true;
                lastRoom = rooms[rooms.Count - 1];
                lastRoomPosition = lastRoom.transform.position;
                lastRoomManager = lastRoom.transform.Find("RoomManager").GetComponent<RoomManager>();
                for (int i = 0; i < lastRoomManager.enemyList.Count; i++)
                    Destroy(lastRoomManager.enemyList[i].GetComponent<Enemy>().hpBarObject);
                Destroy(lastRoom);    //마지막 방 제거
                rooms.RemoveAt(rooms.Count - 1);    //마지막 방 리스트에서 제거

                /*마지막 방의 openingDirection(반드시 존재해야할 통로 방향)에 따라서 보스방을 스폰해준다.
                ex) 마지막 방의 openingDirection이 1(아랫쪽통로가 필요한 방)이라면 아랫쪽통로와 반대쪽 방향인 욋쪽통로를 가지고있는
                TB_bossRoom을 스폰하고 반대쪽 방향인 윗쪽통로를 다음스테이지로 이동하는 통로로 바꾼다.*/
                if (lastRoom.GetComponentInChildren<RoomManager>().openingDirection == 1)
                {
                    bossRoomObject = Instantiate(TB_bossRoom, lastRoomPosition, Quaternion.identity, tile.transform);
                    bossRoomObject.transform.Find("TopPath").GetComponent<Path>().kind = Path.Kind.NextStage;
                }
                else if (lastRoom.GetComponentInChildren<RoomManager>().openingDirection == 2)
                {
                    bossRoomObject = Instantiate(TB_bossRoom, lastRoomPosition, Quaternion.identity, tile.transform);
                    bossRoomObject.transform.Find("BottomPath").GetComponent<Path>().kind = Path.Kind.NextStage;
                }
                else if (lastRoom.GetComponentInChildren<RoomManager>().openingDirection == 3)
                {
                    bossRoomObject = Instantiate(LR_bossRoom, lastRoomPosition, Quaternion.identity, tile.transform);
                    bossRoomObject.transform.Find("RightPath").GetComponent<Path>().kind = Path.Kind.NextStage;
                }
                else if (lastRoom.GetComponentInChildren<RoomManager>().openingDirection == 4)
                {
                    bossRoomObject = Instantiate(LR_bossRoom, lastRoomPosition, Quaternion.identity, tile.transform);
                    bossRoomObject.transform.Find("LeftPath").GetComponent<Path>().kind = Path.Kind.NextStage;
                }

                Spawne_Items();
                //스테이지를 전부 구성하면 모든 Positioning스크립트를 제거하고 네비메쉬를 Bake, 플레이어의 움직임을 가능하게한다.
                StartCoroutine(DestroyPositioning());
            }
		}
    }

    void Spawne_Items()
    {
        //방갯수에 따라 생성할 아이템 갯수
        if (rooms.Count >= 20)
        {
            spawn_ItemCount = 9;
        }
        else if (rooms.Count >= 15)
        {
            spawn_ItemCount = 7;
        }
        else if (rooms.Count >= 10)
        {
            spawn_ItemCount = 4;
        }
        else if (rooms.Count >= 5)
        {
            spawn_ItemCount = 2;
        }

        //중복되지 않는 난수 생성
        rand = Random.Range(0, rooms.Count - 1);
        for (int i = 0; i < spawn_ItemCount;)
        {
            if (roomNumbersForSpawnItem.Contains(rand))
            {
                rand = Random.Range(0, rooms.Count - 1);
            }
            else
            {
                roomNumbersForSpawnItem.Add(rand);
                i++;
            }
        }

        //생성된 난수가 가리키는 방에 아이템 생성
        for (int i = 0; i < spawn_ItemCount; i++)
        {
            roomManager = rooms[roomNumbersForSpawnItem[i]].transform.Find("RoomManager").GetComponent<RoomManager>();

            for (int j = 0; j < roomManager.ItemSpawnCount; j++)
            {
                roomManager.SpawnItem();
            }
        }
    }

    IEnumerator DestroyPositioning()
    {
        yield return new WaitForSeconds(destroyPositioningTime);

        for (int i = 0; i < rooms.Count; i++)
        {
            roomManager = rooms[i].transform.Find("RoomManager").GetComponent<RoomManager>();
            roomManager.Emenies_DestroyPositioning();
            roomManager.Objects_DestroyPositioning();
            roomManager.Items_DestroyPositioning();
        }

        navMeshSurface2d.BuildNavMesh();
        fadeAnimator.SetTrigger("Fade_In");
        startingRoomManager.isEnter = true;
        player.state = Player.State.Idle;
    }
}
