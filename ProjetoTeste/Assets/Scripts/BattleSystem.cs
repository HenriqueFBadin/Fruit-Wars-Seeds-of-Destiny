using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, MoveToForget, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    public event Action<bool> OnBattleOver;
    BattleState state;
    BattleState? previousState;
    int currentAction;
    int currentMove;
    int currentMember;
    bool aboutToUseChoice = true;
    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;
    bool isTrainerBattle = false;
    PlayerControler player;
    TrainerController trainer;
    int escapeAttempts;
    MoveBase moveToLearn;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        Debug.Log(isTrainerBattle);
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerControler>();
        StartCoroutine(setupBattle());
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerControler>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(setupBattle());
    }

    // Update is called once per frame
    public IEnumerator setupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

            yield return (dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Species} appeared!"));
        }
        else
        {
            // Show trainer and player sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);
            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.TrainerName} wants to battle!");

            // Send out first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);

            var trainerPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(trainerPokemon);

            yield return dialogBox.TypeDialog($"{trainer.TrainerName} send out {trainerPokemon.Base.Species}!");

            // Send out first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);

            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Species}!");

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        escapeAttempts = 0;
        partyScreen.Init();

        yield return new WaitForSeconds(1f);

        ActionSelection();
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("What will you do next?");
        dialogBox.EnableActionSelector(true);
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver()); // reset stats
        OnBattleOver(won);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.TrainerName} is going for {newPokemon.Base.Species}. Do you want to switch?");
        state = BattleState.AboutToUse;
        dialogBox.EnableOnChangeSelector(true);
    }

    public void HandleUpdate()
    {
        Debug.Log("BattleSystem: " + state);

        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == 4)
                {
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Species} did not learn {moveToLearn.Name}"));
                }
                else
                {
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Species} forgot {selectedMove.Name} and learned {moveToLearn.Name}!"));
                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };
            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction < 3)
            {
                ++currentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAction > 0)
            {
                --currentAction;
            }
        }

        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 2)
            {
                currentAction += 2;
            }
        }

        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 1)
            {
                currentAction -= 2;
            }
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                // Bag
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }
            else if (currentAction == 2)
            {
                // Pokemon
                previousState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                // Run
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
            {
                ++currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 0)
            {
                --currentMove;
            }
        }

        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
            {
                currentMove += 2;
            }
        }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 1)
            {
                currentMove -= 2;
            }
        }

        dialogBox.UpdateActionSelection(currentMove);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0)
            {
                return;
            }
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMember;
        }

        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }

        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted Pokemon!");
            }
            else if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't switch with the same Pokemon!");
            }
            else
            {
                partyScreen.gameObject.SetActive(false);
                if (previousState == BattleState.ActionSelection)
                {
                    previousState = null;
                    StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
                }
                else
                {
                    state = BattleState.Busy;
                    StartCoroutine(SwitchPokemon(selectedMember));
                }

            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a Pokemon to continue!");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (previousState == BattleState.AboutToUse)
            {
                previousState = null;
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
            {
                ActionSelection();
            }
        }

    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogBox.UpdateOnChangeSelection(aboutToUseChoice);
        Debug.Log(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableOnChangeSelector(false);
            if (aboutToUseChoice == true)
            {
                previousState = BattleState.AboutToUse;
                OpenPartyScreen();
            }
            else
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (canRunMove == false)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Species} used {move.Base.Name}!");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayerAttackAnimation();
            yield return new WaitForSeconds(1f);

            targetUnit.PlayerHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                if (move.Base.Effects.Boosts != null)
                {
                    if (move.Base.Target == MoveTarget.Self)
                    {
                        sourceUnit.Pokemon.ApplyBoosts(move.Base.Effects.Boosts);
                    }
                    else
                    {
                        targetUnit.Pokemon.ApplyBoosts(move.Base.Effects.Boosts);
                    }
                }

                //Set Status

                if (move.Base.Effects.Status != ConditionID.None)
                {
                    targetUnit.Pokemon.SetStatus(move.Base.Effects.Status);
                }

                // Set Volatile Status
                if (move.Base.Effects.VolatileStatus != ConditionID.None)
                {
                    targetUnit.Pokemon.SetVolatileStatus(move.Base.Effects.VolatileStatus);
                }

                yield return ShowStatusChanges(sourceUnit.Pokemon);
                yield return ShowStatusChanges(targetUnit.Pokemon);

            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);

                StartCoroutine(targetUnit.Hud.UpdateHP());
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondaryEffect in move.Base.SecondaryEffects)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondaryEffect.Chance)
                    {
                        if (secondaryEffect.Boosts != null)
                        {
                            targetUnit.Pokemon.ApplyBoosts(secondaryEffect.Boosts);
                        }

                        if (secondaryEffect.Status != ConditionID.None)
                        {
                            targetUnit.Pokemon.SetStatus(secondaryEffect.Status);
                            yield return ShowStatusChanges(targetUnit.Pokemon);
                        }

                        if (secondaryEffect.VolatileStatus != ConditionID.None)
                        {
                            targetUnit.Pokemon.SetVolatileStatus(secondaryEffect.VolatileStatus);
                        }
                    }
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return (HandlePokemonFainted(targetUnit));
            }
        }

        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Species}'s attack missed!");
        }


    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon); // Print status changes
        yield return sourceUnit.Hud.UpdateHP();

        // Check if fainted by status
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            // End battle
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    public bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AwaysHits)
        {
            return true;
        }
        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[PokemonBase.Stat.Accuracy];
        int evasion = target.StatBoosts[PokemonBase.Stat.Evasion];
        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };
        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }
        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                // black out
                BattleOver(false);
            }
        }
        else
        {
            if (isTrainerBattle)
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    StartCoroutine(AboutToUse(nextPokemon));
                }
                else
                {
                    isTrainerBattle = false;
                    BattleOver(true);
                }
            }
            else
            {
                BattleOver(true);
            }
        }
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;
        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            // Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            var firstUnit = playerGoesFirst ? playerUnit : enemyUnit;
            var secondUnit = playerGoesFirst ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            // First turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return new WaitForSeconds(1f);
            yield return new WaitForSeconds(1f);
            if (state == BattleState.BattleOver) yield break;

            if (secondPokemon.HP > 0)
            {
                // Second turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return new WaitForSeconds(1f);
                yield return RunAfterTurn(firstUnit);
                yield return RunAfterTurn(secondUnit);
                yield return new WaitForSeconds(1f);
                if (state == BattleState.BattleOver) yield break;
            }

        }
        else if (playerAction == BattleAction.SwitchPokemon)
        {
            var selectedPokemon = playerParty.Pokemons[currentMember];
            state = BattleState.Busy;
            yield return SwitchPokemon(selectedPokemon);

            // Enemy's turn
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return new WaitForSeconds(1f);
            yield return RunAfterTurn(enemyUnit);
            yield return new WaitForSeconds(1f);
            if (state == BattleState.BattleOver) yield break;
        }
        else if (playerAction == BattleAction.UseItem)
        {
            dialogBox.EnableActionSelector(false);
            yield return ThrowPokeball();
        }
        else if (playerAction == BattleAction.Run)
        {
            yield return TryToEscape();
        }

        if (state != BattleState.BattleOver && state != BattleState.AboutToUse)
        {
            ActionSelection();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("A critical hit!");
            yield return new WaitForSeconds(1f);
        }

        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("It's super effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("It's not very effective...");
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Species}!");
            playerUnit.FaintAnimation();
            yield return new WaitForSeconds(1f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Species}!");
        yield return new WaitForSeconds(1f);

        if (previousState == null)
        {
            state = BattleState.RunningTurn;
        }
        else if (previousState == BattleState.AboutToUse)
        {
            previousState = null;
            StartCoroutine(SendNextTrainerPokemon());

        }
    }

    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();

        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.TrainerName} send out {nextPokemon.Base.Species}!");
        yield return new WaitForSeconds(1f);

        ActionSelection();
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Your Pokemon wants to learn {newMove.Name}!");
        yield return new WaitForSeconds(0.5f);
        yield return dialogBox.TypeDialog($"But your Pokemon already knows four moves!");
        yield return new WaitForSeconds(0.5f);
        yield return dialogBox.TypeDialog($"Choose a move you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("You can't steal another trainer's Pokemon!");
            yield return new WaitForSeconds(1f);
            state = BattleState.RunningTurn;
            yield break;
        }
        yield return dialogBox.TypeDialog($"You throw a pokeball at {enemyUnit.Pokemon.Base.Species}!");

        var pokeballObject = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(100f, 0), Quaternion.identity);
        var pokeball = pokeballObject.GetComponent<SpriteRenderer>();

        // Animation of throwing the pokeball
        yield return (pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 50f, 1, 1f).WaitForCompletion());
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 50f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon);

        // Animation of the pokeball shaking
        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.5f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            yield return dialogBox.TypeDialog($"You caught {enemyUnit.Pokemon.Base.Species}!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Species} has been added to your party!");
            yield return new WaitForSeconds(1f);

            Destroy(pokeballObject);
            BattleOver(true);
        }
        else
        {
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Species} broke free!");

            yield return new WaitForSeconds(1f);

            Destroy(pokeballObject);
            state = BattleState.RunningTurn;
        }
    }

    int TryToCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHP - 2 * pokemon.HP) * pokemon.Base.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHP);

        if (a >= 255)
        {
            return 4;
        }

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;

        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
            {
                break;
            }
            ++shakeCount;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("You can't run from a trainer battle!");
            yield return new WaitForSeconds(1f);
            state = BattleState.RunningTurn;
            yield break;
        }

        var playerSpeed = playerUnit.Pokemon.Speed;
        var enemySpeed = enemyUnit.Pokemon.Speed;

        ++escapeAttempts;

        if (playerSpeed >= enemySpeed)
        {
            yield return dialogBox.TypeDialog("You got away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog("You got away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog("You couldn't get away!");
                state = BattleState.RunningTurn;
            }
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return new WaitForSeconds(1f);
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Species} fainted!");
        yield return new WaitForSeconds(1f);
        faintedUnit.FaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            // Get exp
            var expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = isTrainerBattle ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Species} gained {expGain} exp!");
            yield return playerUnit.Hud.SetExpSmooth();

            // Check level up
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return new WaitForSeconds(1f);
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Species} grew to level {playerUnit.Pokemon.Level}!");
                yield return new WaitForSeconds(1f);

                // Try to learn new move
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrLevel();

                if (newMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < 4)
                    {
                        playerUnit.Pokemon.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Species} learned {newMove.Base.Name}!");
                        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        //Forget a move to learn new move
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }
        }

        CheckForBattleOver(faintedUnit);
    }
}
