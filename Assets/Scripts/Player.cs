/* 
 * @file : Player.cs 
 * @date : 2021.04.18
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 플레이어 캐릭터의 조작
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum State
    {
        Idle,
        Dash,
        Reloading,
        Stop
    }
    [SerializeField] public State state = State.Idle;

    [SerializeField] public float hp;
    [SerializeField] public float walkSpeed;
    [SerializeField] public float dashSpeed;    //대쉬속도
    [SerializeField] private float dashTime;    //대쉬상태유지 시간
    [SerializeField] private float dashDampSpeed;   //대쉬감속
    [SerializeField] private float hitMotion_Time;
    [SerializeField] private Color hitMotion_Color;
    [SerializeField] public Weapon weapon;
    [SerializeField] private GameObject afterImage;     //대쉬 시 출력할 잔상이미지들
    [SerializeField] private GameObject miniMapIcon;

    private Rigidbody2D myRigidbody;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Vector2 dashDirection;
    private Vector2 mouseVec;
    private Vector2 playerToMouse_Direction; 
    private Vector2 startDash_playerToMouse_Direction;  //대쉬를 시작했을 때의 플레이어와 마우스사이의 방향
    private float hpIcon_value;      //hp아이콘 한개가 가질 hp값
    private float tempDashSpeed;    //실질적으로 대쉬속도 연산을 적용할 변수
    private float timer = 0.0f;    //일정시간후에 실행할 명령어를 위한 타이머
    private bool isDash = false;
    private bool isDampDash = false;    //대쉬 시, 감속을 하였는지 체크

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);  //스테이지를 넘어가도 파괴되지 않게하기 위함
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

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        //전체 hp / hp아이콘의 갯수 = 아이콘 한개가 의미하는 hp값
        hpIcon_value = hp / UIManager.instance.playerHP_Icon.Length;

        miniMapIcon.SetActive(true);    //미니맵 아이콘 활성화
    }

    void Update()
    {
        //마우스 좌표 구하기
        mouseVec = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //마우스 좌표와 플레이어 좌표의 방향
        playerToMouse_Direction = mouseVec - (Vector2)transform.position;

        //플레이어 사망시 처리
        if (hp <= 0.0f)
            Destroy(gameObject);

        //플레이어 상태에 따라서 그 상태에서만 처리해야할 것들
        switch (state)
        {
            case State.Stop:    //아무것도 하지않음
                break;
            case State.Idle:
                Render();   //플레이어의 시선에 따른 모션출력
                weapon.Render(playerToMouse_Direction);    //플레이어 위치기준으로 마우스위치에 따른 무기의 회전값 조절

                //---------- 키 입력시 발생 ---------- 
                weapon.Fire();
                if (Input.GetKey(KeyCode.R) || weapon.currentBulletCount <= 0)
                    state = State.Reloading;
                PressDashKey();    //대쉬키를 눌렀을 경우, 대쉬하기 전에 처리할 것들

                //방향키입력시 걷는 모션 실행
                animator.SetBool("Walk", Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W));
                break;
            case State.Reloading:
                weapon.Reload();    //총알 장전후에 Idle상태로 돌아감

                //장전중에도 플레이어의 모션을 출력해야함
                Render();   //플레이어의 시선에 따른 모션출력
                animator.SetBool("Walk", Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W));
                weapon.Render(playerToMouse_Direction);    //무기의 출력관련//방향키입력시 걷는 모션 실행
                break;
            case State.Dash:
                //플레이어에게 적의 총알이 충돌되지 않는 무적부여
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyBullet"), true);
                afterImage.SetActive(true);     //잔상효과 켜주기

                //속도를 지속적으로 감속시키기
                if (!isDampDash)    
                {
                    isDampDash = true;
                    StartCoroutine(DampDashSpeed());
                }

                DashAnimation();    //대쉬 애니메이션 출력

                //대쉬에 걸리는 시간이 지나면 Idle상태로 돌아가기 위해 처리해야할 것들
                timer += Time.deltaTime;    //타이머 시작
                if (timer >= dashTime)    //타이머가 dashTime이 되면 대쉬 종료
                {
                    timer = 0.0f;
                    EndDash();  //대쉬가 끝나면 실행해야할 것들
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Stop:    //아무것도 하지않음
                myRigidbody.velocity = Vector2.zero;
                break;
            case State.Idle: 
                //플레이어 이동
                myRigidbody.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * walkSpeed;  
                break;
            case State.Reloading:
                //장전중에도 플레이어 이동가능
                myRigidbody.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * walkSpeed;
                break;
            case State.Dash:
                //대쉬키를 눌렀을 때의 방향과 대쉬속도 적용
                myRigidbody.velocity = dashDirection * tempDashSpeed;   
                break;
        }
    }

    void Render()
    {
        //스케일값을 조정함으로써 플레이어의 시선을 플레이어 위치를 기준으로 마우스 위치에 따라 변경해준다.
        if (playerToMouse_Direction.normalized.x < 0.0f)     //플레이어기준 커서가 왼쪽에 위치할경우 
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else     //플레이어기준 커서가 오른쪽에 위치할경우
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        //플레이어가 가만히 있을 때, 위를 바라보거나 통상적으로 앞을 바라보았을때, 처리해야 할것들
        if ((-0.7f <= playerToMouse_Direction.normalized.x && playerToMouse_Direction.normalized.x <= 0.7f)
            && (0.7f <= playerToMouse_Direction.normalized.y && playerToMouse_Direction.normalized.y <= 1.0f))    //위쪽(부채꼴 범위)을 바라보았을 경우
            animator.SetBool("LookAtUp", true);
        else
            animator.SetBool("LookAtUp", false);

        //플레이어가 움직일때, 위를바라보거나 통상적으로 앞을 바라보는 애니메이션 블렌드트리
        animator.SetFloat("PosX", playerToMouse_Direction.normalized.x);
        animator.SetFloat("PosY", playerToMouse_Direction.normalized.y);
    }

    void PressDashKey()
    {
        if (Input.GetKeyDown(KeyCode.Space))    //대쉬키를 눌렀을 경우
        {
            dashDirection = myRigidbody.velocity.normalized;    //눌렀을 때의 방향값을 가져옴
            if (dashDirection.magnitude < 1)    //방향의 크기가 1미만일경우(플레이어가 벽에 밀착해 있을경우) 실행하지 않음
                return;

            //대쉬키를 눌렀을 때의 플레이어와 마우스 사이의 방향값 저장
            startDash_playerToMouse_Direction.x = playerToMouse_Direction.normalized.x;
            startDash_playerToMouse_Direction.y = playerToMouse_Direction.normalized.y;
            tempDashSpeed = dashSpeed;  /*원위치 (EndDash()에서 처리하면 EndDash후에도 실행되었던 
                                        DampDashSpeed()코루틴에 의해 변수값이 변경되므로 대쉬하기전에 처리)*/
            state = State.Dash;
        }
    }

    IEnumerator DampDashSpeed()
    {
        yield return new WaitForSeconds(0.1f);
        tempDashSpeed *= dashDampSpeed;     //0.1f초마다 1미만의 소숫점을 곱하여 스피드를 감속시킨다
        isDampDash = false;
    }

    void DashAnimation()
    {
        //한번만 실행할 것들
        if (isDash == false)
        {
            isDash = true;
            //위를 바라보는 모션과, 평상시 모션 결정
            animator.SetFloat("DashDirectionY", dashDirection.y);
            //대시 했을 때, 마우스의 위치가 플레이어기준 왼쪽과 위쪽 대각선, 오른쪽과 위쪽 대각선 사이였을 경우(즉, 플레이어가 위를 바라보았을 경우)
            if ((-0.7f <= playerToMouse_Direction.normalized.x && playerToMouse_Direction.normalized.x <= 0.7f)
                && (0.7f <= playerToMouse_Direction.normalized.y && playerToMouse_Direction.normalized.y <= 1.0f))
                animator.SetTrigger("LookAtUp_Dash");
            else
                animator.SetTrigger("Idle_Dash");
        }

        //위 아니면 평상 모션 중 상하좌우 애니메이션이 담긴 블렌드트리에 값 전달
        if (dashDirection.y == 1.0f || dashDirection.y == -1.0f)
            animator.SetFloat("DashDirectionX", 0.0f);  //위 아래로 대쉬할때는 블렌드트리의 x값을 0으로
        else
            //대쉬방향과 플레이어와마우스사이의 방향이 값다면 1(앞대쉬), 아니라면 -1(백대쉬)
            animator.SetFloat("DashDirectionX", dashDirection.x * playerToMouse_Direction.x > 0.0f ? 1.0f : -1.0f);
    }

    public void EndDash()
    {
        //대쉬 후, 마우스의 위치가 플레이어기준 왼쪽과 위쪽 대각선, 오른쪽과 위쪽 대각선 사이였을 경우(즉, 플레이어가 위를 바라보았을 경우)
        if ((-0.7f <= playerToMouse_Direction.normalized.x && playerToMouse_Direction.normalized.x <= 0.7f)
            && (0.7f <= playerToMouse_Direction.normalized.y && playerToMouse_Direction.normalized.y <= 1.0f))
            animator.SetTrigger("LookAtUp_Trigger"); //위쪽을 바라보는 모션으로 돌아간다
        else
            animator.SetTrigger("Idle_Trigger"); //그 외에는 통상모션

        afterImage.SetActive(false); //잔상효과 꺼주기
        isDash = false; //원위치
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyBullet"), false); //무적해제
        state = State.Idle;     //Move로 돌아가기
    }

    public IEnumerator HitMotion()
    {
        spriteRenderer.color = hitMotion_Color;     //플레이어의 색 바꿔주기
        yield return new WaitForSeconds(hitMotion_Time);    //바뀐색을 hitMotion_Time만큼 유지
        spriteRenderer.color = new Color(255, 255, 255, 255);   //다시 원래대로
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //총알 피격시 총알의 데미지만큼 hp값이 줄어듬
        if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyBullet"))   
        {
            hp = hp - collision.transform.GetComponent<Bullet>().damage;
            UIManager.instance.PlayerHpUI_Update(hpIcon_value, hp);   //HP아이콘 UI갱신
            StartCoroutine(HitMotion());    //피격 모션
        }
    }
}