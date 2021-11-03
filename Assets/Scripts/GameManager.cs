/* 
 * @file : GameManager.cs 
 * @date : 2021.06.13
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 커서텍스쳐 변경 및 시스템 사운드 볼륨 저장
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [SerializeField] public Texture2D cursorTexture;

    public float systemVolume = 1.0f;

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
        AudioListener.volume = systemVolume;    //저장해 놓았던 시스템 볼륨 설정
        //특정 씬에서는 해당 오브젝트 파괴
        if (SceneManager.GetActiveScene().name == "GameClear")
            Destroy(this.gameObject);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        //커서 텍스쳐
        Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width / 2, cursorTexture.height / 2), CursorMode.ForceSoftware); 
          
    }

    public void VolumeControl(float volume)
    {
        systemVolume = volume;
        AudioListener.volume = systemVolume;
    }
}