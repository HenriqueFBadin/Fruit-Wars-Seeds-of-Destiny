using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;

    private void Awake()
    {
        if (FindObjectsOfType<EssentialObjects>().Length == 0)
        {
            Instantiate(essentialObjectsPrefab, new UnityEngine.Vector3(0, 0, 0), UnityEngine.Quaternion.identity);
        }
    }
}
