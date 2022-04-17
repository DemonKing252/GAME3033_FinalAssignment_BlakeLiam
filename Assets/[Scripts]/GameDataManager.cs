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
public class STransform
{
    public SVector3 position;
    public SQuat rotation;
}

[Serializable]
public class SWeaponPickup : STransform
{
    public WeaponType weaponType;
}
[Serializable]
public class SHealthPack : STransform
{
    public float regen;
}
[Serializable]
public class SArmourPack : STransform
{
    public float regen;
}



[Serializable]
public class SZombie
{
    public SVector3 position;
    public SQuat rotation;
    public int waveIndex;
    public float currentHealth;
    public AgentSpeed agentSpeed;
}


[Serializable]
public class SWave
{
    public List<SZombie> zombies = new List<SZombie>();

    public int level;
    public int waveIdx;

}


[Serializable]
public class SaveGameData
{
    // Player data
    public SVector3 playerPos;
    public SQuat playerRot;
    public float health;
    public float armour;

    public List<Weapon> weapons = new List<Weapon>();
    public WeaponType heldWeapon;

    public SWave[] waves = new SWave[3];
    public int hordeNumber = 1;
    public int zombiesKilledThisRound = 0;
    public int sceneIndex;

    // Pickups
    public List<SWeaponPickup> weaponPickups = new List<SWeaponPickup>();
    public List<SHealthPack> healthPickups = new List<SHealthPack>();
    public List<SArmourPack> armourPickups = new List<SArmourPack>();

}




public class GameDataManager : MonoBehaviour
{
    private string path;

    public SaveGameData saveGame = new SaveGameData();
    public WaveSpawner[] waves;
    private static GameDataManager instance;
    public static GameDataManager Instance => instance;

    public WeaponPickup weaponPickupPrefab;
    public HealthPack healthPickupPrefab;
    public ArmourPack armourPickupPrefab;

    void Awake()
    {
        instance = this;
    }


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
        for (int i = 0; i < saveGame.waves.Length; i++)
        {
            saveGame.waves[i].zombies.Clear();
        }

        saveGame.weaponPickups.Clear();
        saveGame.healthPickups.Clear();
        saveGame.armourPickups.Clear();
        saveGame.weapons.Clear();

        saveGame.playerPos = new SVector3(WeaponController.Instance.transform.position);
        saveGame.playerRot = new SQuat(WeaponController.Instance.transform.rotation);

        foreach (GameObject weapon in WeaponController.Instance.GetAllWeapons)
        {
            saveGame.weapons.Add(weapon.GetComponent<WeaponProperties>().weapon);
            saveGame.heldWeapon = WeaponController.Instance.weaponType;
        }
        for (int i = 0; i < saveGame.waves.Length; i++)
        {

            saveGame.waves[i].level = waves[i].Level;
            saveGame.waves[i].waveIdx = waves[i].waveIndex;
        }

        for(int i = 0; i < waves.Length; i++)
        {
            foreach (ZombieController z in waves[i].Zombies)
            {
                SZombie zombie = new SZombie();
                zombie.position = new SVector3(z.transform.position);
                zombie.rotation = new SQuat(z.transform.rotation);
                zombie.waveIndex = z.waveIndex;
                zombie.currentHealth = z.CurrentHealth;
                zombie.agentSpeed = z.Speed;

                saveGame.waves[i].zombies.Add(zombie);
            }
        }
        saveGame.health = WeaponController.Instance.PlayerCtrl.Health;
        saveGame.armour = WeaponController.Instance.PlayerCtrl.Armour;

        saveGame.zombiesKilledThisRound = WeaponController.Instance.PlayerCtrl.zombiesKilledThisRound;
        saveGame.sceneIndex = WeaponController.Instance.PlayerCtrl.sceneIndex;


        PlayerController pController = WeaponController.Instance.PlayerCtrl;

