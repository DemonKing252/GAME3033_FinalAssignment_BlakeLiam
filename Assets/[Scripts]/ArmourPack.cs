using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourPack : MonoBehaviour
{
    [SerializeField]
    private float armourRegen = 25f;

    public float ArmourRegen { get { return armourRegen; } set { armourRegen = value; } }
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
                other.GetComponent<PlayerController>().Armour += armourRegen;
                PlayerController player = WeaponController.Instance.PlayerCtrl;
                player.ArmourPack.Remove(this);
                AudioManager.Instance.PlaySound(Sfx.Pickup);
                Destroy(gameObject);
            }
        }
    }

}
