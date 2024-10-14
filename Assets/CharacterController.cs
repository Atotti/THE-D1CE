using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public GameObject currentDie;  // 現在乗っているサイコロ

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RollDie(Vector3.forward);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            RollDie(Vector3.back);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RollDie(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            RollDie(Vector3.right);
        }

        // キャラクターの位置をサイコロの上に追従させる
        if (currentDie != null)
        {
            float characterHeightOffset = currentDie.transform.localScale.y / 2 + 1.0f; // サイコロの高さの半分 + キャラクターの高さの半分
            transform.position = currentDie.transform.position + new Vector3(0, characterHeightOffset, 0);
        }
    }

    void RollDie(Vector3 direction)
    {
        if (currentDie != null)
        {
            DieController dieController = currentDie.GetComponent<DieController>();
            if (dieController != null)
            {
                dieController.RollDie(direction);
            }
        }
    }
}
