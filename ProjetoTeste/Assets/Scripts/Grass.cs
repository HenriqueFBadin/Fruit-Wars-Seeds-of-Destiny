using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerControler player)
    {
        if (UnityEngine.Random.Range(0, 101) <= 30)
        {
            GameController.Instance.StartBattle();
        }
    }
}
