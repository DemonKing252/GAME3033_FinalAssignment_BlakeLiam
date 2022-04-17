using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponProperties : MonoBehaviour
{
    public Transform gripIKTransform;
    public Transform muzzleFront;
    public Transform muzzleBack;
    public GameObject muzzleFlashPrefab;

    public Weapon weapon;

    public Vector3 desiredParticleScale = Vector3.one;
    public float awakeTime = 0.2f;

    public IEnumerator SpawnMuzzleFlash()
    {
        GameObject go = Instantiate(muzzleFlashPrefab, muzzleBack);
        go.transform.localScale = desiredParticleScale;
        go.GetComponent<ParticleSystem>().Play();
        go.transform.localPosition = Vector3.zero;

        yield return new WaitForSeconds(awakeTime);

        Destroy(go);
    }

}
[System.Serializable]
public class Weapon
{
    public string name = "AK47";
    public bool loopFire;
    public float fireRate = 0.2f;
    public int ammoCount = 30;
    public int ammoTotal = 90;
    public int startingMagSize = 30;
    public float damage;
}
[System.Serializable]
public enum WeaponType
{
    AK_47,
    DoubleBarrelShotgun,
    Rifle,
}