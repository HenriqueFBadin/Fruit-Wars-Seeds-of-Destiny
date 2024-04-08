using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Cutscene, Pause }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerControler playerControler;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera overworldCamera;

    TrainerController trainer;

    GameState state;
    GameState beforePauseState;
    // Update is called once per frame
    public GameState State
    {
        get { return state; }
    }

    public static GameController Instance
    { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += () => { state = GameState.Dialog; };
        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
            {
                state = GameState.FreeRoam;
            }
        };
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            beforePauseState = state;
            state = GameState.Pause;
        }
        else
        {
            state = beforePauseState;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        overworldCamera.gameObject.SetActive(false);

        var playerParty = playerControler.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPkmnCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleSystem.StartBattle(playerParty, wildPkmnCopy);
    }

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        overworldCamera.gameObject.SetActive(false);

        this.trainer = trainer;

        var playerParty = playerControler.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainerView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerControler));
    }

    void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerControler.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        overworldCamera.gameObject.SetActive(true);
    }
}
