/* 
 * @file : Enemy.cs 
 * @date : 2021.04.23
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 적 AI, 플레이어 공격
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private enum State
    { 
        Idle,
        Chase,
        Attack,
    }
    private State state = State.Idle;

    [SerializeField] private float hp;
    [SerializeField] private GameObject weapon;
    [SerializeField] protected GameObject muzzle;
    [SerializeField] private float attackDistance;
    [SerializeField] private GameObject hpBar;
    [SerializeField] private Vector2 hpBarPosition;
    [SerializeField] private GameObject miniMapIcon;
    [SerializeField] private Color hitMotion_Color;
    [SerializeField] private float hitMotion_Time;
    [SerializeField] protected AudioSource shotAudio;
    [SerializeField] private AudioSource deathAudio;
    [SerializeField] public bool isActivity = false;

    protected Rigidbody2D myRigidbody;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected NavMeshAgent navMeshAgent;
    private Slider hpBarSlider;
    private GameObject player;
    protected GameObject poolingBullet;   //가져올 총알
    private GameObject hpBarCanvas;
    [HideInInspector] public GameObject hpBarObject;
    private RaycastHit2D hit;
    private Color enemy_Color;
    protected Vector2 enemyToPlayer_Direction;
    protected Vector2 muzzleToPlayer_Direction;
    private int layerMask;
    protected float enemyToPlayer_Distance;
    private float weaponToPlayer_Angle;
    private float totalHp;
    private bool isPlayerCheck = false;
    protected bool isFire = false;

    protected virtual void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        navMeshAgent = GetComponent<NavMeshAgent>(); 

        //navMehsAgent는 통상 3D에서 적용할 수 있는 기능이다.
        //2D에서 적용할경우 개별적으로 꺼줘야할 기능이 있다.
        navMeshAgent.updateRotation = false;    //회전값에 변동을주지 않아야하므로 off
        navMeshAgent.updateUpAxis = false;  //upAxis에 변동을 주지 않아야하므로 off

        hpBarCanvas = GameObject.FindWithTag("HpBarCanvas");    //Hp바 전용캔버스
        hpBarObject = Instantiate(hpBar, hpBar.transform.position , Quaternion.identity, hpBarCanvas.transform);  //적의 체력hp바 생성
        hpBarSlider = hpBarObject.GetComponent<Slider>();

        totalHp = hp;   //hp슬라이더 value에 적용할 총 hp량 저장

        miniMapIcon.SetActive(true);    //미니맵 아이콘 활성화

        enemy_Color = spriteRenderer.color;    //스테이지마다 적군의 스프라이트랜더러의 컬러값이 다르기 때문에 설정해놓은 컬러값을 저장해놓는다.

        //일부 레이어들은 Ray충돌에서 제외
        layerMask = (1 << LayerMask.NameToLayer("PlayerBullet")) | (1 << LayerMask.NameToLayer("Enemy"))
                        | (1 << LayerMask.NameToLayer("EnemyBullet"));
        layerMask = ~layerMask;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        Die();  //사망시 발생, 바로 Destroy되지 않음

        hpBarObject.transform.position = Camera.main.WorldToScreenPoint((Vector2)gameObject.transform.position + hpBarPosition);  //HpBar 위치 조정

        //플레이어가 존재하고 활동을 개시하면 Attack or Chase, 아니라면 Idle로 대기
        if (GameObject.FindWithTag("Player") != null && isActivity == true)
        {
            if (!isPlayerCheck)     //한번만 실행
            {
                isPlayerCheck = true;
                player = GameObject.FindWithTag("Player");
            }
            enemyToPlayer_Direction = player.transform.position - transform.position;   //적과 플레이어 사이의 방향
            enemyToPlayer_Distance = Vector2.Distance(player.transform.position, transform.position);   //적과 플레이어 사이의 거리
            muzzleToPlayer_Direction = player.transform.position - muzzle.transform.position;     //총구와 플레이어 사이의 방향
            RayCheck(); //공격범위안에 플레이어가 있는지 체크

            //공격범위 안이고, 공격방향에 장애물이 없으면 Attack 아니면 Chase
            if (enemyToPlayer_Distance <= attackDistance && hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                StopMove();     //움직임 정지
                state = State.Attack;
            }
            else
            {
                navMeshAgent.isStopped = false;     //움직임 활성화(한번만 실행해도되기 때문에 state를 변경하기전에 실행)
                state = State.Chase;
            }
        }
        else if (GameObject.FindWithTag("Player") == null) 
        {
            isPlayerCheck = false;  //원위치
            StopMove();
            state = State.Idle;
        }

        switch (state)
        {
            case State.Idle:
                animator.SetBool("Walk", false);    
                break;
            case State.Chase:
                Render();
                navMeshAgent.SetDestination(player.transform.position);     //플레이어 추적
                animator.SetBool("Walk", true);    
                break;
            case State.Attack:
                Render();
                Attack();
                animator.SetBool("Walk", false);   
                break;
        }
    }

    protected virtual void Render()
    {
        //----------- 스케일값을 조정함으로써 적의 시선을 플레이어의 위치에 따라 변경한다. -----------
        if (enemyToPlayer_Direction.normalized.x < 0.0f)     //플레이어가 적의 왼쪽에 위치한 경우
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else     //플레이어가 적의 왼쪽의 위치한 경우
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        //위를 바라보거나 통상적으로 앞을 바라보았을때, 처리해야 할것들
        if ((-0.7f <= enemyToPlayer_Direction.normalized.x && enemyToPlayer_Direction.normalized.x <= 0.7f)
            && (0.7f <= enemyToPlayer_Direction.normalized.y && enemyToPlayer_Direction.normalized.y <= 1.0f))    //위쪽(부채꼴 범위)을 바라보았을 경우
            animator.SetBool("LookAtUp", true);
        else
            animator.SetBool("LookAtUp", false);

        //----------- 무기의 회전처리 -----------
        //무기와 플레이어 사이의 각도계산
        weaponToPlayer_Angle = Mathf.Atan2(player.transform.position.y - weapon.transform.position.y, player.transform.position.x - weapon.transform.position.x) * Mathf.Rad2Deg;
        //적이 왼쪽을 바라볼때와 오른쪽을 바라볼때 적용해야할 값이 다르다
        weapon.transform.rotation = Quaternion.AngleAxis(enemyToPlayer_Direction.normalized.x > 0.0f ? weaponToPlayer_Angle : weaponToPlayer_Angle + 180, Vector3.forward);

        //적과 플레이어의 위치에 따라 바라보는 애니메이션
        animator.SetFloat("PosX", enemyToPlayer_Direction.normalized.x);
        animator.SetFloat("PosY", enemyToPlayer_Direction.normalized.y);
    }

    protected virtual void Attack() { }

    private void StopMove()
    {
        navMeshAgent.isStopped = true;  //움직임 비활성화(한번만 실행해도되기 때문에 state를 변경하기전에 실행)
        navMeshAgent.velocity = Vector2.zero;   //velocity를 0으로 초기화함으로써 관성으로 인한 미끄러짐 현상을 방지할 수 있다.
    }

    protected void RayCheck()
    {
        hit = Physics2D.Raycast(transform.position, enemyToPlayer_Direction, attackDistance, layerMask);   //Ray발사 후 충돌저장
        Debug.DrawRay(transform.position, enemyToPlayer_Direction.normalized * attackDistance, Color.red);     //Ray 출력
    }

    public virtual void Die()
    {
        if (hp <= 0.0f)
        {
            //방안에 생존해 있는 적 체크를 위한 리스트에서 현재 적 오브젝트 제거
            transform.parent.GetComponent<RoomManager>().enemyList.Remove(gameObject);
            deathAudio.Play();  
            Destroy(hpBarObject);   
            navMeshAgent.speed = 0.0f;      //움직임 정지
            gameObject.GetComponent<CapsuleCollider2D>().enabled = false;   //사망했으므로 충돌제거
            animator.SetTrigger("Die");     //사망 애니메이션
            this.enabled = false;
        }
    }

    public virtual void Destroy()   //애니메이션 이벤트
    {
        Destroy(gameObject);
    }

    //보스몹의 색이 잠시 바뀌는 모션
    IEnumerator HitMotion()     
    {
        spriteRenderer.color = hitMotion_Color;
        yield return new WaitForSeconds(hitMotion_Time);
        spriteRenderer.color = enemy_Color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //플레이어의 총알에 피격됬을 경우
        if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
        {
            hp = hp - collision.transform.GetComponent<Bullet>().damage;
            hpBarSlider.value = hp / totalHp;     //hpBarUI 갱신
            StartCoroutine(HitMotion());
        }
    }
}
