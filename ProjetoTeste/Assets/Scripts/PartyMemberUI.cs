using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import the TextMeshPro namespace

public class PartyMember : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] TMP_Text hpText;
    private Image spriteRenderer;

    [SerializeField] Sprite newSprite;
    Pokemon _pokemon;

    [SerializeField] Sprite originalSprite;

    public void SetData(Pokemon pokemon)
    {

        _pokemon = pokemon;
        nameText.text = pokemon.Base.Species;
        levelText.text = "Lv." + pokemon.Level.ToString();
        hpBar.setHp((float)pokemon.HP / pokemon.MaxHP);
        hpText.text = pokemon.HP.ToString() + "/" + pokemon.MaxHP.ToString();
        spriteRenderer = GetComponent<Image>();
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            spriteRenderer.sprite = newSprite;
        }
        else
        {
            spriteRenderer.sprite = originalSprite;
        }
    }

}