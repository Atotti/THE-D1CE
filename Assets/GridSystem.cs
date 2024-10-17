using UnityEngine;
using System.Collections.Generic;

public class GridSystem : MonoBehaviour
{
    public int gridSizeX = 7;
    public int gridSizeY = 7;
    public float cellSize = 1.0f;

    public GameObject diePrefab;       // サイコロのPrefab（Unityエディタで設定）
    public GameObject characterPrefab; // キャラクターのPrefab

    public AudioSource audioSourceSpawn; // AudioSourceの参照
    public AudioSource audioSourceRemove;

    private Dictionary<Vector2Int, GameObject> diePositions = new Dictionary<Vector2Int, GameObject>(); // 使用済みの位置を記録する辞書

    void Start()
    {
        CreateGrid();
        PlaceRandomDice(16, true);
        PlaceCharacterOnRandomDie();

        // 一定時間ごとにサイコロを生成する
        InvokeRepeating("PlaceRandomDiceWrapper", 0f, 5f); // 5秒ごとに実行
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

    void PlaceRandomDice(int numberOfDice, bool init)
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
            } while (diePositions.ContainsKey(randomPosition));

            // サイコロの位置を計算
            Vector3 diePosition = new Vector3(randomPosition.x * cellSize, 0.5f, randomPosition.y * cellSize);

            // サイコロを生成
            GameObject newDie = Instantiate(diePrefab, diePosition, Quaternion.identity);

            Debug.Log($"サイコロ {i + 1} を配置しました: 位置 {randomPosition}");

            diePositions.Add(randomPosition, newDie); // 位置とサイコロをマッピング

