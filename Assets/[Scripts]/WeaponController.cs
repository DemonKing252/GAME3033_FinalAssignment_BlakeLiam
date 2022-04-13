using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject[] weaponPrefabs;
    [SerializeField] private List<GameObject> equippedWeapons;
    [SerializeField] private Transform armSocketTransform_Idle;
    [SerializeField] private Transform armSocketTransform_Moving;
    [SerializeField] private TMP_Text weaponNameText;


    [SerializeField] private GameObject bulletPrefab;

    private PlayerController pController;
    public PlayerController PlayerCtrl => pController;

    private GameObject heldWeapon;

    [SerializeField] private Image ui;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask groundLayer;
    public TMP_Text ammoText;

    private bool canFireWeapon = false;

    private static WeaponController sInstance;
    public static WeaponController Instance => sInstance;
    public GameObject EquippedWeapon => equippedWeapons[(int)weaponType];
    public List<GameObject> GetAllWeapons => equippedWeapons;
    
    public WeaponType weaponType = WeaponType.AK_47;

    void Awake()
    {
        sInstance = this;    
    }

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


        pController = GetComponent<PlayerController>();
        pController.SetGripTransform(equippedWeapons[(int)weaponType].transform.Find("Grip"));

        pController.onMovementStateChanged += OnMovementStateChanged;
        pController.onAimStateChanged += OnAimStateChanged;

        RefreshWeaponProperties();
    }
    public void RefreshWeaponProperties()
    {
        ammoText.text = equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoCount.ToString() + " / " + equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoTotal.ToString();
        weaponNameText.text = equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.name;
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
            if (equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.loopFire && equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoCount > 0)
            {
                InvokeRepeating(nameof(InvokeFire), 0f, equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.fireRate);
            }
            else
            {
                if (equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoCount > 0)
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

        int ammoDifference = equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.startingMagSize - equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoCount;
        if (equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoTotal > ammoDifference)
        {
            equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoCount += ammoDifference;
            equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoTotal -= ammoDifference;
        }
        else
        {
            equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoCount += equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoTotal;
            equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoTotal = 0;
        }

        ammoText.text = equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoCount.ToString() + " / " + equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoTotal.ToString();

        pController.isReloading = true;

        yield return new WaitForSeconds(1f);

        pController.isReloading = false;
    }

    public void InvokeFire()
    {
        if (equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoCount <= 0)
            CancelInvoke(nameof(InvokeFire));
        else
            OnFire();
    }
    public void OnFire()
    {
        equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoCount--;
        ammoText.text = equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoCount.ToString() + " / " + equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().weapon.ammoTotal.ToString();

        GameObject bullet = Instantiate(bulletPrefab);
        pController.anim.SetTrigger("IsFiring");
        Vector3 dir = GetCrossHairWorldPoint() - equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().muzzleBack.position;
        bullet.GetComponent<BulletComponent>().FireAt(equippedWeapons[(int)weaponType].GetComponent<WeaponProperties>().muzzleFront.position, dir, 60f, 10f);

    }

}
