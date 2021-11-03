/* 
 * @file : Stage3_Boss.cs 
 * @date : 2021.06.04
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 스테이지3 보스 공격패턴
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Stage3_Boss : MonoBehaviour
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
    [SerializeField] private GameObject hpBar;
    [SerializeField] private Vector2 hpBarPosition;
    [SerializeField] private Color hitMotion_Color;
    [SerializeField] private float hitMotion_Time;
    [SerializeField] private GameObject miniMapIcon;
    [SerializeField] private GameObject king;
    [SerializeField] private GameObject throne;
    [SerializeField] private GameObject topMuzzle;
    [SerializeField] private GameObject leftMuzzle;
    [SerializeField] private GameObject rightMuzzle;
    [SerializeField] private GameObject leftBottomMuzzle;
    [SerializeField] private GameObject rightBottomMuzzle;
    [SerializeField] private AudioSource shotAudio;
    [SerializeField] private AudioSource spinShotAudio;
    //패턴3
    [SerializeField] private int pattern3_FireBulletCount;
    //패턴4
    [SerializeField] private int pattern4_FireBulletCount;

    private Rigidbody2D myRigidbody;
    private Animator kingAnimator;
    private Animator throneAnimator;
    private Slider hpBarSlider;
    private SpriteRenderer king_SpriteRenderer;
    private SpriteRenderer throne_SpriteRenderer;
    private GameObject player;
    private GameObject poolingBullet;   //가져올 총알
    private GameObject hpBarCanvas;
    [HideInInspector] private GameObject hpBarObject;
    private GameObject pattern1_ShotMuzzle;
    private NavMeshAgent navMeshAgent;
    private Vector2 bossToPlayer_Direction;
    private Vector2 pattern3_BulletDirection;
    private Vector2 pattern4_BulletDirection;
    private float totalHp;
    private float pattern4_WeightAngle;
    private bool isPlayerCheck = false;
    public bool isActivity = false;    //BossRoomManager에서 플레이어가 방입장시 true로 변경
    private bool isPattern = false;
    private bool isChase = false;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        kingAnimator = king.GetComponent<Animator>();
        throneAnimator = throne.GetComponent<Animator>();
        king_SpriteRenderer = king.GetComponent<SpriteRenderer>();
        throne_SpriteRenderer = throne.GetComponent<SpriteRenderer>();

        //navMehsAgent는 통상 3D에서 적용할 수 있는 기능이다. 
        //2D에서 적용할경우 개별적으로 꺼줘야할 기능이 있다.
        navMeshAgent.updateRotation = false;    //회전값에 변동을주지 않아야하므로 off
        navMeshAgent.updateUpAxis = false;  //upAxis에 변동을 주지 않아야하므로 off

        hpBarCanvas = GameObject.FindWithTag("HpBarCanvas");    //Hp바 전용캔버스
        hpBarObject = Instantiate(hpBar, (Vector2)hpBarCanvas.transform.position + hpBarPosition, Quaternion.identity, hpBarCanvas.transform);  //적의 체력hp바 생성
        hpBarSlider = hpBarObject.GetComponent<Slider>();

        totalHp = hp;   //hp슬라이더 value에 적용할 총 hp량 저장

        miniMapIcon.SetActive(true);    //미니맵 아이콘 활성화
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
                isPattern = false;
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
                break;
            case State.Pattern1:    //패턴1 : 플레이어의 위치에 따라 발사할 총구를 정한 뒤, 3번 발사
                if (!isPattern)
                {
                    isPattern = true; 
                    navMeshAgent.isStopped = false;     //움직임 활성화
                    kingAnimator.SetTrigger("Pattern1");
                }
                navMeshAgent.SetDestination(player.transform.position);
                break;
            case State.Pattern2:    //패턴2 : 모든 총구에서 총구방향으로 연발
                if (!isPattern)
                {
                    isPattern = true;
                    StopMove();
                    kingAnimator.SetTrigger("Pattern2");
                }

                if(isChase)
                    navMeshAgent.SetDestination(player.transform.position);
                break;
            case State.Pattern3:    //패턴3 : 전체방향으로 총알발사
                if (!isPattern)
                {
                    isPattern = true;
                    StopMove();
                    kingAnimator.SetTrigger("Pattern3");
                }

                if (isChase)
                    navMeshAgent.SetDestination(player.transform.position);
                break;
			case State.Pattern4:    //패턴4 : 발사각를 조금씩 변경하면서 전체방향으로 총알을 여러번 발사
                if (!isPattern)
                {
                    isPattern = true;
                    StopMove();
                    throneAnimator.SetTrigger("Pattern4");
                    kingAnimator.SetTrigger("Pattern4");
                }

                if (isChase)
                    navMeshAgent.SetDestination(player.transform.position);
                break;
        }
	}

	private void StopMove()
    {
        navMeshAgent.isStopped = true;  //움직임 비활성화(한번만 실행해도되기 때문에 state를 변경하기전에 실행)
        navMeshAgent.velocity = Vector2.zero;   //velocity를 0으로 초기화함으로써 관성으로 인한 미끄러짐 현상을 방지할 수 있다.
    }

    public void NextPattern()
    {
        isPattern = false;     //Update에서 한번만 실행할 명령어를 위한 변수 원위치
        state = (State)(Random.Range((int)State.Pattern1, (int)State.Pattern4 + 1));    //랜덤하게 패턴 결정
    }

    public void Pattern1()  
    {
        StartCoroutine(Pattern1_IEnumerator());
    }

    private IEnumerator Pattern1_IEnumerator()
	{
        for (int i = 0; i < 2; i++)
        {
            if ((-0.7f <= bossToPlayer_Direction.normalized.x && bossToPlayer_Direction.normalized.x <= 0.7f)
                && (0.7f <= bossToPlayer_Direction.normalized.y && bossToPlayer_Direction.normalized.y <= 1.0f))    //플레이어가 위쪽(부채꼴범위)에 위치한 경우
                pattern1_ShotMuzzle = topMuzzle;
            else if ((-1.0f <= bossToPlayer_Direction.normalized.x && bossToPlayer_Direction.normalized.x < -0.7f)
                && (-0.7f <= bossToPlayer_Direction.normalized.y && bossToPlayer_Direction.normalized.y < 0.7f))    //왼쪽에 위치한 경우
                pattern1_ShotMuzzle = leftMuzzle;
            else if (0.7f < bossToPlayer_Direction.normalized.x && bossToPlayer_Direction.normalized.x <= 1.0f
                 && (-0.7f <= bossToPlayer_Direction.normalized.y && bossToPlayer_Direction.normalized.y < 0.7f))   //오른쪽에 위치한 경우
                pattern1_ShotMuzzle = rightMuzzle;
            else if ((-0.7f <= bossToPlayer_Direction.normalized.x && bossToPlayer_Direction.normalized.x < 0.0f)
                && (-1.0f < bossToPlayer_Direction.normalized.y && bossToPlayer_Direction.normalized.y <= -0.7f))  //왼쪽 아래에 위치한 경우
                pattern1_ShotMuzzle = leftBottomMuzzle;
            else if ((0.0f <= bossToPlayer_Direction.normalized.x && bossToPlayer_Direction.normalized.x <= 0.7f)
                && (-1.0f <= bossToPlayer_Direction.normalized.y && bossToPlayer_Direction.normalized.y <= -0.7f))   //오른쪽 아래에 위치한 경우
                pattern1_ShotMuzzle = rightBottomMuzzle;

            for (int j = 0; j < pattern1_ShotMuzzle.transform.childCount; j++)
            {
                shotAudio.Play();
                poolingBullet = PoolingManager.instance.GetObject("Boss_Bullet");
                poolingBullet.transform.position = pattern1_ShotMuzzle.transform.GetChild(j).transform.position;
                poolingBullet.GetComponent<Bullet>().direction = (Vector2)(player.transform.position - pattern1_ShotMuzzle.transform.GetChild(j).transform.position);
                poolingBullet.transform.up = (Vector2)(player.transform.position - pattern1_ShotMuzzle.transform.GetChild(j).transform.position);

				yield return new WaitForSeconds(0.1f);
			}

			yield return new WaitForSeconds(0.2f);
        }

        kingAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(1.0f);
        NextPattern();
    }

    public void Pattern2()
    {
        StartCoroutine(Pattern2_IEnumerator());
    }

    private IEnumerator Pattern2_IEnumerator()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                poolingBullet = PoolingManager.instance.GetObject("Boss_Bullet");
                poolingBullet.transform.position = topMuzzle.transform.GetChild(j).transform.position;
                poolingBullet.GetComponent<Bullet>().direction = topMuzzle.transform.GetChild(j).transform.right;
                poolingBullet.transform.up = topMuzzle.transform.GetChild(j).transform.right;

                poolingBullet = PoolingManager.instance.GetObject("Boss_Bullet");
                poolingBullet.transform.position = leftMuzzle.transform.GetChild(j).transform.position;
                poolingBullet.GetComponent<Bullet>().direction = leftMuzzle.transform.GetChild(j).transform.right;
                poolingBullet.transform.up = leftMuzzle.transform.GetChild(j).transform.right;

                poolingBullet = PoolingManager.instance.GetObject("Boss_Bullet");
                poolingBullet.transform.position = rightMuzzle.transform.GetChild(j).transform.position;
                poolingBullet.GetComponent<Bullet>().direction = rightMuzzle.transform.GetChild(j).transform.right;
                poolingBullet.transform.up = rightMuzzle.transform.GetChild(j).transform.right;

                poolingBullet = PoolingManager.instance.GetObject("Boss_Bullet");
                poolingBullet.transform.position = leftBottomMuzzle.transform.GetChild(j).transform.position;
                poolingBullet.GetComponent<Bullet>().direction = leftBottomMuzzle.transform.GetChild(j).transform.right;
                poolingBullet.transform.up = leftBottomMuzzle.transform.GetChild(j).transform.right;

                poolingBullet = PoolingManager.instance.GetObject("Boss_Bullet");
                poolingBullet.transform.position = rightBottomMuzzle.transform.GetChild(j).transform.position;
                poolingBullet.GetComponent<Bullet>().direction = rightBottomMuzzle.transform.GetChild(j).transform.right;
                poolingBullet.transform.up = rightBottomMuzzle.transform.GetChild(j).transform.right;
            }

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);
        navMeshAgent.isStopped = false;     //움직임 활성화
        isChase = true;     //플레이어 추적
        kingAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(1.0f);
        isChase = false;    //추적해제
        NextPattern();
    }

    public void Pattern3()
    {
        StartCoroutine(Pattern3_IEnumerator());
    }

    private IEnumerator Pattern3_IEnumerator()  //애니메이션 이벤트
    {
        //총알이 보스몹 본체에서 생성되므로 보스몹이 총알보다 우선 출력되게 설정
        throne.GetComponent<SpriteRenderer>().sortingOrder = 4;
        king.GetComponent<SpriteRenderer>().sortingOrder = 5;
        for (int i = 0; i < pattern3_FireBulletCount; i++)
        {
            poolingBullet = PoolingManager.instance.GetObject("Boss_Bullet");
            poolingBullet.transform.position = transform.position;      //총구가 아닌 보스몹 본체에서 생성
            pattern3_BulletDirection = Quaternion.Euler(new Vector3(0.0f, 0.0f, (360.0f / pattern3_FireBulletCount) * i)) * bossToPlayer_Direction;
            poolingBullet.GetComponent<Bullet>().direction = pattern3_BulletDirection;
            poolingBullet.transform.up = pattern3_BulletDirection;
        }

        yield return new WaitForSeconds(0.5f);
        navMeshAgent.isStopped = false;     //움직임 활성화
        isChase = true;     //플레이어 추적
        kingAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(1.0f);
        isChase = false;    //추적해제
        //원위치
        throne.GetComponent<SpriteRenderer>().sortingOrder = 2;
        king.GetComponent<SpriteRenderer>().sortingOrder = 3;
        NextPattern();
    }

    public void Pattern4()
    {
        StartCoroutine(Pattern4_IEnumerator());
    }

    IEnumerator Pattern4_IEnumerator()
	{
        king.SetActive(false);    //Throne만 출력되므로 King은 꺼준다
        spinShotAudio.Play(); 
        throneAnimator.SetTrigger("Pattern4_2");
        //총알이 보스몹 본체에서 생성되므로 보스몹이 총알보다 우선 출력되게 설정
        throne.GetComponent<SpriteRenderer>().sortingOrder = 4;

        for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < pattern4_FireBulletCount; j++)
			{
				poolingBullet = PoolingManager.instance.GetObject("Boss_Bullet");
                poolingBullet.transform.position = transform.position;      //총구가 아닌 보스몹 본체에서 생성
                pattern4_BulletDirection = Quaternion.Euler(new Vector3(0.0f, 0.0f, pattern4_WeightAngle + ((360.0f / pattern4_FireBulletCount) * j))) * bossToPlayer_Direction;
                poolingBullet.GetComponent<Bullet>().direction = pattern4_BulletDirection;
                poolingBullet.transform.up = pattern4_BulletDirection;
            }

            pattern4_WeightAngle += (360.0f / pattern4_FireBulletCount) / 2.0f;
            yield return new WaitForSeconds(0.3f);
		}

        throne.GetComponent<SpriteRenderer>().sortingOrder = 2;     //원위치
        throneAnimator.SetTrigger("Idle");
        king.SetActive(true);
        kingAnimator.SetTrigger("Pattern4_2");
        spinShotAudio.Stop();
    }

    IEnumerator HitMotion()     //보스몹의 색이 잠시 바뀌는 모션
    {
        king_SpriteRenderer.color = hitMotion_Color;
        throne_SpriteRenderer.color = hitMotion_Color;
        yield return new WaitForSeconds(hitMotion_Time);
        king_SpriteRenderer.color = new Color(255, 255, 255, 255);
        throne_SpriteRenderer.color = new Color(255, 255, 255, 255);
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

    private void Die()
    {
        if (hp <= 0.0f)
        {
            transform.parent.GetComponent<BossRoomManager>().isClear = true;
            Destroy(hpBarObject);
            navMeshAgent.speed = 0.0f;      //움직임 정지
            gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
            kingAnimator.SetTrigger("Die"); //사망 애니메이션
            this.enabled = false;
        }
    }
}
