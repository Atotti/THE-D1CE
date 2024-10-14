using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public GameObject currentDie;  // 現在乗っているサイコロ
    public GridSystem gridSystem;  // GridSystemへの参照

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AttemptRoll(Vector3.forward);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AttemptRoll(Vector3.back);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            AttemptRoll(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            AttemptRoll(Vector3.right);
        }

        if (currentDie != null)
        {
            float characterHeightOffset = currentDie.transform.localScale.y / 2 + 1.0f;
            transform.position = currentDie.transform.position + new Vector3(0, characterHeightOffset, 0);
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
                    dieController.RollDie(direction);
                    gridSystem.UpdateDiePosition(currentDie, targetPosition);
                }
            }
        }
    }
}
