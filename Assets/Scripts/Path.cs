/* 
 * @file : Path.cs 
 * @date : 2021.05.13
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 방 사이를 이동하는 통로
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Path : MonoBehaviour
{
    public enum Kind
    {
        Normal,
        NextStage
    }
    public Kind kind = Kind.Normal;

    [SerializeField] public GameObject blockWall;   //뚫려있지 않은 통로를 막아줄 벽
    [SerializeField] public Transform movePoint;    //통로 이동시 다음방에 배치될 플레이어의 위치
    [SerializeField] public GameObject pathGuardians;   //방을 클리어하기 전까지 통로를 막아줄 오브젝트
    [SerializeField] public GameObject bossRoom_PathGuardians;  //건너편이 보스방이라면 다른 오브젝트를 배치
    [SerializeField] private float fadeTime;    //이동하는동안 페이드 될 시간

    [HideInInspector] public Path otherPath;    //반대편 통로
    private Animator fadeAnimator;  
    [HideInInspector] public GameObject player;
    private float playerWalkSpeed;

    private void Start()
    {
        fadeAnimator = GameObject.FindWithTag("FadeManager").GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player = collision.gameObject;
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            //플레이어의 이동속도를 별도로 저장해놓고 0으로 설정
            playerWalkSpeed = player.GetComponent<Player>().walkSpeed;
            player.GetComponent<Player>().walkSpeed = 0.0f;    //플레이어 움직임 잠시 멈춤
            fadeAnimator.SetTrigger("Fade_Out");
            //현재 방에서 플레이어가 다른 방으로 이동하므로 현재 방이 가지고 있는 플레이어가 들어왔는지 체크하는 변수를 false로 설정해준다.
            if(transform.parent.gameObject.layer == LayerMask.NameToLayer("Room"))
            {
                transform.parent.transform.Find("RoomManager").GetComponent<RoomManager>().isEnter = false;
                transform.parent.transform.Find("RoomManager").GetComponent<RoomManager>().isEnterSetting = false;
            }

            if (kind == Kind.Normal)    //보통 통로
                StartCoroutine(MoveOtherRoom());
            else if (kind == Kind.NextStage)    //다음 스테이지로 이동하는 통로
                StartCoroutine(MoveNextStage());
        }


        //통로가 이어져 있다면 막아놓았던 벽을 뚫은 후, 이어진 통로의 방이 보스방이라면 다른 통로를 막는 오브젝트를 배치한다.
        if (collision.gameObject.layer == LayerMask.NameToLayer("Path"))
        {
            otherPath = collision.gameObject.GetComponent<Path>();
            if(blockWall != null)
                blockWall.SetActive(false);

            if (collision.transform.parent.gameObject.layer == LayerMask.NameToLayer("BossRoom"))   //건너편이 보스방이라면
            {
                pathGuardians.SetActive(false);     //기존의 통로를 막는 오브젝트 꺼주기
                bossRoom_PathGuardians.SetActive(true);     //보스방으로 가는 통로를 막아주는 오브젝트 배치
            }
            else
            {
                pathGuardians.SetActive(true);
            }
        }
    }

    IEnumerator MoveOtherRoom()
    {
        yield return new WaitForSeconds(fadeTime);
        player.transform.position = otherPath.movePoint.position;   //다음방 이동
        player.GetComponent<Player>().walkSpeed = playerWalkSpeed;  //플레이어에게 빼앗은 속도 돌려주기
        if (otherPath.transform.parent.gameObject.layer == LayerMask.NameToLayer("Room"))
            //다음방으로 이동하므로 다음방이 가지고 있는 플레이어가 들어왔는지 체크하는 변수를 true로 설정
            otherPath.transform.parent.transform.Find("RoomManager").GetComponent<RoomManager>().isEnter = true;    
        else if (otherPath.transform.parent.gameObject.layer == LayerMask.NameToLayer("BossRoom"))
            //보스방 전용 플레이어가 들오왔는지 체크하는 변수를 true로 설정
            otherPath.transform.parent.transform.Find("BossRoomManager").GetComponent<BossRoomManager>().isEnter = true;
        fadeAnimator.SetTrigger("Fade_In");
    }

    IEnumerator MoveNextStage()
    { 
        yield return new WaitForSeconds(fadeTime); 
        player.GetComponent<Player>().walkSpeed = playerWalkSpeed;  //플레이어에게 빼앗은 속도 돌려주기
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);   //현재 빌드인덱스 다음 씬 불러오기
    }
}
