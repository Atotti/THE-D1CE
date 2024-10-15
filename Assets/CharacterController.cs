using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public GameObject currentDie;  // 現在乗っているサイコロ
    public GridSystem gridSystem;  // GridSystemへの参照
    private bool isRolling = false; // サイコロが転がり中かどうか

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

        if (!IsCharacterOnGrid(this.gameObject))
        {
            currentDie = null;
        }


        if (currentDie != null)
        {
            float characterHeightOffset = currentDie.transform.localScale.y / 2 + 1.0f;
            transform.position = currentDie.transform.position + new Vector3(0, characterHeightOffset, 0);
        }
    }

    void AttemptMove(Vector3 direction)
    {
        if (currentDie != null)
        {
            AttemptRoll(direction);
        }
        else
        {
            AttemptMoveOnGrid(direction);
        }
    }

    void AttemptRoll(Vector3 direction)
    {
        if (currentDie != null)
        {
            Vector2Int currentPosition = new Vector2Int(
                Mathf.RoundToInt(currentDie.transform.position.x / gridSystem.cellSize),
                Mathf.RoundToInt(currentDie.transform.position.z / gridSystem.cellSize)
            );

            Vector2Int targetPosition = currentPosition + new Vector2Int(
                Mathf.RoundToInt(direction.x),
                Mathf.RoundToInt(direction.z)
            );

            // ターゲット位置に既にサイコロがある場合
            if (gridSystem.IsPositionOccupied(targetPosition))
            {
                // キャラクターが別のサイコロに乗り換える
                foreach (GameObject die in gridSystem.diceList)
                {
                    Vector2Int diePosition = new Vector2Int(
                        Mathf.RoundToInt(die.transform.position.x / gridSystem.cellSize),
                        Mathf.RoundToInt(die.transform.position.z / gridSystem.cellSize)
                    );

                    if (diePosition == targetPosition)
                    {
                        currentDie = die;
                        break;
                    }
                }
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
        Vector2Int targetGridPosition = new Vector2Int(
            Mathf.RoundToInt(targetPosition.x / gridSystem.cellSize),
            Mathf.RoundToInt(targetPosition.z / gridSystem.cellSize)
        );

        if (gridSystem.IsPositionOccupied(targetGridPosition))
        {
            // キャラクターがサイコロに乗り換える
            foreach (GameObject die in gridSystem.diceList)
            {
                Vector2Int diePosition = new Vector2Int(
                    Mathf.RoundToInt(die.transform.position.x / gridSystem.cellSize),
                    Mathf.RoundToInt(die.transform.position.z / gridSystem.cellSize)
                );

                if (diePosition == targetGridPosition)
                {
                    currentDie = die;
                    transform.position = die.transform.position + new Vector3(0, 1.0f, 0); // サイコロの上に移動
                    break;
                }
            }
        }
        else
        {
            // ターゲット位置にサイコロがない場合、Grid上を移動
            transform.position = targetPosition;
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
