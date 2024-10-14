using UnityEngine;
using System.Collections.Generic;

public class GridSystem : MonoBehaviour
{
    public int gridSizeX = 16;
    public int gridSizeY = 16;
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

    void CreateGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // 各マスの中心位置を計算
                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);

                // 床を作成（Cubeで表現）
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

}
