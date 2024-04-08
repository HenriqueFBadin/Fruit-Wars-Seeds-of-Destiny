using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import the TextMeshPro namespace
using DG.Tweening;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] TMP_Text hpText;
    [SerializeField] Pokemon _pokemon;
    [SerializeField] TMP_Text statusText;
    [SerializeField] GameObject expBar;
    int lastHP;

    public void setData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Species;
        SetLevel();
        hpBar.setHp((float)pokemon.HP / pokemon.MaxHP);
        SetExp();
        hpText.text = pokemon.HP.ToString() + "/" + pokemon.MaxHP.ToString();
        lastHP = pokemon.MaxHP;
        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
    }

    public void SetStatusText()
    {
        if (_pokemon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _pokemon.Status.StatusName.ToUpper();
            statusText.color = _pokemon.Status.StatusColor;
        }
    }

    public IEnumerator UpdateHP()
    {
        if (_pokemon.HpChanged)
        {
            int damage = lastHP - _pokemon.HP;

            float maxDuration = 2f;  // Maximum duration for the animation
            float duration = Mathf.Clamp(maxDuration / (((float)_pokemon.HP / _pokemon.MaxHP) + 0.0001f), 0f, maxDuration);
            float timer = 0f;

            while (timer < duration)
            {
                float t = timer / duration;

                // Use Mathf.Lerp to smoothly interpolate between lastHP and _pokemon.HP
                float currentHP = Mathf.Lerp(lastHP, _pokemon.HP, t);
                hpBar.setHp((float)currentHP / _pokemon.MaxHP);
                hpText.text = (Mathf.FloorToInt(currentHP)).ToString() + "/" + _pokemon.MaxHP.ToString();

                timer += Time.deltaTime;
                yield return null;  // Wait for the next frame
            }

            // Ensure the final values are set correctly
            hpBar.setHp((float)_pokemon.HP / _pokemon.MaxHP);
            hpText.text = _pokemon.HP.ToString() + "/" + _pokemon.MaxHP.ToString();

            lastHP = _pokemon.HP;
            _pokemon.HpChanged = false;
        }

    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = GetNormalizeExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) yield break;

        if (reset)
        {
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        float normalizedExp = GetNormalizeExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    private float GetNormalizeExp()
    {
        int currentLevelEXP = _pokemon.Base.GetExpForLevel(_pokemon.Level, _pokemon.Base.GRate);
        int nextLevelEXP = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1, _pokemon.Base.GRate);
        float normalizedExp = (float)(_pokemon.Exp - currentLevelEXP) / (nextLevelEXP - currentLevelEXP);
        return Mathf.Clamp(normalizedExp, 0f, 1f);
    }

    public void SetLevel()
    {
        levelText.text = "Lv." + _pokemon.Level.ToString();
    }
}
