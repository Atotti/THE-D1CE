using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public int gridSizeX = 16;  // 横方向のグリッド数
    public int gridSizeY = 16;  // 縦方向のグリッド数
    public float cellSize = 1.0f;  // 各グリッドのサイズ（1x1のグリッドにする）

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // グリッドの中心位置を計算
                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);

                // グリッドの表示のためにCubeを配置（デバッグ用）
                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cell.transform.position = cellPosition;
                cell.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);  // 厚みを0.1にして床として使う
                cell.GetComponent<Renderer>().material.color = Color.gray;  // グリッドをグレーに
            }
        }
    }
}
