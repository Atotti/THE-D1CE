using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    // ゲームを開始するメソッド
    public void StartGame()
    {
        // "RuleAndName" という名前のシーンをロード
        SceneManager.LoadScene("RuleAndName");
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
