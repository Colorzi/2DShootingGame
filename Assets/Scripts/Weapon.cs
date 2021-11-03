/* 
 * @file : Weapon.cs 
 * @date : 2021.04.21
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 플레이어 무기의 출력, 장전기능
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] public SpriteRenderer Sprite;
    [SerializeField] public GameObject muzzle;  //총구
    [SerializeField] protected AudioSource shotAudio;
    [SerializeField] protected AudioSource reloadAudio;
    [SerializeField] protected float localPositionX;
    [SerializeField] protected float localPositionY;
    [SerializeField] protected int bulletCount;
    [SerializeField] public float reloadTime;

    protected Player player;
    protected GameObject poolingBullet;   //가져올 총알
    private Vector2 mouseVec;
    protected Vector2 muzzleToMouse_Direction;
    [HideInInspector] public int currentBulletCount;
    protected int player_OrderInLayer;  //플레이어의 레이어순서
    private float weaponToMouse_angle;    //마우스 커서에 따른 무기 회전각도
    protected bool isFire = false;
    protected bool isReload = false;

    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        player_OrderInLayer = player.GetComponent<SpriteRenderer>().sortingOrder;

        transform.SetParent(GameObject.FindWithTag("Player").transform);  //플레이어를 부모로 설정
        transform.localPosition = new Vector2(localPositionX, localPositionY);  //위치 조정

        //총알의 갯수 초기화
        currentBulletCount = bulletCount;
        //탄창(남은 총알의 갯수 / 모든 총알의 갯수)UI 갱신
        UIManager.instance.MagazineUI_Update(currentBulletCount, bulletCount);
    }

    protected virtual void Update()
    {
        //마우스 좌표 구하기
        mouseVec = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //총구와 마우스사이의 방향
        muzzleToMouse_Direction = mouseVec - (Vector2)muzzle.transform.position;
    }

    public void Render(Vector2 playerToMouse_Direction)
    {
        //무기와 마우스위치 사이의 각도계산
        weaponToMouse_angle = Mathf.Atan2(mouseVec.y - transform.position.y, mouseVec.x - transform.position.x) * Mathf.Rad2Deg;
        //플레이어가 왼쪽을 바라볼때와 오른쪽을 바라볼때 적용해야할 값이 다르다
        transform.rotation = Quaternion.AngleAxis(playerToMouse_Direction.normalized.x > 0.0f ? weaponToMouse_angle : weaponToMouse_angle + 180, Vector3.forward);

        //플레이어위를 바라볼때는 무기가 가려져야하고, 통상적으로 앞을 볼때는 무기가 플레이어보다 앞에 출력되어야한다.
        if ((-0.7f <= playerToMouse_Direction.normalized.x && playerToMouse_Direction.normalized.x <= 0.7f)
            && (0.7f <= playerToMouse_Direction.normalized.y && playerToMouse_Direction.normalized.y <= 1.0f))    //위쪽(부채꼴 범위)을 바라보았을 경우
            Sprite.sortingOrder = player_OrderInLayer - 1;
        else
            Sprite.sortingOrder = player_OrderInLayer + 1;
    }

    public virtual void Fire() { }

    public virtual void Reload()
    {
        if (!isReload)
        {
            isReload = true;
            StartCoroutine(ReloadTime());
        }
    }

    private IEnumerator ReloadTime()
    {
        reloadAudio.Play();     //재장전 소리 출력
        yield return new WaitForSeconds(reloadTime);
        currentBulletCount = bulletCount;    //총알 갯수 채우기
        //탄창(남은 총알의 갯수 / 모든 총알의 갯수)UI 갱신
        UIManager.instance.MagazineUI_Update(currentBulletCount, bulletCount);   
        isReload = false;   //변수 원위치
        player.state = Player.State.Idle;  //플레이어의 상태를 Idle로 되돌리기
    }
}