            if (!init)
            {
                // アニメーションを開始
                StartCoroutine(SpawnDiceAnimation(newDie));
            }
        }
    }

    private System.Collections.IEnumerator SpawnDiceAnimation(GameObject die)
    {
        float duration = 5.0f; // アニメーションの長さ
        float elapsed = 0f;

        Renderer renderer = die.GetComponent<Renderer>();
        DieController dieController = die.GetComponent<DieController>();

        // 効果音再生
        if (audioSourceSpawn != null)
        {
            audioSourceSpawn.Play();
        }

        if (dieController != null)
        {
            dieController.isSpawning = true; // アニメーション開始時にフラグを設定
        }

        if (renderer != null)
        {
            renderer.material.color = Color.blue; // 色を青に変更
        }

        Vector3 startPosition = die.transform.position - Vector3.up * 1f; // 1ユニット下から開始
        Vector3 endPosition = die.transform.position;

        while (elapsed < duration)
        {
            die.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        die.transform.position = endPosition; // 最終位置を設定
        if (renderer != null)
        {
            renderer.material.color = Color.white; // 色を白に変更
        }

        if (dieController != null)
        {
            dieController.isSpawning = false; // アニメーション終了時にフラグを更新
        }
    }

    // 後から生えてくるサイコロを生成
    void PlaceRandomDiceWrapper() {
        PlaceRandomDice(1, false);
    }

    public Vector2Int GetGridPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / cellSize);
        int y = Mathf.RoundToInt(position.z / cellSize);
        return new Vector2Int(x, y);
    }

    void PlaceCharacterOnRandomDie()
    {
        // サイコロの位置リストからランダムに1つを選ぶ
        if (diePositions.Count > 0)
        {
            List<GameObject> diceListTmp = new List<GameObject>(diePositions.Values);
            int randomIndex = Random.Range(0, diceListTmp.Count);
            GameObject selectedDie = diceListTmp[randomIndex];

            // サイコロの上にキャラクターの位置を設定
            float characterHeightOffset = selectedDie.transform.localScale.y / 2 + 1.0f; // サイコロの高さの半分 + キャラクターの高さの半分
            Vector3 characterPosition = selectedDie.transform.position + new Vector3(0, characterHeightOffset, 0);

            // キャラクターを生成
            GameObject character = Instantiate(characterPrefab, characterPosition, Quaternion.identity);

            // キャラクターに現在乗っているサイコロを設定
            GhostCharacterController characterController = character.GetComponent<GhostCharacterController>();
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
        return diePositions.ContainsKey(position);
    }

    // サイコロの位置を更新するメソッド
    public void UpdateDiePosition(GameObject die, Vector2Int newPosition)
    {
        Vector2Int oldPosition = GetGridPosition(die.transform.position);

        diePositions.Remove(oldPosition);
        diePositions.Add(newPosition, die);
    }

    void RemoveConnectedDice(Vector2Int startPosition, int dieNumber, HashSet<GameObject> toBeRemoved)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(startPosition);

        while (stack.Count > 0)
        {
            Vector2Int position = stack.Pop();

            // この位置にサイコロがあり、まだ削除予定に入っていない場合
            if (diePositions.ContainsKey(position) && GetDieNumberAtPosition(position) == dieNumber)
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

    // 消える条件判定関数
    void CheckMatchesAndRemove()
    {
        // 既にチェックしたサイコロを追跡するためのセット
        HashSet<GameObject> checkedDice = new HashSet<GameObject>();
        HashSet<GameObject> toBeRemoved = new HashSet<GameObject>();

        foreach (GameObject die in diePositions.Values)
        {
            if (checkedDice.Contains(die)) continue;

            DieController dieController = die.GetComponent<DieController>();
            if (dieController == null || dieController.IsRolling()) continue;

            int dieNumber = dieController.GetDieNumber();
            HashSet<GameObject> connectedDice = new HashSet<GameObject>();

            // DFSで同じ目の隣接サイコロを探索
            DFS(die, dieNumber, connectedDice, checkedDice);

            int matchCount = Mathf.Max(dieNumber, 2); // 1の場合は2に設定

            if (connectedDice.Count >= matchCount)
            {
                foreach (GameObject d in connectedDice)
                {
                    toBeRemoved.Add(d);
                }
            }
        }

        // 削除予定のサイコロを順番に削除
        foreach (GameObject die in toBeRemoved)
        {
            StartCoroutine(RemoveDieAnimation(die));
        }
    }

    int GetDieNumberAtPosition(Vector2Int position)
    {
        GameObject die = GetDieAtPosition(position);
        if (die != null)
        {
            DieController dieController = die.GetComponent<DieController>();
            if (dieController != null)
            {
                return dieController.GetDieNumber();
            }
        }
        return -1; // サイコロがない場合やエラーの場合は -1 を返す
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
        DieController dieController = die.GetComponent<DieController>();


        if (dieController != null)
        {
            // 効果音再生
            if (audioSourceRemove != null && !dieController.isRemoving) // サイコロあたり 1 回のみ再生
            {
                audioSourceRemove.Play();
            }
            dieController.isRemoving = true; // アニメーション開始時にフラグを設定
        }

        if (renderer != null)
        {
            renderer.material.color = Color.red; // 色を赤に変更
        }

        // ここで30フレーム待機
        yield return StartCoroutine(WaitForFrames(30));

        float elapsedTime = 0;
        float sinkDuration = 10.0f; // 沈むのにかかる時間
        Vector3 startPosition = die.transform.position;
        Vector3 endPosition = startPosition + new Vector3(0, -1.0f, 0); // 徐々に下に沈む

        while (elapsedTime < sinkDuration)
        {
            elapsedTime += Time.deltaTime;
            if (die != null)
            {
                die.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / sinkDuration);
            }
            yield return null;
        }

        if (die != null)
        {
            Vector2Int diePosition = GetGridPosition(die.transform.position);

            diePositions.Remove(diePosition);

            Destroy(die);
        }
    }

    public GameObject GetDieAtPosition(Vector2Int position)
    {
        if (diePositions.TryGetValue(position, out GameObject die))
        {
            return die;
        }
        return null;
    }

    void DFS(GameObject die, int dieNumber, HashSet<GameObject> connectedDice, HashSet<GameObject> checkedDice)
    {
        connectedDice.Add(die);
        checkedDice.Add(die);

        Vector2Int position = GetGridPosition(die.transform.position);

        // 上下左右の方向
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int newPos = position + dir;
            if (diePositions.ContainsKey(newPos))
            {
                GameObject adjacentDie = diePositions[newPos];
                if (checkedDice.Contains(adjacentDie)) continue;

                DieController adjacentDieController = adjacentDie.GetComponent<DieController>();
                if (adjacentDieController != null && !adjacentDieController.IsRolling())
                {
                    if (adjacentDieController.GetDieNumber() == dieNumber)
                    {
                        DFS(adjacentDie, dieNumber, connectedDice, checkedDice);
                    }
                }
            }
        }
    }
}
