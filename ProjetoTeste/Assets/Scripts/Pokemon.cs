using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    public PokemonBase Base { get { return _base; } }
    public int Level { get { return level; } }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<PokemonBase.Stat, int> Stats { get; private set; }
    public Dictionary<PokemonBase.Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; } = null;
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    public Queue<string> StatusChanges { get; private set; }
    public bool HpChanged { get; set; }
    public event System.Action OnStatusChanged;
    public int Exp { get; set; }

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;
        Init();
    }
    public void Init()
    {
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }
            if (Moves.Count >= 4)
                break;
        }

        Exp = Base.GetExpForLevel(Level, Base.GRate);
        CalculateStats();
        HP = MaxHP;
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<PokemonBase.Stat, int>();
        Stats.Add(PokemonBase.Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(PokemonBase.Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(PokemonBase.Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(PokemonBase.Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(PokemonBase.Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);
        Stats.Add(PokemonBase.Stat.Accuracy, 100);
        Stats.Add(PokemonBase.Stat.Evasion, 100);

        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + Level;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<PokemonBase.Stat, int>()
        {
            { PokemonBase.Stat.Attack, 0 },
            { PokemonBase.Stat.Defense, 0 },
            { PokemonBase.Stat.SpAttack, 0 },
            { PokemonBase.Stat.SpDefense, 0 },
            { PokemonBase.Stat.Speed, 0 },
            { PokemonBase.Stat.Accuracy, 0 },
            { PokemonBase.Stat.Evasion, 0 },
        };
    }

    int GetStat(PokemonBase.Stat stat)
    {
        int statValue = Stats[stat];
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
        {
            statValue = Mathf.FloorToInt(statValue * boostValues[boost]);
        }
        else
        {
            statValue = Mathf.FloorToInt(statValue / boostValues[-boost]);
        }

        return statValue;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        HpChanged = true;
    }

    public bool CheckForLevelUp()
    {
        if (Exp >= Base.GetExpForLevel(Level + 1, Base.GRate))
        {
            ++level;
            return true;
        }
        return false;
    }
    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null)
        {
            return;
        }
        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Species} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (VolatileStatus != null)
        {
            return;
        }
        VolatileStatus = ConditionsDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Species} {VolatileStatus.StartMessage}");
    }

    public int Attack
    {
        get { return GetStat(PokemonBase.Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(PokemonBase.Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(PokemonBase.Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(PokemonBase.Stat.SpDefense); }
    }

    public int Speed
    {
        get { return GetStat(PokemonBase.Stat.Speed); }
    }

    public int MaxHP
    {
        get; private set;
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        float attack = 1f;
        float defense = 1f;

        if (Random.value * 100f <= 6.25f)
        {
            critical *= 2f;
            Debug.Log("Critical Hit!");
        }

        if (move.Base.Category == MoveCategory.Special)
        {
            attack = attacker.SpAttack;
            defense = SpDefense;
        }
        else
        {
            attack = attacker.Attack;
            defense = Defense;
        }

        float type = PokemonBase.TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * PokemonBase.TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);
        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        if (move.Base.Power == 0)
        {
            damage = 0;
            critical = 0f;
        }

        var damageDetails = new DamageDetails()
        {
            Fainted = false,
            TypeEffectiveness = type,
            Critical = critical,
            STAB = 1f,
        };

        UpdateHP(damage);

        return damageDetails;
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.FindAll(m => m.PP > 0);
        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }
        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }
        return canPerformMove;
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this); //? means if not null
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Species}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Species}'s {stat} fell!");
            }
        }
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }

    public PokemonBase.LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == Level).FirstOrDefault();
    }

    public void LearnMove(PokemonBase.LearnableMove moveToLearn)
    {
        if (Moves.Count > 4)
        {
            return;
        }
        Moves.Add(new Move(moveToLearn.Base));
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
    public float STAB { get; set; }
}
