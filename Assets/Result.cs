using UnityEngine;
using UnityEngine.SceneManagement;

public class Result : MonoBehaviour
{
    // ゲームを開始するメソッド
    public void StartGame()
    {
        // ゲームから再開
        SceneManager.LoadScene("MainGame");
    }

    public void BackMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
