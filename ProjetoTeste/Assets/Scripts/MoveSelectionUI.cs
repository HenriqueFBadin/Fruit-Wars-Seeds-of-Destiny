using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<TMP_Text> moveTexts;
    [SerializeField] Color highlightedColor;
    int selection = 0;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].Name;
        }

        moveTexts[currentMoves.Count].text = newMove.Name;
    }

    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selection++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selection--;
        }

        selection = Mathf.Clamp(selection, 0, 4);

        UpdateMoveSelection();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke(selection);
        }
    }

    public void UpdateMoveSelection()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i != selection)
            {
                moveTexts[i].color = Color.black;
            }
            else
            {
                moveTexts[i].color = highlightedColor;
            }
        }
    }
}