        for (int i = 0; i < pController.WeaponPickup.Count; i++)
        {
            SWeaponPickup weapon = new SWeaponPickup();
            weapon.position = new SVector3(pController.WeaponPickup[i].transform.position);
            weapon.rotation = new SQuat(pController.WeaponPickup[i].transform.rotation);
            weapon.weaponType = pController.WeaponPickup[i].weaponType;
            saveGame.weaponPickups.Add(weapon);
        }

        for (int i = 0; i < pController.HealthPacks.Count; i++)
        {
            SHealthPack health = new SHealthPack();
            health.position = new SVector3(pController.HealthPacks[i].transform.position);
            health.rotation = new SQuat(pController.HealthPacks[i].transform.rotation);
            health.regen = pController.HealthPacks[i].HealthRegen;
            saveGame.healthPickups.Add(health);
        }
        for (int i = 0; i < pController.ArmourPack.Count; i++)
        {
            SArmourPack armour = new SArmourPack();
            armour.position = new SVector3(pController.ArmourPack[i].transform.position);
            armour.rotation = new SQuat(pController.ArmourPack[i].transform.rotation);
            armour.regen = pController.ArmourPack[i].ArmourRegen;
            saveGame.armourPickups.Add(armour);
        }

        string data = JsonUtility.ToJson(saveGame, true);
        StreamWriter sw = new StreamWriter(path);
        sw.Write(data);
        sw.Close();
    }
    public void Reset()
    {
        for (int i = 0; i < 3; i++)
        {
            // first one will be active
            if (i == 0)
            {
                waves[i].gameObject.SetActive(true);
            }
            else
            {
                waves[i].GetComponent<WaveSpawner>().CancelSpawn();
                waves[i].gameObject.SetActive(false);

            }

        }
    }

    public void LoadGame()
    {
        StartCoroutine(Delay());
    }
    private IEnumerator Delay()
    {
        int count;
        // wait one frame, give everything in the scene a chance to load in.
        yield return null;

        PlayerController p = WeaponController.Instance.PlayerCtrl;
        // ------------------------------
        // Clear all weapons from save game instance, and re add them (no duplicates!)
        saveGame.weapons.Clear();
        for (int i = 0; i < saveGame.waves.Length; i++)
        {
            saveGame.waves[i].zombies.Clear();
        }
        for (int i = 0; i < saveGame.waves.Length; i++)
        {
            foreach (ZombieController z in waves[i].Zombies)
            {
                Destroy(z.gameObject);
            }
        }
        for (int i = 0; i < saveGame.waves.Length; i++)
        {
            waves[i].Zombies.Clear();
        }
        count = FindObjectsOfType<ArmourPack>().Length;
        //Debug.Log("armours in list: " + p.ArmourPack.Count + ", armours in scene: " + count.ToString() + ", objects in save game instance:" + saveGame.armourPickups.Count);


        for (int i = 0; i < p.WeaponPickup.Count; i++)
        {
            Destroy(p.WeaponPickup[i].gameObject);
        }

        for (int i = 0; i < p.HealthPacks.Count; i++)
        {
            Destroy(p.HealthPacks[i].gameObject);
        }

        for (int i = 0; i < p.ArmourPack.Count; i++)
        {
            Destroy(p.ArmourPack[i].gameObject);
        }

        p.WeaponPickup.Clear();
        p.ArmourPack.Clear();
        p.HealthPacks.Clear();

        saveGame.weaponPickups.Clear();
        saveGame.healthPickups.Clear();
        saveGame.armourPickups.Clear();


        yield return null;

        count = FindObjectsOfType<ArmourPack>().Length;
        //Debug.Log("armours in list: " + p.ArmourPack.Count + ", armours in scene: " + count.ToString() + ", objects in save game instance:" + saveGame.armourPickups.Count);

        p = WeaponController.Instance.PlayerCtrl;

        StreamReader sr = new StreamReader(path);
        string raw = sr.ReadToEnd();
        saveGame = JsonUtility.FromJson<SaveGameData>(raw);
        sr.Close();


        for (int i = 0; i < 3; i++)
        {
            if (i == saveGame.sceneIndex)
            {
                waves[i].gameObject.SetActive(true);
            }
            else
            {
                waves[i].GetComponent<WaveSpawner>().CancelSpawn();
                waves[i].gameObject.SetActive(false);

            }
        }

        WeaponController.Instance.PlayerCtrl.Controller.enabled = false;
        WeaponController.Instance.transform.position = saveGame.playerPos.ToVector();
        WeaponController.Instance.transform.rotation = saveGame.playerRot.ToQuat();
        WeaponController.Instance.PlayerCtrl.Controller.enabled = true;

        Assert.IsTrue(WeaponController.Instance.GetAllWeapons.Count == saveGame.weapons.Count, "Weapon size() doesn't match!");
        for (int i = 0; i < WeaponController.Instance.GetAllWeapons.Count; i++)
        {
            WeaponController.Instance.GetAllWeapons[i].GetComponent<WeaponProperties>().weapon = saveGame.weapons[i];
        }
        WeaponController.Instance.weaponType = saveGame.heldWeapon;
        WeaponController.Instance.SetActiveWeapon(saveGame.heldWeapon);
        
        for (int i = 0; i < saveGame.waves.Length; i++)
        {
            waves[i].Level = saveGame.waves[i].level;
            waves[i].waveIndex = saveGame.waves[i].waveIdx;            
        }        

        for (int j = 0; j < waves.Length; j++)
        {
            for (int i = 0; i < saveGame.waves[j].zombies.Count; i++)
            {
                ZombieController zombie = Instantiate(waves[j].zombiePrefab, saveGame.waves[j].zombies[i].position.ToVector(), saveGame.waves[j].zombies[i].rotation.ToQuat());
                zombie.waveIndex = saveGame.waves[j].zombies[i].waveIndex;
                zombie.Seek(WeaponController.Instance.transform, saveGame.waves[j].zombies[i].agentSpeed, saveGame.waves[j].zombies[i].currentHealth);
                waves[j].AddZombie(zombie);
            }
        }
        WeaponController.Instance.PlayerCtrl.zombiesKilledThisRound = saveGame.zombiesKilledThisRound;
        WeaponController.Instance.PlayerCtrl.sceneIndex = saveGame.sceneIndex;
        WeaponController.Instance.PlayerCtrl.Health = saveGame.health;
        WeaponController.Instance.PlayerCtrl.Armour = saveGame.armour;
        WeaponController.Instance.RefreshWeaponProperties();

        for (int i = 0; i < saveGame.weaponPickups.Count; i++)
        {
            WeaponPickup weaponPickup = Instantiate(weaponPickupPrefab, saveGame.weaponPickups[i].position.ToVector(), saveGame.weaponPickups[i].rotation.ToQuat());
            weaponPickup.weaponType = saveGame.weaponPickups[i].weaponType;
            weaponPickup.SetWeaponEquipped(saveGame.weaponPickups[i].weaponType);

            p.WeaponPickup.Add(weaponPickup);
        }

        count = FindObjectsOfType<ArmourPack>().Length;
        for (int i = 0; i < saveGame.healthPickups.Count; i++)
        {
            HealthPack health = Instantiate(healthPickupPrefab, saveGame.healthPickups[i].position.ToVector(), saveGame.healthPickups[i].rotation.ToQuat());
            health.HealthRegen = saveGame.healthPickups[i].regen;
            p.HealthPacks.Add(health);
        }
        for (int i = 0; i < saveGame.armourPickups.Count; i++)
        {
            ArmourPack armour = Instantiate(armourPickupPrefab, saveGame.armourPickups[i].position.ToVector(), saveGame.armourPickups[i].rotation.ToQuat());
            armour.ArmourRegen = saveGame.armourPickups[i].regen;
            p.ArmourPack.Add(armour);
        }

        count = FindObjectsOfType<ArmourPack>().Length;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadGame();
        }
    }
}
