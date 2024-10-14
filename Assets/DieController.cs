using UnityEngine;

public class DieController : MonoBehaviour
{
    public float rollDuration = 0.5f; // サイコロが転がるのにかかる時間
    private bool isRolling = false;   // 転がっている間は操作を受け付けない
    public GameObject character; // キャラクターの参照

    // サイコロを転がすための公開メソッド
    public void RollDie(Vector3 direction, System.Action onComplete)
    {
        if (!isRolling)
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

        isRolling = false;
        onComplete?.Invoke();
    }
}
