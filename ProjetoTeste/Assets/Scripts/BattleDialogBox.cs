using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] TMP_Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject OnChangeSelector;

    [SerializeField] List<TMP_Text> actionTexts;
    [SerializeField] List<TMP_Text> moveTexts;

    [SerializeField] TMP_Text ppText;
    [SerializeField] TMP_Text typeText;
    [SerializeField] TMP_Text yesText;
    [SerializeField] TMP_Text noText;

    [SerializeField] Color highlightedColor;

    public Color HighlightedColor { get => highlightedColor; }

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / 30); // Pause for a fraction of a second and then continue
        }
    }

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void EnableOnChangeSelector(bool enabled)
    {
        OnChangeSelector.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; ++i)
        {
            if (i == selectedAction)
            {
                actionTexts[i].color = highlightedColor;
            }
            else
            {
                actionTexts[i].color = Color.black;
            }
        }
    }

    public void UpdateOnChangeSelection(bool yesSelector)
    {
        if (yesSelector)
        {
            yesText.color = highlightedColor;
            noText.color = Color.black;
        }
        else
        {
            yesText.color = Color.black;
            noText.color = highlightedColor;
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = "-";
            }
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i == selectedMove)
            {
                moveTexts[i].color = highlightedColor;
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }

        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if (move.PP == 0)
        {
            ppText.color = Color.red;
        }
        else if (move.PP <= move.Base.PP / 2)
        {
            ppText.color = Color.yellow;
        }
        else
        {
            ppText.color = Color.black;
        }
    }
}
