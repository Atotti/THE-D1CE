using UnityEngine;

public class DieController : MonoBehaviour
{
    public float rollDuration = 0.1f; // サイコロが転がるのにかかる時間
    private bool isRolling = false;   // 転がっている間は操作を受け付けない
    public GameObject character; // キャラクターの参照
    public bool isRemoving = false; // サイコロが消える途中かどうか

    // サイコロの各面の数値を保持
    private int[] faceValues = new int[6]; // 0: 上, 1: 下, 2: 前, 3: 後ろ, 4: 左, 5: 右

    void Start()
    {
        // 初期状態を設定（上が2の目とする）
        faceValues[0] = 2; // 上
        faceValues[1] = 5; // 下
        faceValues[2] = 4; // 前
        faceValues[3] = 3; // 後ろ
        faceValues[4] = 1; // 左
        faceValues[5] = 6; // 右

        // サイコロをランダムに回転させる
        RandomlyRotateDie();
    }

    // サイコロをランダムに回転させるメソッド
    private void RandomlyRotateDie()
    {
        // ランダムに回転する回数を設定
        int randomRolls = Random.Range(1, 4); // 1〜3回転が行われる
        for (int i = 0; i < randomRolls; i++)
        {
            // ランダムな方向に転がす
            Vector3 randomDirection = GetRandomDirection();
            PerformRotation(randomDirection);
        }
    }

    // ランダムな方向を取得するメソッド
    private Vector3 GetRandomDirection()
    {
        int randomIndex = Random.Range(0, 4);
        switch (randomIndex)
        {
            case 0: return Vector3.forward;
            case 1: return Vector3.back;
            case 2: return Vector3.left;
            case 3: return Vector3.right;
            default: return Vector3.forward;
        }
    }

    // サイコロを指定された方向に転がすメソッド
    private void PerformRotation(Vector3 direction)
    {
        // サイコロの面の値を更新
        UpdateFaceValues(direction);

        // 回転処理
        Vector3 rollAxis = Vector3.Cross(Vector3.up, direction).normalized;
        float angle = 90.0f;
        transform.RotateAround(transform.position, rollAxis, angle);
    }


    public int GetDieNumber()
    {
        return faceValues[0]; // 上面の値を返す;
    }

    public bool IsRolling()
    {
        return isRolling;
    }

    // サイコロを転がすための公開メソッド
    public void RollDie(Vector3 direction, System.Action onComplete)
    {
        if (!isRolling && !isRemoving)
        {
            StartCoroutine(Roll(direction, onComplete));
        }
    }

    // 転がりアニメーションを行うためのメソッド
    private System.Collections.IEnumerator Roll(Vector3 direction, System.Action onComplete)
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

            // キャラクターの位置を更新
            if (character != null)
            {
                float characterHeightOffset = transform.localScale.y / 2 + 1.0f;
                character.transform.position = transform.position + new Vector3(0, characterHeightOffset, 0);
            }
            yield return null;
        }

        transform.rotation = endRotation;
        transform.position = endPosition;

        // サイコロの面の値を更新
        UpdateFaceValues(direction);

        isRolling = false;
        onComplete?.Invoke();
    }

    // 転がり方向に応じて面の値を更新するメソッド
    private void UpdateFaceValues(Vector3 direction)
    {
        int top = faceValues[0];
        int bottom = faceValues[1];
        int front = faceValues[2];
        int back = faceValues[3];
        int left = faceValues[4];
        int right = faceValues[5];

        if (direction == Vector3.forward) // 前に転がる
        {
            faceValues[0] = back;  // 上 -> 後ろ
            faceValues[1] = front; // 下 -> 前
            faceValues[2] = top;   // 前 -> 上
            faceValues[3] = bottom; // 後ろ -> 下
        }
        else if (direction == Vector3.back) // 後ろに転がる
        {
            faceValues[0] = front;  // 上 -> 前
            faceValues[1] = back;   // 下 -> 後ろ
            faceValues[2] = bottom; // 前 -> 下
            faceValues[3] = top;    // 後ろ -> 上
        }
        else if (direction == Vector3.left) // 左に転がる
        {
            faceValues[0] = right;  // 上 -> 右
            faceValues[1] = left;   // 下 -> 左
            faceValues[4] = top;    // 左 -> 上
            faceValues[5] = bottom; // 右 -> 下
        }
        else if (direction == Vector3.right) // 右に転がる
        {
            faceValues[0] = left;   // 上 -> 左
            faceValues[1] = right;  // 下 -> 右
            faceValues[4] = bottom; // 左 -> 下
            faceValues[5] = top;    // 右 -> 上
        }
    }
}
