/* 
 * @file : MainMenu.cs 
 * @date : 2021.06.18
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 메인메뉴 씬의 버튼 기능
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;      

    public void Start()
    {
        //메인메뉴 시작시 옵션의 볼륨슬라이더 조정
        volumeSlider.value = GameManager.instance.systemVolume;     
    }

    public void PlayGame()  //게임플레이 버튼
    {
        SceneManager.LoadScene(1);  //첫번째 스테이지 불러오기
    }

    public void QuitGame()  //게임종료 버튼
    {
        Application.Quit(); //종료
    }

    public void VolumeControl()   //메인메뉴에서 사운드조절시 발생
    {
        GameManager.instance.systemVolume = volumeSlider.value;     //시스템의 볼륨적용
    }
}