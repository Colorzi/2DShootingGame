/* 
 * @file : RoomManager.cs 
 * @date : 2021.05.10
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 방 안에 아이템, 적, 오브젝트 스폰 및 플레이어 입장시 적군 활성화
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    //아이템, 오브젝트, 적들 스폰할 지역
    [SerializeField] public float spawnAreaMinX;
    [SerializeField] public float spawnAreaMaxX;
    [SerializeField] public float spawnAreaMinY;
    [SerializeField] public float spawnAreaMaxY;
    [SerializeField] private GameObject[] enemies;  //적 종류
    [SerializeField] private GameObject[] objects;  //오브젝트 종류
    [SerializeField] private GameObject[] Items;    //아이템 종류
    [SerializeField] public int enemySpawnCount;    
    [SerializeField] public int objectSpawnCount;
    [SerializeField] public int ItemSpawnCount;
    [SerializeField] private List<GameObject> pathGuardian = new List<GameObject>();    //방을 클리어하기 전까지 통로를 막아줄 오브젝트
    [SerializeField] private float enemyWaitTime;   //플레이어가 방 입장후에 적들이 행동을 시작하기까지 걸리는 시간

    //스폰된 GameObject들이 담겨있는 List
    [HideInInspector] public List<GameObject> enemyList = new List<GameObject>();
    [HideInInspector] public List<GameObject> objectList = new List<GameObject>();
    [HideInInspector] public List<GameObject> ItemList = new List<GameObject>();
    private int rand;
    [HideInInspector] public int openingDirection;
    private float randPositionX;
    private float randPositionY;
    private GameObject spawnedObject;
    private GameObject miniMapCam;
    protected GameObject player;
    [HideInInspector] public bool isEnter = false;
    [HideInInspector] public bool isEnterSetting = false;
    [HideInInspector] public bool isActiveEnemy = false;
    [HideInInspector] public bool isClear = false;

    protected virtual void Start()
    {
        for (int i = 0; i < enemySpawnCount; i++)
            SpawnEnemy();

        for (int i = 0; i < objectSpawnCount; i++)
            SpawnObject();

        miniMapCam = GameObject.FindWithTag("MiniMapCamera");
    }

	private void Update()
	{
        //플레이어가 방에 입장해있는 경우
		if (isEnter)
		{
            if(!isEnterSetting)
            {
                isEnterSetting = true;
                miniMapCam.transform.position = transform.parent.position + new Vector3(0, 0, -10);   //미니맵 카메라 세팅
            }

            if(!isActiveEnemy)  //적 활성화는 방 하나당 한번만 실행
			{
                isActiveEnemy = true;
                StartCoroutine(ActiveEnemy());
			}
        }

		if (enemyList.Count == 0)   //적 전부 처치 시
        {
            if(!isClear)    //pathGuardian 파괴는 플레이어가 방을 클리어했을때 한번만 실행
            {
                isClear = true;

                for (int i = 0; i < pathGuardian.Count; i++)
                {
                    for (int j = 0; j < pathGuardian[i].transform.childCount; j++)
                    {
                        pathGuardian[i].transform.GetChild(j).GetComponent<Animator>().SetTrigger("Destroy");
                    }
                }
            }
        }
    }

    public void SpawnEnemy()
    {
        rand = Random.Range(0, enemies.Length);
        randPositionX = Random.Range(spawnAreaMinX, spawnAreaMaxX);
        randPositionY = Random.Range(spawnAreaMinY, spawnAreaMaxY);
        spawnedObject = Instantiate(enemies[rand], transform.position + new Vector3(randPositionX, randPositionY), Quaternion.identity, transform);
        enemyList.Add(spawnedObject);
    }

    public void SpawnObject()
    {
        rand = Random.Range(0, objects.Length);
        randPositionX = Random.Range(spawnAreaMinX, spawnAreaMaxX);
        randPositionY = Random.Range(spawnAreaMinY, spawnAreaMaxY);
        spawnedObject = Instantiate(objects[rand], transform.position + new Vector3(randPositionX, randPositionY), Quaternion.identity, transform);
        objectList.Add(spawnedObject);
    }

    public void SpawnItem()
    {
        rand = Random.Range(0, Items.Length);
        randPositionX = Random.Range(spawnAreaMinX, spawnAreaMaxX);
        randPositionY = Random.Range(spawnAreaMinY, spawnAreaMaxY);
        spawnedObject = Instantiate(Items[rand], transform.position + new Vector3(randPositionX, randPositionY), Quaternion.identity, transform);
        ItemList.Add(spawnedObject);
    }

    public IEnumerator ActiveEnemy()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].SetActive(true);
            enemyList[i].GetComponent<Enemy>().hpBarObject.SetActive(true);
        }

        //플레이어가 방에 입장한 후에 일정시간후에 적들이 활동하도록 딜레이를 줌
        yield return new WaitForSeconds(enemyWaitTime);     

        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].GetComponent<Enemy>().isActivity = true;
        }
    }

    public void Emenies_DestroyPositioning()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            Destroy(enemyList[i].GetComponent<Positioning>());
            enemyList[i].GetComponent<Enemy>().hpBarObject.SetActive(false);
            enemyList[i].SetActive(false);
        }
    }

    public void Objects_DestroyPositioning()
    {
        for (int i = 0; i < objectList.Count; i++)
        {
            Destroy(objectList[i].GetComponent<Positioning>());
        }
    }

    public void Items_DestroyPositioning()
    {
        for (int i = 0; i < ItemList.Count; i++)
        {
            Destroy(ItemList[i].GetComponent<Positioning>());
        }
    }
}