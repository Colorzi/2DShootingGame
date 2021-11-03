/* 
 * @file : BossRoomManager.cs 
 * @date : 2021.05.16
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 보스 스폰, 플레이어가 보스방 입장시 연출실행 후 보스 활성화
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomManager : MonoBehaviour
{
    private enum BossType
    {
        Stage1,
        Stage2,
        Stage3
    }
    [SerializeField] private BossType bossType;
    [SerializeField] private GameObject boss;
    [SerializeField] private List<GameObject> pathGuardian = new List<GameObject>();   //방을 클리어하기 전까지 통로를 막아줄 오브젝트
    [SerializeField] private AudioClip bossBGM;    
    [SerializeField] private AudioClip normalBGM;
    [SerializeField] private float zoomBossTime;    //플레이어가 방 입장시 보스를 비춰주는 시간

    private Camera mainCamera;
    private Player player;
    private GameObject bossObject;
    private GameObject miniMapCam;
    [HideInInspector] public bool isEnter = false; 
    private bool bossBGM_Play = false;
    [HideInInspector] public bool isClear = false;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        mainCamera = Camera.main;
        miniMapCam = GameObject.FindWithTag("MiniMapCamera");
        bossObject = Instantiate(boss, transform.position, Quaternion.identity, transform);
        bossObject.SetActive(false);
    }

    void Update()
    {
        //보스방 입장시 플레이어의 움직임이 정지되고 보스몹에게 카메라가 줌되는 연출 실행
        if (isEnter)
        {
            isEnter = false;
            bossObject.SetActive(true);
            player.state = Player.State.Stop;
            miniMapCam.transform.position = transform.parent.position + new Vector3(0, 0, -10);   //미니맵 카메라 세팅
            mainCamera.GetComponent<MainCamera>().enabled = false;
            mainCamera.gameObject.transform.position = bossObject.transform.position + new Vector3(0.0f, 0.0f, -10.0f);
            //보스전BGM실행
            if (!bossBGM_Play)
            {
                bossBGM_Play = true;
                mainCamera.GetComponent<AudioSource>().clip = bossBGM;
                mainCamera.GetComponent<AudioSource>().Play();
            }

            //연출 종료후 플레이어의 움직임을 해제하고 카메라 원위치, 보스몹 활동시작
            StartCoroutine(StartBossStage());
        }

        //보스방 클리어 시
        if(isClear)
        {
            isClear = false;

            //보통 BGM으로 변경
            mainCamera.GetComponent<AudioSource>().clip = normalBGM;
            mainCamera.GetComponent<AudioSource>().Play(); 
            //통로를 막는 오브젝트 파괴
            for (int i = 0; i < pathGuardian.Count; i++)
            {
                for (int j = 0; j < pathGuardian[i].transform.childCount; j++)
                {
                    pathGuardian[i].transform.GetChild(j).GetComponent<Animator>().SetTrigger("Destroy");
                }
            }
        }
    }

    IEnumerator StartBossStage()
    {
        yield return new WaitForSeconds(zoomBossTime);
        mainCamera.GetComponent<MainCamera>().enabled = true;
        player.state = Player.State.Idle;
        switch (bossType)
        {
            case BossType.Stage1:
                bossObject.GetComponent<Stage1_Boss>().isActivity = true;
                break;
            case BossType.Stage2:
                bossObject.GetComponent<Stage2_Boss>().isActivity = true;
                break;
            case BossType.Stage3:
                bossObject.GetComponent<Stage3_Boss>().isActivity = true;
                break;
        }
    }
}
