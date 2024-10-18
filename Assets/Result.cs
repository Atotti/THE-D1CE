using UnityEngine;
using UnityEngine.SceneManagement;

public class Result : MonoBehaviour
{
    // ゲームを開始するメソッド
    public void StartGame()
    {
        // "MainGame" という名前のシーンをロード
        SceneManager.LoadScene("MainGame");
    }

    public void BackMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
