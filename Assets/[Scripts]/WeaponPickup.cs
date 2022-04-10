using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private List<GameObject> weapons;
    private GameDialogue gameDialogue = null;
    private WeaponController weaponController;

    public WeaponType weaponType;
    private bool isPlayerColliding = false;


    private void Start()
    {
        SetWeaponEquipped(weaponType);

        gameDialogue = FindObjectOfType<GameDialogue>();
        weaponController = FindObjectOfType<WeaponController>();
    }
    public void SetWeaponEquipped(WeaponType type)
    {
        foreach (GameObject go in weapons)
            go.SetActive(false);

        weapons[(int)type].SetActive(true);
    }

    private void Update()
    {
        if (isPlayerColliding)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                int currentPlayerWeapon = (int)weaponController.weaponType;
                weaponController.SetActiveWeapon(weaponType);

                weaponType = (WeaponType)currentPlayerWeapon;
                SetWeaponEquipped(weaponType);

                weaponController.RefreshWeaponProperties();

                gameDialogue.SetDialogue("Press [E] to swap for " + weaponType.ToString());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerColliding = false;
            gameDialogue.ClearDialogue();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerColliding = true;
            gameDialogue.SetDialogue("Press [E] to swap for " + weaponType.ToString());
        }
    }
}
