/* 
 * @file : UIManager.cs 
 * @date : 2021.06.09
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 게임UI 관리자
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] public Image[] playerHP_Icon;      //플레이어HP아이콘
    [SerializeField] private GameObject menu;          //메뉴
    [SerializeField] private GameObject optionsMenu;     //옵션메뉴
    [SerializeField] private Slider volumeSlider;        //옵션메뉴안의 볼륨 슬라이더
    [SerializeField] private GameObject gameOver;      //게임오버
    [SerializeField] private TextMeshProUGUI magazineUI;    //탄창

    public static UIManager instance;
    private Player player;
    private float hpRatio;
    private bool isPlayerStop = false;

    private void Awake()
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

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        volumeSlider.value = GameManager.instance.systemVolume;
    }

    void Update()
    {
        OnOffMenu();    //메뉴 온오프기능

        //매뉴나 옵션매뉴 실행시, 모든 오브젝트를 멈추게 하는 기능
        //timeScale을 0으로 해도 Player의 움직임 모션은 출력되기 때문에 상태를 Stop으로 변경해줌
        if (menu.activeSelf || optionsMenu.activeSelf)
        {
            Time.timeScale = 0.0f;
            if (!isPlayerStop)
            {
                isPlayerStop = true;
                if(player.state == Player.State.Idle)
                    player.state = Player.State.Stop;
            }
        }
        else if(!menu.activeSelf && !optionsMenu.activeSelf)
        {
            Time.timeScale = 1.0f;
            if (isPlayerStop)
            { 
                isPlayerStop = false;
                if(player.state == Player.State.Stop)
                    player.state = Player.State.Idle;
            }
        }

        if(player.hp <= 0)
        {
            gameOver.SetActive(true);
            this.enabled = false;
        }
    }

    //탄창(남은 총알의 갯수 / 모든 총알의 갯수)UI 갱신
    public void MagazineUI_Update(int currentBulletCount, int bulletCount)
    {
        magazineUI.text = currentBulletCount + " / " + bulletCount;
    }

    //HpUI_Update(float 아이콘하나가 가지는 hp값, float 플레이어의 현재hp값)
    public void PlayerHpUI_Update(float hpIcon_value, float current_playerHp)
    {
        //hp아이콘이 표기될 비율
        //ex) hpRatio == 3.5라면 아이콘이 3개하고 절반만큼 표기됨
        hpRatio = current_playerHp / hpIcon_value;

        //아이콘의 fillAmout를 hpRatio에 따라서 왼쪽부터 채워준다
        for(int i = 0; i < playerHP_Icon.Length; i++)
        {
            if(hpRatio >= 1.0f)    
            {
                playerHP_Icon[i].fillAmount = 1.0f;
                hpRatio--;
            }
            else if(1.0f > hpRatio && hpRatio > 0.0f)
            {
                playerHP_Icon[i].fillAmount = hpRatio;
                hpRatio -= hpRatio;
            }
            else if(hpRatio <= 0.0f)
            {
                playerHP_Icon[i].fillAmount = 0.0f;
            }
        }
    }

    public void VolumeControl()     
    {
        GameManager.instance.VolumeControl(volumeSlider.value);
    }

    void OnOffMenu()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsMenu.activeSelf) //옵션메뉴가 활성화 되어있다면 실행하지않음
                return;

            if(!menu.activeSelf)    //메뉴가 비활성화 되있을 경우 메뉴를 활성화시킨다
                menu.SetActive(true);
            else     //메뉴가 활성화 되있을경우에는 메뉴를 비활성화 시킨다
                menu.SetActive(false);
        }
    }

    public void MainMenuButton()    //메인메뉴버튼
    {
        SceneManager.LoadScene("MainMenu");
    }
}