/* 
 * @file : Stage1_Boss.cs 
 * @date : 2021.05.14
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 스테이지1 보스 공격패턴
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Stage1_Boss : MonoBehaviour
{
    private enum State
    {
        Idle,
        Pattern1,    
        Pattern2,
        Pattern3,
        Pattern4,
    }
    private State state = State.Idle;

    [SerializeField] private float hp;
    [SerializeField] private GameObject weapon;
    [SerializeField] private GameObject muzzle;
    [SerializeField] private float narrowPlayer_Distance;   //플레이어와 좁히고싶은 거리
    [SerializeField] private GameObject hpBar;
    [SerializeField] private Vector2 hpBarPosition;
    [SerializeField] private Color hitMotion_Color;
    [SerializeField] private float hitMotion_Time;
    [SerializeField] private GameObject miniMapIcon;
    [SerializeField] private AudioSource shotAudio;
    [SerializeField] private AudioSource deathAudio;
    //패턴1
    [SerializeField] private int pattern1_FireBulletCount;
    [SerializeField] private float pattern1_AttackRangeAngle;   //공격범위각도
    [SerializeField] private float pattern1_FireDelay;          //한번 난사후 다음에 난사하기 까지의 시간
    //패턴2
    [SerializeField] private float pattern2_Speed;              //대쉬 속도
    //패턴3
    [SerializeField] private int pattern3_FireBulletCount;    //발사할 총알 갯수
    //패턴4
    [SerializeField] private int pattern4_OnceScatterBulletCount;   //한번 흩뿌리는데 발사할 총알 갯수
    [SerializeField] private float pattern4_AttackRangeAngle;   //보스가 플레이어쪽으로 총알을 발사 할 각도

    private Rigidbody2D myRigidbody;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Slider hpBarSlider;
    private GameObject player;
    private GameObject poolingBullet;   //가져올 총알
    private GameObject hpBarCanvas;
    private GameObject hpBarObject;
    private NavMeshAgent navMeshAgent;
    private Vector2 bossToPlayer_Direction;
    private Vector2 muzzleToPlayer_Direction;
    private Vector2 pattern3_BulletDirection;
    private Vector2 pattern4_BulletDirection;
    private Vector2 pattern4_BossToPlayerDirection;
    private int pattern4_ScatterBulletDirection;    //-1이라면 위 방향으로 흩뿌리고 1이라면 아래방향으로 흩뿌린다.
    private float bossToPlayer_Distance;
    private float weaponToPlayer_Angle;
    private float totalHp;
    private float pattern4_AngleBetweenBullet;
    private bool isPlayerCheck = false;
    private bool isPlayerInDistance = false;
    public bool isActivity = false;    //BossRoomManager에서 플레이어가 방입장시 true로 변경
    private bool isPattern = false;

    void Start()
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
        hpBarObject = Instantiate(hpBar, (Vector2)hpBarCanvas.transform.position + hpBarPosition, Quaternion.identity, hpBarCanvas.transform);  //적의 체력hp바 생성
        hpBarSlider = hpBarObject.GetComponent<Slider>();
            
        totalHp = hp;   //hp슬라이더 value에 적용할 총 hp량 저장

        miniMapIcon.SetActive(true);    //미니맵 아이콘 활성화

        isPlayerInDistance = bossToPlayer_Distance > narrowPlayer_Distance;     //처음 실행하는 패턴1에 사용 - 플레이어가 거리안에 있는지 확인
        pattern4_AngleBetweenBullet = pattern4_AttackRangeAngle / (pattern4_OnceScatterBulletCount - 1);    //패턴4 - 총알사이의 각도 미리계산
    }

    void Update()
    {
        Die();      //사망시 발생, 바로 Destroy되지 않음

        //플레이어가 존재하고 활동을 개시하면 Attack or Chase, 아니라면 Idle로 대기
        if (GameObject.FindWithTag("Player") != null && isActivity)
        {
            if (!isPlayerCheck)     //한번만 실행
            {
                isPlayerCheck = true;
                player = GameObject.FindWithTag("Player");
                NextPattern();
            }

            bossToPlayer_Direction = player.transform.position - transform.position;   //적과 플레이어 사이의 방향
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
                animator.SetBool("Walk", false);    //걷는 애니메이션 해제
                break;
            case State.Pattern1:    //패턴1 : 플레이어 캐릭터 방향으로 이동하면서 플레이어 캐릭터 방향기준으로 일정한 각도안에서 무작위 각도로 난사
                BossRender();
                WeaponRender();

                if (!isPattern)
                {
                    isPattern = true;
                    StartCoroutine(Pattern1());
                }

                bossToPlayer_Distance = Vector2.Distance(player.transform.position, transform.position);   //적과 플레이어 사이의 거리

                //보스가 플레이어와의 거리를 충분히 좁혔다면 더이상 접근하지않음
                if (bossToPlayer_Distance <= narrowPlayer_Distance)
                {
                    if (!isPlayerInDistance)     //한번만 실행해주기 위함
                    {
                        isPlayerInDistance = true;
                        StopMove();     //움직임 정지
                    }
                    animator.SetBool("Walk", false);    //걷는 애니메이션 해제
                }
                else
                {
                    if (isPlayerInDistance)
                    {
                        isPlayerInDistance = false;
                        navMeshAgent.isStopped = false;     //움직임 활성화(한번만 실행해도되기 때문에 state를 변경하기전에 실행)
                    }
                    navMeshAgent.SetDestination(player.transform.position);
                    animator.SetBool("Walk", true);    //걷는 애니메이션 실행
                }
                break;
            case State.Pattern2:    //패턴2 : 플레이어 쪽으로 대쉬하는 패턴
                BossRender();

                if (!isPattern)
                {
                    isPattern = true;
                    StopMove();
                    animator.SetTrigger("Pattern2");
                }
                break;
            case State.Pattern3:     //패턴3 : 총알을 전방향으로 한번에 발사하는 패턴
                if (!isPattern)
                {
                    isPattern = true;
                    StopMove();
                    animator.SetTrigger("Pattern3");
                }
                break;
            case State.Pattern4:    //패턴4 : 아래, 위, 아래 방향으로 3번 총알들을 흩뿌리는 패턴
                if (!isPattern)
                {
                    isPattern = true;
                    StopMove();
                    animator.SetTrigger("Pattern4");
                }
                break;
        }
    }

    private void BossRender()
    {
        //----------- 스케일값을 조정함으로써 보스의 시선을 플레이어의 위치에 따라 변경한다. -----------
        if (bossToPlayer_Direction.normalized.x < 0.0f)     //플레이어가 보스의 왼쪽에 위치한 경우
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else     //플레이어가 보스의 오른쪽에 위치한 경우
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        //위를 바라보거나 통상적으로 앞을 바라보았을때, 처리해야 할것들
        if ((-0.7f <= bossToPlayer_Direction.normalized.x && bossToPlayer_Direction.normalized.x <= 0.7f)
            && (0.7f <= bossToPlayer_Direction.normalized.y && bossToPlayer_Direction.normalized.y <= 1.0f))    //위쪽(부채꼴 범위)을 바라보았을 경우
            animator.SetBool("LookAtUp", true);
        else
            animator.SetBool("LookAtUp", false);

        //적과 플레이어의 위치에 따라 바라보는 애니메이션
        animator.SetFloat("PosX", bossToPlayer_Direction.normalized.x);
        animator.SetFloat("PosY", bossToPlayer_Direction.normalized.y);
    }

    private void WeaponRender()
    {
        //----------- 무기의 회전처리 -----------
        //무기와 플레이어 사이의 각도계산
        weaponToPlayer_Angle = Mathf.Atan2(player.transform.position.y - weapon.transform.position.y, player.transform.position.x - weapon.transform.position.x) * Mathf.Rad2Deg;
        //보스가 왼쪽을 바라볼때와 오른쪽을 바라볼때 적용해야할 값이 다르다
        weapon.transform.rotation = Quaternion.AngleAxis(bossToPlayer_Direction.normalized.x > 0.0f ? weaponToPlayer_Angle : weaponToPlayer_Angle + 180, Vector3.forward);
    }

    private void StopMove()
    {
        navMeshAgent.isStopped = true;  //움직임 비활성화(한번만 실행해도되기 때문에 state를 변경하기전에 실행)
        navMeshAgent.velocity = Vector2.zero;   //velocity를 0으로 초기화함으로써 관성으로 인한 미끄러짐 현상을 방지할 수 있다.
    }

    //다음패턴 결정
    public void NextPattern()   
    {
        isPattern = false;     //Update에서 한번만 실행할 명령어를 위한 변수원위치
        state = (State)(Random.Range((int)State.Pattern1, (int)State.Pattern4 + 1));    //랜덤하게 패턴 결정

        if (state == State.Pattern1)    //만약 Pattern1이 결정됬을경우 플레이어가 보스의 접근거리 안에 있는지 검사
        {
            bossToPlayer_Distance = Vector2.Distance(player.transform.position, transform.position);   //적과 플레이어 사이의 거리
            isPlayerInDistance = bossToPlayer_Distance > narrowPlayer_Distance;
        }
    }

    public void IdleTrigger()
    {
        animator.SetTrigger("Idle");
    }

    //플레이어 캐릭터 방향기준으로 일정한 각도안에서 무작위 각도로 난사
    IEnumerator Pattern1()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < pattern1_FireBulletCount; j++)
            {
                muzzleToPlayer_Direction = (player.transform.position - muzzle.transform.position);     //총구와 플레이어 사이의 방향
                shotAudio.Play();
                poolingBullet = PoolingManager.instance.GetObject("Boss_Bullet");
                poolingBullet.transform.position = muzzle.transform.position;
                poolingBullet.GetComponent<Bullet>().direction = Quaternion.Euler(0, 0, Random.Range(-pattern1_AttackRangeAngle, pattern1_AttackRangeAngle))
                    * muzzleToPlayer_Direction;
                poolingBullet.transform.up = Quaternion.Euler(0, 0, Random.Range(-pattern1_AttackRangeAngle, pattern1_AttackRangeAngle))
                    * muzzleToPlayer_Direction; 

                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(pattern1_FireDelay);
        }

        NextPattern();
    }

    //플레이어 쪽으로 대쉬
    public IEnumerator Pattern2()  //애니메이션 이벤트
    {
        myRigidbody.velocity = ((player.transform.position - transform.position).normalized * pattern2_Speed);  //돌진 시 속도 설정
        yield return new WaitForSeconds(Vector2.Distance(player.transform.position, transform.position) / pattern2_Speed);  //거리 / 속도 = 시간
        myRigidbody.velocity = Vector2.zero;    //돌진 완료시 속도값 0
        IdleTrigger();
        NextPattern();
    }

    //전방향으로 한번에 발사
    public void Pattern3()  //애니메이션 이벤트
    {
        for (int i = 0; i < pattern3_FireBulletCount; i++)
        {
            poolingBullet = PoolingManager.instance.GetObject("Boss_Bullet");
            poolingBullet.transform.position = transform.position;      //총구가 아닌 보스몹 본체에서 생성
            pattern3_BulletDirection = Quaternion.Euler(new Vector3(0.0f, 0.0f, (360.0f / pattern3_FireBulletCount) * i)) * bossToPlayer_Direction;
            poolingBullet.GetComponent<Bullet>().direction = pattern3_BulletDirection;
            poolingBullet.transform.up = pattern3_BulletDirection;
        }
    }

    //아래, 위, 아래 순서로 3번 총알들을 흩뿌리는 패턴
    public IEnumerator Pattern4()
    {
        //---------- 발사하기 시작할때 스케일 값을 조정해줘야 함 ------------
        if (bossToPlayer_Direction.normalized.x < 0.0f)     //플레이어가 보스의 왼쪽에 위치한 경우
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else     //플레이어가 보스의 오른쪽에 위치한 경우
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        pattern4_BossToPlayerDirection = player.transform.position - transform.position;    //기준이 될 발사 방향
        for (int i = 0; i < 3; i++)
        {  
            pattern4_ScatterBulletDirection = (i + 1) % 2 == 1 ? 1 : -1; 
            for (int j = 0; j < pattern4_OnceScatterBulletCount; j++)
            {
                //발사할 총알 방향계산
                pattern4_BulletDirection = Quaternion.Euler(new Vector3(0.0f, 0.0f, (pattern4_ScatterBulletDirection * (-pattern4_AttackRangeAngle / 2.0f)) 
                    + (pattern4_ScatterBulletDirection * (pattern4_AngleBetweenBullet * j)))) * pattern4_BossToPlayerDirection;
                //무기의 rotation값 조정
                weaponToPlayer_Angle = Mathf.Atan2(pattern4_BulletDirection.y, pattern4_BulletDirection.x) * Mathf.Rad2Deg;
                weapon.transform.rotation = Quaternion.AngleAxis(pattern4_BossToPlayerDirection.normalized.x > 0.0f ? weaponToPlayer_Angle : weaponToPlayer_Angle + 180, Vector3.forward);
                //총알 발사
                shotAudio.Play();
                poolingBullet = PoolingManager.instance.GetObject("Boss_Bullet");
                poolingBullet.transform.position = muzzle.transform.position;
                poolingBullet.GetComponent<Bullet>().direction = pattern4_BulletDirection;
                poolingBullet.transform.up = pattern4_BulletDirection;
                yield return new WaitForSeconds(0.1f);
            }
        }

        IdleTrigger();
        NextPattern();
    }

    private void Die()
    {
        if (hp <= 0.0f)
        {
            transform.parent.GetComponent<BossRoomManager>().isClear = true;
            deathAudio.Play();
            Destroy(hpBarObject);
            navMeshAgent.speed = 0.0f;      //움직임 정지
            gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
            animator.SetTrigger("Die");     //애니메이션
            this.enabled = false;
        }
    }

    //사망 모션 출력후 실행할 애니메이션 이벤트
    private void Destroy()
    {
        Destroy(gameObject);
    }

    IEnumerator HitMotion()     //보스몹의 색이 잠시 바뀌는 모션
    {
        spriteRenderer.color = hitMotion_Color;
        yield return new WaitForSeconds(hitMotion_Time);
        spriteRenderer.color = new Color(255, 255, 255, 255);
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