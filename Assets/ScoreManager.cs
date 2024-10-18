using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance; // シングルトンのインスタンス
    public float score = 0; // スコアの変数

    private void Awake()
    {
        // インスタンスが既に存在する場合は破棄し、新たに作らないようにする
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでオブジェクトを破棄しないようにする
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // スコアを更新するメソッド
    public void UpdateScore(float value)
    {
        score += value;
    }

    // スコアをリセットするメソッド（必要に応じて使用）
    public void ResetScore()
    {
        score = 0;
    }
}
