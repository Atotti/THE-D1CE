using UnityEngine;
using System.Collections.Generic;

public class GridSystem : MonoBehaviour
{
    public int gridSizeX = 7;
    public int gridSizeY = 7;
    public float cellSize = 1.0f;

    public GameObject diePrefab;       // サイコロのPrefab（Unityエディタで設定）
    public GameObject characterPrefab; // キャラクターのPrefab

    private List<Vector2Int> usedPositions = new List<Vector2Int>(); // 使用済みの位置を記録するリスト
    public List<GameObject> diceList = new List<GameObject>(); // 生成されたサイコロを保持

    void Start()
    {
        CreateGrid();
        PlaceRandomDice(16);
        PlaceCharacterOnRandomDie();
    }

    void Update()
    {
        CheckMatchesAndRemove(); // 消える判定の呼び出し
    }

    void CreateGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // 各マスの中心位置を計算
                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);

                // 床を作成（Cubeで表現） TODO: 床のデザインも作る
                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cell.transform.position = cellPosition;
                cell.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                cell.GetComponent<Renderer>().material.color = Color.gray;

                // 境界線を描画
                DrawCellBorders(cellPosition);
            }
        }
    }

    void DrawCellBorders(Vector3 position)
    {
        // マス目の四隅の座標を計算
        Vector3 topLeft = position + new Vector3(-cellSize / 2, 0.05f, cellSize / 2);
        Vector3 topRight = position + new Vector3(cellSize / 2, 0.05f, cellSize / 2);
        Vector3 bottomRight = position + new Vector3(cellSize / 2, 0.05f, -cellSize / 2);
        Vector3 bottomLeft = position + new Vector3(-cellSize / 2, 0.05f, -cellSize / 2);

        // 境界線のオブジェクトを作成
        GameObject lineObj = new GameObject("GridLine");
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

        // LineRendererの設定
        lineRenderer.positionCount = 5; // 四隅 + 最初の点に戻る
        lineRenderer.SetPosition(0, topLeft);
        lineRenderer.SetPosition(1, topRight);
        lineRenderer.SetPosition(2, bottomRight);
        lineRenderer.SetPosition(3, bottomLeft);
        lineRenderer.SetPosition(4, topLeft);

        lineRenderer.startWidth = 0.05f; // 線の太さ
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // 単純な色のためのシェーダー
        lineRenderer.startColor = Color.black; // 線の色
        lineRenderer.endColor = Color.black;
    }

    void PlaceRandomDice(int numberOfDice)
    {
        for (int i = 0; i < numberOfDice; i++)
        {
            Vector2Int randomPosition;

            // 重複しない位置が見つかるまでランダムな座標を選ぶ
            do
            {
                int x = Random.Range(0, gridSizeX);
                int y = Random.Range(0, gridSizeY);
                randomPosition = new Vector2Int(x, y);
            } while (usedPositions.Contains(randomPosition));

            usedPositions.Add(randomPosition);

            // サイコロの位置を計算
            Vector3 diePosition = new Vector3(randomPosition.x * cellSize, 0.5f, randomPosition.y * cellSize);

            // サイコロを生成
            GameObject newDie = Instantiate(diePrefab, diePosition, Quaternion.identity);

            Debug.Log($"サイコロ {i + 1} を配置しました: 位置 {randomPosition}");

            diceList.Add(newDie); // リストにサイコロを追加
        }
    }

    void PlaceCharacterOnRandomDie()
    {
        // サイコロリストからランダムに1つを選ぶ
        if (diceList.Count > 0)
        {
            int randomIndex = Random.Range(0, diceList.Count);
            GameObject selectedDie = diceList[randomIndex];

            // サイコロの上にキャラクターの位置を設定
            float characterHeightOffset = selectedDie.transform.localScale.y / 2 + 1.0f; // サイコロの高さの半分 + キャラクターの高さの半分
            Vector3 characterPosition = selectedDie.transform.position + new Vector3(0, characterHeightOffset, 0);

            // キャラクターを生成
            GameObject character = Instantiate(characterPrefab, characterPosition, Quaternion.identity);

            // キャラクターに現在乗っているサイコロを設定
            CharacterController characterController = character.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.currentDie = selectedDie;
                characterController.gridSystem = this; // GridSystemを参照
            }
        }
    }

    // サイコロが指定位置に存在するか確認するメソッド
    public bool IsPositionOccupied(Vector2Int position)
    {
        return usedPositions.Contains(position);
    }

    // サイコロの位置を更新するメソッド
    public void UpdateDiePosition(GameObject die, Vector2Int newPosition)
    {
        Vector2Int oldPosition = new Vector2Int(
            Mathf.RoundToInt(die.transform.position.x / cellSize),
            Mathf.RoundToInt(die.transform.position.z / cellSize)
        );

        usedPositions.Remove(oldPosition);
        usedPositions.Add(newPosition);
    }

    void CheckMatchesAndRemove()
    {
        // 既に削除予定になっているサイコロを追跡するためのリスト
        HashSet<GameObject> toBeRemoved = new HashSet<GameObject>();

        foreach (GameObject die in diceList)
        {
            if (toBeRemoved.Contains(die)) continue; // 既に削除予定ならスキップ

            DieController dieController = die.GetComponent<DieController>();
            if (dieController == null || dieController.IsRolling()) continue; // サイコロが回転中ならスキップ

            Vector2Int currentPosition = new Vector2Int(
                Mathf.RoundToInt(die.transform.position.x / cellSize),
                Mathf.RoundToInt(die.transform.position.z / cellSize)
            );

            int dieNumber = dieController.GetDieNumber(); // サイコロの目を取得
            int matchCount = (dieNumber >= 2) ? dieNumber : 2; // 1の場合は2つに設定

            // 消える条件を満たした場合、隣接する同じ目のサイコロを削除リストに追加
            if (CheckMatches(currentPosition, dieNumber, matchCount))
            {
                RemoveConnectedDice(currentPosition, dieNumber, toBeRemoved);
            }
        }

        // 削除予定のサイコロを順番に削除
        foreach (GameObject die in toBeRemoved)
        {
            StartCoroutine(RemoveDieAnimation(die));
        }
    }

    void RemoveConnectedDice(Vector2Int startPosition, int dieNumber, HashSet<GameObject> toBeRemoved)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(startPosition);

        while (stack.Count > 0)
        {
            Vector2Int position = stack.Pop();

            // この位置にサイコロがあり、まだ削除予定に入っていない場合
            if (usedPositions.Contains(position) && GetDieNumberAtPosition(position) == dieNumber)
            {
                GameObject die = GetDieAtPosition(position);
                if (die != null && !toBeRemoved.Contains(die))
                {
                    toBeRemoved.Add(die);

                    // 上下左右に対して再帰的に探索
                    stack.Push(new Vector2Int(position.x + 1, position.y));
                    stack.Push(new Vector2Int(position.x - 1, position.y));
                    stack.Push(new Vector2Int(position.x, position.y + 1));
                    stack.Push(new Vector2Int(position.x, position.y - 1));
                }
            }
        }
    }

    bool CheckMatches(Vector2Int position, int dieNumber, int matchCount)
    {
        int horizontalCount = 1;
        int verticalCount = 1;

        // 左右の隣接チェック
        for (int i=1; i < matchCount; i++)
        {
            Vector2Int leftPosition = new Vector2Int(position.x - i, position.y);
            Vector2Int rightPosition = new Vector2Int(position.x + i, position.y);

            if (usedPositions.Contains(leftPosition) && GetDieNumberAtPosition(leftPosition) == dieNumber)
            {
                horizontalCount++;
            }
            if (usedPositions.Contains(rightPosition) && GetDieNumberAtPosition(rightPosition) == dieNumber)
            {
                horizontalCount++;
            }
        }

        // 上下の隣接チェック
        for (int i = 1; i < matchCount; i++)
        {
            Vector2Int upPosition = new Vector2Int(position.x, position.y + i);
            Vector2Int downPosition = new Vector2Int(position.x, position.y - i);

            if (usedPositions.Contains(upPosition) && GetDieNumberAtPosition(upPosition) == dieNumber)
            {
                verticalCount++;
            }
            if (usedPositions.Contains(downPosition) && GetDieNumberAtPosition(downPosition) == dieNumber)
            {
                verticalCount++;
            }
        }

        return (horizontalCount >= matchCount || verticalCount >= matchCount);
    }

    int GetDieNumberAtPosition(Vector2Int position)
    {
        foreach (GameObject die in diceList)
        {
            Vector2Int diePosition = new Vector2Int(
                Mathf.RoundToInt(die.transform.position.x / cellSize),
                Mathf.RoundToInt(die.transform.position.z / cellSize)
            );

            if (diePosition == position)
            {
                DieController dieController = die.GetComponent<DieController>();
                return dieController.GetDieNumber(); // サイコロの目の値を返す
            }
        }
        return -1;
    }

    private System.Collections.IEnumerator WaitForFrames(int frameCount)
    {
        for (int i = 0; i < frameCount; i++)
        {
            yield return null;
        }
    }

    private System.Collections.IEnumerator RemoveDieAnimation(GameObject die)
    {
        Renderer renderer = die.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red; // 色を赤に変更
        }

        // ここで5フレーム待機
    yield return StartCoroutine(WaitForFrames(30));

        float elapsedTime = 0;
        float sinkDuration = 10.0f; // 沈むのにかかる時間
        Vector3 startPosition = die.transform.position;
        Vector3 endPosition = startPosition + new Vector3(0, -1.0f, 0); // 徐々に下に沈む

        while (elapsedTime < sinkDuration)
        {
            elapsedTime += Time.deltaTime;
            die.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / sinkDuration);
            yield return null;
        }

        diceList.Remove(die);
        usedPositions.Remove(new Vector2Int(
            Mathf.RoundToInt(die.transform.position.x / cellSize),
            Mathf.RoundToInt(die.transform.position.z / cellSize)
        ));
        Destroy(die);
    }

    GameObject GetDieAtPosition(Vector2Int position)
    {
        foreach (GameObject die in diceList)
        {
            Vector2Int diePosition = new Vector2Int(
                Mathf.RoundToInt(die.transform.position.x / cellSize),
                Mathf.RoundToInt(die.transform.position.z / cellSize)
            );

            if (diePosition == position)
            {
                return die;
            }
        }
        return null;
    }
}
