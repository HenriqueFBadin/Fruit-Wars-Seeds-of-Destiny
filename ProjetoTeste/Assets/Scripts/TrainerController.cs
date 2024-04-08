using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable
{
    [SerializeField] GameObject exclamation;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] string trainerName;
    [SerializeField] Sprite sprite;

    Character character;

    [SerializeField] GameObject fov;

    bool battleLost = false;

    public void Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);
        if (!battleLost)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => { GameController.Instance.StartTrainerBattle(this); }));
        }
        else
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle));
        }

    }

    public string TrainerName { get => trainerName; }
    public Sprite Sprite { get => sprite; }

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator TriggerTrainerBattle(PlayerControler player)
    {
        // Show exclamation mark
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        // Face player
        var playerPosition = player.transform.position;
        var trainerPosition = transform.position;
        var diff = (-trainerPosition + playerPosition);
        var moveVector = diff - diff.normalized;

        moveVector = new Vector2(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));

        // Move trainer
        yield return character.Move(moveVector);

        // Start Dialog
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => { GameController.Instance.StartTrainerBattle(this); }));
    }

    public void BattleLost()
    {
        fov.gameObject.SetActive(false);
        battleLost = true;
    }

    public void SetFovRotation(FaceDirection dir)
    {
        float angle = 0f;
        if (dir == FaceDirection.Up)
        {
            angle = 180f;
        }
        else if (dir == FaceDirection.Right)
        {
            angle = 90f;
        }
        else if (dir == FaceDirection.Down)
        {
            angle = 0f;
        }
        else if (dir == FaceDirection.Left)
        {
            angle = 270f;
        }

        fov.transform.eulerAngles = new Vector3(0, 0, angle);
    }
}
