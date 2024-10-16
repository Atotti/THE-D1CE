using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    public GameObject currentDie;  // 現在乗っているサイコロ
    public GridSystem gridSystem;  // GridSystemへの参照
    private bool isRolling = false; // サイコロが転がり中かどうか
    public Text dieNumberText; // UIテキストの参照

    void Start()
    {
        // タグを使ってTextオブジェクトを取得
        dieNumberText = GameObject.FindGameObjectWithTag("DieNumberText").GetComponent<Text>();
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

        if (!IsCharacterOnGrid(this.gameObject))
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
        if (currentDie != null)
        {
            DieController dieController = currentDie.GetComponent<DieController>();
            if (dieController.isRemoving)
            {
                AttemptMoveOnRemovingDie(direction); // 消える途中のサイコロ上を移動する
            if (dieController.isSpawning)
            {
                AttemptMoveOnRemovingDie(direction); // 生成途中のサイコロ上の移動も消える途中と同じにする
            }
            } else
            {
                AttemptRoll(direction); // 通常状態 サイコロ上を移動する
            }
        }
        else
        {
            AttemptMoveOnGrid(direction); // グリッド上を移動する
        }
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
                foreach (GameObject die in gridSystem.diceList)
                {
                    Vector2Int diePosition = gridSystem.GetGridPosition(die.transform.position);

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
        Vector2Int targetGridPosition = gridSystem.GetGridPosition(targetPosition);

        if (gridSystem.IsPositionOccupied(targetGridPosition))
        {
            // キャラクターがサイコロに乗りなおす
            foreach (GameObject die in gridSystem.diceList)
            {
                Vector2Int diePosition = gridSystem.GetGridPosition(die.transform.position);

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
                // キャラクターが別のサイコロに乗り換える
                foreach (GameObject die in gridSystem.diceList)
                {
                    Vector2Int diePosition = gridSystem.GetGridPosition(die.transform.position);

                    if (diePosition == targetPosition)
                    {
                        currentDie = die;
                        break;
                    }
                }
            }
            else
            {
                // 消える途中のサイコロは転がらないのでサイコロが無いマスには移動できない
            }
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
