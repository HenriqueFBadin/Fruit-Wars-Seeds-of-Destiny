using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{

    [SerializeField] GameObject health;

    // Start is called before the first frame update
    public void setHp(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
