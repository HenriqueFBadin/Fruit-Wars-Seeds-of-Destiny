using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;
    PlayerControler player;

    public Transform SpawnPoint => spawnPoint;

    Fader faderTransition;

    public void Start()
    {
        faderTransition = FindObjectOfType<Fader>();
    }

    public void OnPlayerTriggered(PlayerControler player)
    {
        this.player = player;
        StartCoroutine(SwitchScene());
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        GameController.Instance.PauseGame(true);

        yield return faderTransition.FadeIn(0.5f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return faderTransition.FadeOut(0.5f);

        GameController.Instance.PauseGame(false);

        Destroy(gameObject);
    }
}

public enum DestinationIdentifier
{
    A, B, C, D, E
}
