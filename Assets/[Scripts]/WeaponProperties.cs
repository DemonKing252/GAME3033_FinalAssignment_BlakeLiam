using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponProperties : MonoBehaviour
{
    public Transform gripIKTransform;
    public Transform muzzleFront;
    public Transform muzzleBack;

    public Weapon weapon;
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
public enum WeaponType
{
    AK_47,
    DoubleBarrelShotgun

}