using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import the TextMeshPro namespace

public class PartyScreen : MonoBehaviour
{
    PartyMember[] memberSlots;
    [SerializeField] TMP_Text messageText;
    List<Pokemon> pokemons;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMember>(true);
    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
        messageText.text = "Choose a Pokemon";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
