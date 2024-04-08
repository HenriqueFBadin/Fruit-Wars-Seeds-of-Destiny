using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PokemonParty : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] List<Pokemon> pokemons;

    public List<Pokemon> Pokemons { get => pokemons; }

    public void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    public Pokemon GetHealthyPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
        }
        else
        {
            // Add to PC
        }
    }
}
