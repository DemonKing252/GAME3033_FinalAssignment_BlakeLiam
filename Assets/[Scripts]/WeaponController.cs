using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject ak47Prefab;
    [SerializeField] private Transform armSocketTransform_Idle;
    [SerializeField] private Transform armSocketTransform_Moving;

    private PlayerController pController;

    private GameObject heldWeapon;
    [SerializeField] private Image ui;

    // Start is called before the first frame update
    void Start()
    {
        GameObject go = Instantiate(ak47Prefab, armSocketTransform_Idle);
        heldWeapon = go;

        pController = GetComponent<PlayerController>();
        pController.SetGripTransform(go.transform.Find("Grip"));

        pController.onMovementStateChanged += OnAimStateChanged;
    }
    public void OnAimStateChanged(bool moving)
    {
        if (moving)
        {
            //Debug.Log(Camera.main.ScreenToWorldPoint(ui.transform.position));


            heldWeapon.transform.position = armSocketTransform_Moving.position;
            heldWeapon.transform.rotation = armSocketTransform_Moving.rotation;
            //heldWeapon.transform.SetParent(armSocketTransform_Idle);
        }
        else
        {
            heldWeapon.transform.position = armSocketTransform_Idle.position;
            heldWeapon.transform.rotation = armSocketTransform_Idle.rotation;
            //heldWeapon.transform.SetParent(armSocketTransform_Moving);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
