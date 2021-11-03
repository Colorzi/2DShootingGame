/* 
 * @file : GameClear.cs 
 * @date : 2021.06.19
 * @author : 조성우(whtjddn2495@gmail.com)
 * @brief : 게임클리어 씬의 메인메뉴 버튼기능
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameClear : MonoBehaviour
{
    public void MainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
