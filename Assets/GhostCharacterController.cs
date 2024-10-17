using UnityEngine;
using UnityEngine.UI;

public class GhostCharacterController : MonoBehaviour
{
    public GameObject currentDie;  // 現在乗っているサイコロ
    public GridSystem gridSystem;  // GridSystemへの参照
    private bool isRolling = false; // サイコロが転がり中かどうか
    public Text dieNumberText; // UIテキストの参照

    void Start()
    {
        // タグを使ってTextオブジェクトを取得
        if (dieNumberText == null)
        {
            dieNumberText = GameObject.FindGameObjectWithTag("DieNumberText").GetComponent<Text>();
        }
    }

    void Update()
    {
        if (isRolling) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AttemptMove(Vector3.forward);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AttemptMove(Vector3.back);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            AttemptMove(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            AttemptMove(Vector3.right);
        }

        if (gridSystem == null)
        {
            gridSystem = FindObjectOfType<GridSystem>();
            if (gridSystem == null)
            {
                Debug.LogError("GridSystem is not found in the scene.");
            }
        }

        if (gridSystem != null && !IsCharacterOnGrid(this.gameObject))
        {
            currentDie = null;
        }

        if (currentDie != null)
        {
            float characterHeightOffset = currentDie.transform.localScale.y / 2 + 1.0f;
            transform.position = currentDie.transform.position + new Vector3(0, characterHeightOffset, 0);

            // サイコロの数の目を取得してUIテキストを更新
            DieController dieController = currentDie.GetComponent<DieController>();
            if (dieController != null)
            {
                int dieNumber = dieController.GetDieNumber();
                dieNumberText.text = "Current Die Number: " + dieNumber;
            }
        } else
        {
            dieNumberText.text = "Not on a die";
        }
    }

    void AttemptMove(Vector3 direction)
    {
        if (gridSystem == null)
        {
            Debug.LogError("GridSystem is not assigned.");
            return;
        }

        Vector3 targetPosition = transform.position + direction * gridSystem.cellSize;
        Vector2Int targetGridPosition = gridSystem.GetGridPosition(targetPosition);

        if (IsWithinGridBounds(targetGridPosition)) // グリッドの範囲外に出ないかどうか
        {
            if (currentDie != null)
            {
                DieController dieController = currentDie.GetComponent<DieController>();
                if (dieController != null)
                {
                    if (dieController.isRemoving || dieController.isSpawning)
                    {
                        AttemptMoveOnRemovingDie(direction); // 転がらずにサイコロ上を移動する
                    }
                    else
                    {
                        AttemptRoll(direction); // 通常状態 サイコロ上を移動する
                    }
                }
            }
            else
            {
                AttemptMoveOnGrid(direction); // グリッド上を移動する
            }
        }
    }

    private bool IsWithinGridBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridSystem.gridSizeX &&
               position.y >= 0 && position.y < gridSystem.gridSizeY;
    }

    void AttemptRoll(Vector3 direction)
    {
        if (currentDie != null)
        {
            Vector2Int currentPosition = gridSystem.GetGridPosition(currentDie.transform.position);

            Vector2Int targetPosition = currentPosition + new Vector2Int(
                Mathf.RoundToInt(direction.x),
                Mathf.RoundToInt(direction.z)
            );

            // ターゲット位置に既にサイコロがある場合
            if (gridSystem.IsPositionOccupied(targetPosition))
            {
                // キャラクターが別のサイコロに乗り換える
                currentDie = gridSystem.GetDieAtPosition(targetPosition);
            }
            else
            {
                // サイコロが転がる
                DieController dieController = currentDie.GetComponent<DieController>();
                if (dieController != null)
                {
                    isRolling = true; // 転がり開始
                    dieController.character = this.gameObject; // キャラクターの参照を設定
                    dieController.RollDie(direction, () => {
                        isRolling = false; // 転がり終了
                    });
                    gridSystem.UpdateDiePosition(currentDie, targetPosition);
                }
            }
        }
    }

    void AttemptMoveOnGrid(Vector3 direction)
    {
        Vector3 targetPosition = transform.position + direction * gridSystem.cellSize;

        // ターゲット位置にサイコロがある場合
        Vector2Int targetGridPosition = gridSystem.GetGridPosition(targetPosition);

        if (gridSystem.IsPositionOccupied(targetGridPosition))
        {
            // キャラクターがサイコロに乗りなおす
            GameObject targetDie = gridSystem.GetDieAtPosition(targetGridPosition);
            currentDie = targetDie;
            transform.position = targetDie.transform.position + new Vector3(0, 1.0f, 0); // サイコロの上に移動
        }
        else
        {
            // ターゲット位置にサイコロがない場合、Grid上を移動
            transform.position = targetPosition;
        }
    }

    // 削除途中 または Spawn途中のサイコロ上の移動
    void AttemptMoveOnRemovingDie(Vector3 direction)
    {
        if (currentDie != null)
        {
            Vector2Int currentPosition = gridSystem.GetGridPosition(currentDie.transform.position);

            Vector2Int targetPosition = currentPosition + new Vector2Int(
                Mathf.RoundToInt(direction.x),
                Mathf.RoundToInt(direction.z)
            );

            // ターゲット位置に既にサイコロがある場合
            if (gridSystem.IsPositionOccupied(targetPosition))
            {
                // キャラクターがサイコロに乗りなおす
                GameObject targetDie = gridSystem.GetDieAtPosition(targetPosition);
                currentDie = targetDie;
                transform.position = targetDie.transform.position + new Vector3(0, 1.0f, 0); // サイコロの上に移動
            }
            else
            {
                // 消える途中のサイコロは転がらないので移動できない
            }
        } else
        {

        }
    }

    private bool IsCharacterOnGrid(GameObject character)
    {
        // キャラクターの位置を取得
        Vector3 characterPosition = character.transform.position;

        // グリッドの範囲内にキャラクターがいるかどうかを確認
        // ここでは、グリッドの範囲を適切に設定してください
        float gridMinX = 0.0f;
        float gridMaxX = gridSystem.gridSizeX * gridSystem.cellSize;
        float gridMinZ = 0.0f;
        float gridMaxZ = gridSystem.gridSizeY * gridSystem.cellSize;

        return characterPosition.x >= gridMinX && characterPosition.x <= gridMaxX &&
            characterPosition.z >= gridMinZ && characterPosition.z <= gridMaxZ;
    }
}
