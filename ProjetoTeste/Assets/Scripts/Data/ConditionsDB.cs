using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB : MonoBehaviour
{
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {ConditionID.Burn, new Condition()
        {
            StatusName = "BRN",
            StartMessage = "was burned",
            Description = "Burned Pokemon recieve damage every turn and have their physical attack reduced by 50%",
            StatusColor = Color.red,
            OnAfterTurn = (Pokemon pokemon) =>
            {
                pokemon.UpdateHP(pokemon.MaxHP / 16);
                pokemon.StatusChanges.Enqueue($"{pokemon.Base.Species} was hurt by burn");
            }
        }},
        {ConditionID.Poison, new Condition()
        {
            StatusName = "PSN",
            StartMessage = "was poisoned",
            Description = "Poisoned Pokemon recieve damage every turn",
            StatusColor = Color.magenta, // Purple
            OnAfterTurn = (Pokemon pokemon) =>
            {
                pokemon.UpdateHP(pokemon.MaxHP / 8);
                pokemon.StatusChanges.Enqueue($"{pokemon.Base.Species} was hurt by poison");
            }
        }},
        {ConditionID.Sleep, new Condition()
        {
            StatusName = "SLP",
            StartMessage = "fell asleep",
            Description = "Sleeping Pokemon cannot move",
            StatusColor = Color.gray, // Light Gray
            OnStart = (Pokemon pokemon) =>
            {
                pokemon.StatusTime = Random.Range(1, 6);
                pokemon.StatusChanges.Enqueue($"{pokemon.Base.Species} is fast asleep");
            },
            OnBeforeMove = (Pokemon pokemon) =>
            {
                if (pokemon.StatusTime <= 0)
                {
                    pokemon.CureStatus();
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Species} woke up!");
                    return true;
                }
                pokemon.StatusTime--;
                pokemon.StatusChanges.Enqueue($"{pokemon.Base.Species} is fast asleep");
                return false;
            }
        }},
        {ConditionID.Confusion, new Condition()
        {
            StatusName = "CONF",
            StartMessage = "has been confused",
            Description = "Confused Pokemon have a 50% chance of hurting themselves when attacking",
            StatusColor = new Color(1.0f, 0.5f, 0.0f),
            OnStart = (Pokemon pokemon) =>
            {
                pokemon.VolatileStatusTime = Random.Range(1, 6);
            },
            OnBeforeMove = (Pokemon pokemon) =>
            {
                if (pokemon.VolatileStatusTime <= 0)
                {
                    pokemon.CureStatus();
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Species} kicked out of confusion!");
                    return true;
                }
                pokemon.VolatileStatusTime--;
                if (Random.Range(1, 3) == 1)
                {
                    pokemon.UpdateHP(pokemon.MaxHP / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Species} is confused and hurt itself in confusion");
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }},
        {ConditionID.Paralyze, new Condition()
        {
            StatusName = "PAR",
            StartMessage = "was paralyzed",
            Description = "Paralyzed Pokemon have a 25% chance of not being able to move",
            StatusColor = Color.yellow,
            OnBeforeMove = (Pokemon pokemon) =>
            {
                if (Random.Range(1, 5) == 1)
                {
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Species} is paralyzed and can't move");
                    return false;
                }
                return true;
            }
        }},
        {ConditionID.Frozen, new Condition()
        {
            StatusName = "FRO",
            StartMessage = "was frozen",
            Description = "Frozen Pokemon cannot move",
            StatusColor = Color.cyan, // Light Blue
            OnBeforeMove = (Pokemon pokemon) =>
            {
                if (Random.Range(1, 5) == 1)
                {
                    pokemon.CureStatus();
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Species} thawed out");
                    return true;
                }
                pokemon.StatusChanges.Enqueue($"{pokemon.Base.Species} is frozen and can't move");
                return false;
            }
        }}
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
        {
            return 1f;
        }
        else if (condition.StatusName == "BRN" || condition.StatusName == "PAR" || condition.StatusName == "PSN")
        {
            return 1.5f;
        }
        else if (condition.StatusName == "FRO" || condition.StatusName == "SLP")
        {
            return 2f;
        }
        else
        {
            return 1f;
        }
    }
}

public enum ConditionID
{
    None,
    Burn,
    Poison,
    Sleep,
    Paralyze,
    Frozen,
    Confusion
}
