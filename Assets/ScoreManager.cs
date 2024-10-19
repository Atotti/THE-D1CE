using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance; // シングルトンのインスタンス
    public float score = 0; // スコアの変数
    public string name = "";

    private string endpoint = "https://xi-server.ayutaso.com/result/";

    [System.Serializable]
    public class ResultData
    {
        public string name;
        public int score;
    }

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

    public void SetName(string player)
    {
        name = player;
        Debug.Log($"1 {ScoreManager.instance.name}");
        Debug.Log($"2 {name}");

    }

    // スコアを更新するメソッド
    public void UpdateScore(float value)
    {
        score = value;
        // 名前と一緒にサーバーに送信
        ResultData data = new ResultData();
        data.name = ScoreManager.instance.name;
        data.score = Mathf.RoundToInt(ScoreManager.instance.score);
        Debug.Log($"UpdateScore {data.name} {data.score}");

        // コルーチンでPOSTリクエストを送信
        StartCoroutine(PostRequest(endpoint, data));
    }

    // スコアをリセットするメソッド（必要に応じて使用）
    public void ResetScore()
    {
        score = 0;
    }

    IEnumerator PostRequest(string url, ResultData resultData)
    {
        // JSONにシリアライズ
        string jsonData = JsonUtility.ToJson(resultData);
        Debug.Log("Serialized JSON: " + jsonData);

        // JSONデータをバイト配列に変換
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // UnityWebRequestのセットアップ
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // リクエストの送信
        yield return request.SendWebRequest();

        // レスポンスの処理
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Request successful: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Request failed: " + request.error);
        }
    }
}
