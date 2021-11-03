/* 
 * @file : PoolingManager.cs 
 * @date : 2021.04.21
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 모든 총알오브젝트 풀링
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]   //인스펙터창에서 관리
public struct PoolingInfo
{
    public GameObject prefab;   //생성해놓을 프리팹
    public int count;   //생성해놓을 갯수
}

public class PoolingManager : MonoBehaviour
{
    Dictionary<string, List<GameObject>> poolingObjects = new Dictionary<string, List<GameObject>>();
    public static PoolingManager instance;

    [SerializeField] private PoolingInfo[] poolingInfos;

    void Awake()
    {
        //싱글톤 및 씬이 변경되어도 해당 오브젝트가 파괴되지 않고 유지
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            if (instance != this)
            {
                Destroy(this.gameObject);
            }
        }
    }

    //OnEnble(), OnSceneLoaded(), OnDisable()는 DontDestroyOnLoad되어있는 오브젝트들이 씬이 바뀔때마다 호출하는 함수이다
    void OnEnable()
    {
        // 씬 매니저의 sceneLoaded에 체인을 건다.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 체인을 걸어서 이 함수는 매 씬마다 호출된다.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //특정 씬에서는 해당 오브젝트 파괴
        if (SceneManager.GetActiveScene().name == "GameClear" ||
            SceneManager.GetActiveScene().name == "MainMenu")
            Destroy(this.gameObject);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Initialize();   //오브젝트 생성
    }

    void Initialize()
    {
        for (int i = 0; i < poolingInfos.Length; i++)
        {
            List<GameObject> list = new List<GameObject>();

            for (int j = 0; j < poolingInfos[i].count; j++)
            {
                GameObject go = Instantiate(poolingInfos[i].prefab, transform);
                go.SetActive(false);

                list.Add(go);
            }

            string keyName = poolingInfos[i].prefab.name;   //Dictionary의 Key를 프리팹의 이름으로 설정
            poolingObjects.Add(keyName, list);  //Dictionary에 생성한 오브젝트리스트 추가
        }
    }

    public GameObject GetObject(string key)     //다른 스크립트에서 GetObject함수와 오브젝트를 
    {                                           //호출하면 생성해놓았던 오브젝트 한개를 리턴
        List<GameObject> list = new List<GameObject>();

        if (poolingObjects.ContainsKey(key) == true) //해당 키값이 있을때
        {
            list = poolingObjects[key];

            foreach (GameObject go in list)     //다른 스크립트에서 GetObject함수와 오브젝트를 호출하면 생성해놓았던 오브젝트 활성화
            {
                if (go.activeSelf == false)
                {
                    go.SetActive(true);
                    return go;
                }
            }

            /*만약 모든 오브젝트들이 활성화 되어있다면 
            오브젝트를 추가 생성하고 생성한 오브젝트 한개를 리턴*/
            for (int i = 0; i < 20; i++)   
            {                              
                GameObject go = Instantiate(list[0], transform);
                go.SetActive(false);
                list.Add(go);
            }
            list[list.Count - 1].SetActive(true);
            return list[list.Count - 1];
        }

        return null;
    }
}
