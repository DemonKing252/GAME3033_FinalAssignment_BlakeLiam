using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    // Start is called before the first frame update
    void Start()
    {
        GameObject go = Instantiate(ak47Prefab, armSocketTransform_Idle);
        heldWeapon = go;

        ak47MuzzleBack = go.transform.Find("Muzzle1");
        ak47MuzzleFront = go.transform.Find("Muzzle2");


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
    public Vector3 GetCrossHairWorldPoint()
    {
        Ray screenRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(screenRay, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 hitLocation = hit.point;

            Vector3 hitDirection = hit.point - cam.transform.position;
            Debug.DrawRay(cam.transform.position, hitDirection.normalized * 100f, Color.red, 1f);

            return hitLocation;

            //Vector3 screenToWorld = cam.ScreenToWorldPoint(ui.gameObject.GetComponent<RectTransform>().position);
            //screenToWorld += cam.transform.forward * 5f;
            //
            //return screenToWorld;
        }
        return Vector3.zero;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject bullet = Instantiate(bulletPrefab);

            Vector3 dir = GetCrossHairWorldPoint() - ak47MuzzleBack.position;

            bullet.GetComponent<BulletComponent>().FireAt(ak47MuzzleBack.position, dir, 3f, 10f);

        }
    }
}
