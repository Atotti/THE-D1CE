using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    // ゲームを開始するメソッド
    public void StartGame()
    {
        // "MainGame" という名前のシーンをロード
        SceneManager.LoadScene("MainGame");
    }

    public void ViewRule()
    {
        SceneManager.LoadScene("ViewRule");
    }

    // ゲームを終了するメソッド（エディターやビルド時用）
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
