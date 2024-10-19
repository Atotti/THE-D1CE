using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RuleAndNameMenu : MonoBehaviour
{
    public TMP_InputField inputField;

    // ゲームを開始するメソッド
    public void StartGame()
    {
        // ゲームシーンに移動
        SceneManager.LoadScene("MainGame");
    }

    public void SetPlayerName()
    {
        if (inputField == null)
        {
            Debug.LogError("InputField is not assigned!");
            return;
        }

        if (ScoreManager.instance == null)
        {
            Debug.LogError("ScoreManager instance is null!");
            return;
        }
        string inputText = inputField.text;
        Debug.Log("Name: " + inputText);

        // 名前を一時保存
        ScoreManager.instance.SetName(inputText);
    }

}
