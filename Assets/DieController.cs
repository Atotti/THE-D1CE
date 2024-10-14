using UnityEngine;

public class DieController : MonoBehaviour
{
    public float rollDuration = 0.5f; // サイコロが転がるのにかかる時間
    private bool isRolling = false;   // 転がっている間は操作を受け付けない

    // 初期化処理
    void Start()
    {
        // SetInitialPosition(5, 5); // 仮で5, 5にサイコロを設置
    }

    void Update()
    {
        // 矢印キー入力に応じてサイコロを転がす
        if (!isRolling)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                StartCoroutine(Roll(Vector3.forward));
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                StartCoroutine(Roll(Vector3.back));
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                StartCoroutine(Roll(Vector3.left));
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                StartCoroutine(Roll(Vector3.right));
            }
        }
    }

    // 初期位置を設定するメソッド
    private void SetInitialPosition(int gridX, int gridY)
    {
        float cellSize = 1.0f; // グリッドのセルサイズが1の場合

        // グリッドのX, Yに基づいて位置を計算
        Vector3 initialPosition = new Vector3(gridX * cellSize, 0.5f, gridY * cellSize);
        transform.position = initialPosition;
    }

    private System.Collections.IEnumerator Roll(Vector3 direction)
    {
        isRolling = true;

        // サイコロの転がり軸を計算
        Vector3 rollAxis = Vector3.Cross(Vector3.up, direction).normalized;
        float angle = 90.0f; // 90度回転

        // 回転アニメーション
        float elapsedTime = 0;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.AngleAxis(angle, rollAxis) * startRotation;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + direction;

        while (elapsedTime < rollDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / rollDuration);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        transform.rotation = endRotation;
        transform.position = endPosition;

        isRolling = false;
    }
}
