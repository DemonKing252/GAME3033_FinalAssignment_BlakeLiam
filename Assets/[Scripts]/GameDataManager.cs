using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Assertions;

[Serializable]
public class SVector3
{
    public float x;
    public float y;
    public float z;
    public SVector3(Vector3 vec)
    {
        this.x = vec.x;
        this.y = vec.y;
        this.z = vec.z;
    }
    public Vector3 ToVector()
    {
        return new Vector3(x, y, z);
    }
}
[Serializable]
public class SQuat
{
    public float x;
    public float y;
    public float z;
    public float w;
    public SQuat(Quaternion quat)
    {

        this.x = quat.x;
        this.y = quat.y;
        this.z = quat.z;
        this.w = quat.w;
    }
    public Quaternion ToQuat()
    {
        return new Quaternion(x, y, z, w);
    }
}

[Serializable]
public class SZombie
{
    public SVector3 position;
    public SQuat rotation;
    public int waveIndex;
    public float currentHealth;
}


[Serializable]
public class SWave
{
    public List<SZombie> zombies = new List<SZombie>();
}


[Serializable]
public class SaveGameData
{
    // Player data
    public SVector3 playerPos;
    public SQuat playerRot;

    public List<Weapon> weapons = new List<Weapon>();
    public WeaponType heldWeapon;

    public SWave wave1;
}




public class GameDataManager : MonoBehaviour
{
    private string path;

    public SaveGameData saveGame = new SaveGameData();
    public WaveSpawner[] waves;

    // Start is called before the first frame update
    void Start()
    {
        path = Application.dataPath + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "SaveGame.json";
        MenuController.Instance.onSaveGame += SaveGame;
        MenuController.Instance.onLoadGame += LoadGame;
    
    }
    private void OnDestroy()
    {
        MenuController.Instance.onSaveGame -= SaveGame;
        MenuController.Instance.onLoadGame -= LoadGame;
    }

    public void SaveGame()
    {
        //saveGame = new SaveGameData();
        saveGame.wave1.zombies.Clear();
        saveGame.weapons.Clear();

        saveGame.playerPos = new SVector3(WeaponController.Instance.transform.position);
        saveGame.playerRot = new SQuat(WeaponController.Instance.transform.rotation);

        //saveGame.weapons = WeaponController.Instance.GetAllWeapons();
        
        foreach(GameObject weapon in WeaponController.Instance.GetAllWeapons)
        {
            saveGame.weapons.Add(weapon.GetComponent<WeaponProperties>().weapon);
            saveGame.heldWeapon = WeaponController.Instance.weaponType;
            //sweapon.weaponType = weapon.GetComponent<WeaponProperties>(
        }

        
        foreach(ZombieController z in waves[0].Zombies)
        {
            SZombie zombie = new SZombie();
            zombie.position = new SVector3(z.transform.position);
            zombie.rotation = new SQuat(z.transform.rotation);
            zombie.waveIndex = z.waveIndex;
            zombie.currentHealth = z.CurrentHealth;

            saveGame.wave1.zombies.Add(zombie);
        }

        //saveGame.weaponType = WeaponController.Instance.weaponType;
        //saveGame.weapon = WeaponController.Instance.EquippedWeapon.GetComponent<WeaponProperties>().weapon;

        string data = JsonUtility.ToJson(saveGame, true);
        StreamWriter sw = new StreamWriter(path);
        sw.Write(data);
        sw.Close();
    }
    public void LoadGame()
    {
        StartCoroutine(Delay());
    }
    private IEnumerator Delay()
    {

        // wait one frame, give everything in the scene a chance to load in.
        yield return null;


        saveGame.weapons.Clear();
        saveGame.wave1.zombies.Clear();

        foreach (ZombieController z in waves[0].Zombies)
        {
            Destroy(z.gameObject);
        }
        waves[0].Zombies.Clear();

        StreamReader sr = new StreamReader(path);
        string raw = sr.ReadToEnd();
        saveGame = JsonUtility.FromJson<SaveGameData>(raw);
        sr.Close();

        WeaponController.Instance.PlayerCtrl.Controller.enabled = false;

        WeaponController.Instance.transform.position = saveGame.playerPos.ToVector();
        WeaponController.Instance.transform.rotation = saveGame.playerRot.ToQuat();
        WeaponController.Instance.PlayerCtrl.Controller.enabled = true;

        //WeaponController.Instance.EquippedWeapon.GetComponent<WeaponProperties>().weapon = saveGame.weapon;

        Assert.IsTrue(WeaponController.Instance.GetAllWeapons.Count == saveGame.weapons.Count, "Weapon size() doesn't match!");
        for (int i = 0; i < WeaponController.Instance.GetAllWeapons.Count; i++)
        {
            WeaponController.Instance.GetAllWeapons[i].GetComponent<WeaponProperties>().weapon = saveGame.weapons[i];
        }
        WeaponController.Instance.weaponType = saveGame.heldWeapon;
        WeaponController.Instance.SetActiveWeapon(saveGame.heldWeapon);
        WeaponController.Instance.RefreshWeaponProperties();


        for(int i = 0; i < saveGame.wave1.zombies.Count; i++)
        {
            ZombieController zombie = Instantiate(waves[0].zombiePrefab, saveGame.wave1.zombies[i].position.ToVector(), saveGame.wave1.zombies[i].rotation.ToQuat());
            zombie.waveIndex = saveGame.wave1.zombies[i].waveIndex;
            zombie.Seek(WeaponController.Instance.transform, AgentSpeed.Walk, saveGame.wave1.zombies[i].currentHealth);
            waves[0].AddZombie(zombie);
        }


        waves[1].gameObject.SetActive(false);
        waves[2].gameObject.SetActive(false);

        //Debug.Log(WeaponController.Instance.transform.position.ToString());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            SaveGame();
        }
        //p.position = new Vector3(0f, 100f, 0f);
        //Debug.Log(p.position);
    }
}
