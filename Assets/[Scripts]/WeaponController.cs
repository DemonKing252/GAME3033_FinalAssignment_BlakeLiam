using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject ak47Prefab;
    [SerializeField] private Transform armSocketTransform_Idle;
    [SerializeField] private Transform armSocketTransform_Moving;

    [SerializeField] private GameObject bulletPrefab;

    private PlayerController pController;

    private GameObject heldWeapon;
    public Transform ak47MuzzleBack;
    public Transform ak47MuzzleFront;

    [SerializeField] private Image ui;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask groundLayer;
    private bool canFireWeapon = false;


    // This will eventually move to a Weapon script on the prefab once we have multiple weapons.
    [Header("Weapon Properties")]
    [SerializeField] private bool loopFire;
    [SerializeField] private int startingAmmo;
    [SerializeField] private int startingMagSize = 30;
    [SerializeField] private float fireRate = 0.2f;

    [SerializeField] private TMP_Text ammoText;


    private int currentMagSize = 30;
    private int currentAmmo = 300;

    

    // Start is called before the first frame update
    void Start()
    {
        GameObject go = Instantiate(ak47Prefab, armSocketTransform_Idle);
        heldWeapon = go;

        ak47MuzzleBack = go.transform.Find("Muzzle1");
        ak47MuzzleFront = go.transform.Find("Muzzle2");


        pController = GetComponent<PlayerController>();
        pController.SetGripTransform(go.transform.Find("Grip"));

        pController.onMovementStateChanged += OnMovementStateChanged;
        pController.onAimStateChanged += OnAimStateChanged;

        currentMagSize = startingMagSize;
        currentAmmo = startingAmmo;

        ammoText.text = currentMagSize.ToString() + " / " + currentAmmo.ToString();
    }

    private void OnDestroy()
    {
        // Important to avoid memory leaks
        pController.onMovementStateChanged -= OnMovementStateChanged;
        pController.onAimStateChanged -= OnAimStateChanged;

    }
    public void OnMovementStateChanged(bool moving)
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

    public void OnAimStateChanged(bool isAiming)
    {
        canFireWeapon = isAiming;
        if (isAiming)
        {
            ui.gameObject.SetActive(true);
        }
        else
        {
            ui.gameObject.SetActive(false);
        }
    }

    public Vector3 GetCrossHairWorldPoint()
    {
        Ray screenRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(screenRay, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 hitLocation = hit.point;
            Vector3 hitDirection = hit.point - cam.transform.position;
            Debug.DrawRay(cam.transform.position, hitDirection.normalized * 100f, Color.red, 1f);
            return hitLocation;
        }
        else
        {
            return cam.ScreenToWorldPoint(ui.GetComponent<RectTransform>().position) + (cam.gameObject.transform.forward * 20f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canFireWeapon)
        {
            if (loopFire && currentMagSize > 0)
            {
                InvokeRepeating("InvokeFire", 0f, fireRate);
            }
            else
            {
                if (currentMagSize > 0)
                    OnFire();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            CancelInvoke("InvokeFire");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(OnReload());   
        }
    }
    IEnumerator OnReload()
    {

        pController.anim.SetTrigger("IsReloading");

        int bulletsToReload = startingMagSize - currentAmmo;
        if (bulletsToReload < 0)
        {
            currentAmmo -= (startingMagSize - currentMagSize);
            currentMagSize = startingMagSize;
        }
        else
        {
            currentMagSize = startingAmmo;
            currentAmmo = 0;
        }
        ammoText.text = currentMagSize.ToString() + " / " + currentAmmo.ToString();

        pController.isReloading = true;

        yield return new WaitForSeconds(1f);

        pController.isReloading = false;
    }

    public void InvokeFire()
    {
        if (currentMagSize <= 0)
            CancelInvoke("InvokeFire");

        OnFire();
    }
    public void OnFire()
    {
        currentMagSize--;
        ammoText.text = currentMagSize.ToString() + " / " + currentAmmo.ToString();

        GameObject bullet = Instantiate(bulletPrefab);
        pController.anim.SetTrigger("IsFiring");
        Vector3 dir = GetCrossHairWorldPoint() - ak47MuzzleBack.position;
        bullet.GetComponent<BulletComponent>().FireAt(ak47MuzzleBack.position, dir, 60f, 10f);

    }

}
