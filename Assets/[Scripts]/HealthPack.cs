using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [SerializeField]
    private float healthRegen = 25f;

    public float HealthRegen { get { return healthRegen; } set { healthRegen = value; } }

    // Start is called before the first frame update
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
            if (other.GetComponent<PlayerController>().Health < 100f)
            {
                other.GetComponent<PlayerController>().Health += healthRegen;
                PlayerController player = WeaponController.Instance.PlayerCtrl;
                player.HealthPacks.Remove(this);
                AudioManager.Instance.PlaySound(Sfx.Pickup);
                Destroy(gameObject);
            }
        }
    }
}
