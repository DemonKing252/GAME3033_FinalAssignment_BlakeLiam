using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Weapon
{
    public string name = "AK47";
    public bool loopFire;
    public float fireRate = 0.2f;
    public int ammoCount = 30;
    public int ammoTotal = 90;
    public int startingMagSize = 30;
}
public enum WeaponType
{
    AK_47,
    DoubleBarrelShotgun

}


public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject[] weaponPrefabs;
    [SerializeField] private List<GameObject> equippedWeapons;
    [SerializeField] private Transform armSocketTransform_Idle;
    [SerializeField] private Transform armSocketTransform_Moving;

    [SerializeField] private GameObject bulletPrefab;

    private PlayerController pController;

    private GameObject heldWeapon;
    //public Transform ak47MuzzleBack;
    //public Transform ak47MuzzleFront;

    [SerializeField] private Image ui;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask groundLayer;
    public TMP_Text ammoText;

    private bool canFireWeapon = false;

    [SerializeField] private Weapon equippedWeapon;

    // This will eventually move to a Weapon script on the prefab once we have multiple weapons.

    public WeaponType weaponType = WeaponType.AK_47;
    // Start is called before the first frame update
    void Start()
    {
        //GameObject go = Instantiate(ak47Prefab, armSocketTransform_Idle);
        for (int i = 0; i < weaponPrefabs.Length; i++)
        {
            GameObject go = Instantiate(weaponPrefabs[i], armSocketTransform_Idle);
            equippedWeapons.Add(go);
            if ((WeaponType)i != weaponType)
            {
                equippedWeapons[i].SetActive(false);
            }
        }
        
        heldWeapon = equippedWeapons[(int)weaponType];

        //ak47MuzzleBack = equippedWeapons[(int)weaponType].transform.Find("Muzzle1");
        //ak47MuzzleFront = equippedWeapons[(int)weaponType].transform.Find("Muzzle2");


        pController = GetComponent<PlayerController>();
        pController.SetGripTransform(equippedWeapons[(int)weaponType].transform.Find("Grip"));

        pController.onMovementStateChanged += OnMovementStateChanged;
        pController.onAimStateChanged += OnAimStateChanged;


        ammoText.text = equippedWeapon.ammoCount.ToString() + " / " + equippedWeapon.ammoTotal.ToString();
    }
    public void SetActiveWeapon(WeaponType weaponType)
    {
        this.weaponType = weaponType;
        for (int i = 0; i < weaponPrefabs.Length; i++)
        {
            if ((WeaponType)i != weaponType)
            {
                equippedWeapons[i].SetActive(false);
            }
            equippedWeapons[(int)weaponType].SetActive(true);
        }
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
            if (equippedWeapon.loopFire && equippedWeapon.ammoCount > 0)
            {
                InvokeRepeating(nameof(InvokeFire), 0f, equippedWeapon.fireRate);
            }
            else
            {
                if (equippedWeapon.ammoCount > 0)
                    OnFire();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            CancelInvoke(nameof(InvokeFire));
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(OnReload());   
        }
    }
    IEnumerator OnReload()
    {

        pController.anim.SetTrigger("IsReloading");

        int ammoDifference = equippedWeapon.startingMagSize - equippedWeapon.ammoCount;
        if (equippedWeapon.ammoTotal > ammoDifference)
        {
            equippedWeapon.ammoCount += ammoDifference;
            equippedWeapon.ammoTotal -= ammoDifference;
        }
        else
        {
            equippedWeapon.ammoCount += equippedWeapon.ammoTotal;
            equippedWeapon.ammoTotal = 0;
        }

        ammoText.text = equippedWeapon.ammoCount.ToString() + " / " + equippedWeapon.ammoTotal.ToString();

        pController.isReloading = true;

        yield return new WaitForSeconds(1f);

        pController.isReloading = false;
    }

    public void InvokeFire()
    {
        if (equippedWeapon.ammoCount <= 0)
            CancelInvoke(nameof(InvokeFire));
        else
            OnFire();
    }
    public void OnFire()
    {
        equippedWeapon.ammoCount--;
        ammoText.text = equippedWeapon.ammoCount.ToString() + " / " + equippedWeapon.ammoTotal.ToString();

        GameObject bullet = Instantiate(bulletPrefab);
        pController.anim.SetTrigger("IsFiring");
        Vector3 dir = GetCrossHairWorldPoint() - equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().muzzleBack.position;
        bullet.GetComponent<BulletComponent>().FireAt(equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().muzzleFront.position, dir, 60f, 10f);

    }

}
