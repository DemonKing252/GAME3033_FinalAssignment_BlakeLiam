using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourPack : MonoBehaviour
{
    public void Start()
    {
    }

    // Update is called once per frame
    public void Update()
    {
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerController>().Armour < 100f)
            {
                other.GetComponent<PlayerController>().Armour += 25f;
                Destroy(gameObject);
            }
        }
    }

}
