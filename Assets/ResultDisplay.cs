using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultDisplay : MonoBehaviour
{
    public TMP_Text scoreText; // スコアを表示するテキスト（Unityエディタで設定）

    void Start()
    {
        // スコアを表示
        if (ScoreManager.instance != null)
        {
            scoreText.text = "Score: " + ScoreManager.instance.score.ToString();
        }
    }
}
