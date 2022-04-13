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

    public int level;
    public int waveIdx;

}


[Serializable]
public class SaveGameData
{
    // Player data
    public SVector3 playerPos;
    public SQuat playerRot;

    public List<Weapon> weapons = new List<Weapon>();
    public WeaponType heldWeapon;

    public SWave[] waves = new SWave[3];
    public int hordeNumber = 1;
    public int zombiesKilledThisRound = 0;
    public int sceneIndex;
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

        //waves[1].gameObject.SetActive(false);
        //waves[2].gameObject.SetActive(false);
    
    }
    private void OnDestroy()
    {
        MenuController.Instance.onSaveGame -= SaveGame;
        MenuController.Instance.onLoadGame -= LoadGame;
    }

    public void SaveGame()
    {
        //saveGame = new SaveGameData();

        for (int i = 0; i < saveGame.waves.Length; i++)
        {
            saveGame.waves[i].zombies.Clear();
        }

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

                saveGame.waves[i].zombies.Add(zombie);
            }
        }

        saveGame.zombiesKilledThisRound = WeaponController.Instance.PlayerCtrl.zombiesKilledThisRound;
        saveGame.sceneIndex = WeaponController.Instance.PlayerCtrl.sceneIndex;
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

        //WeaponController.Instance.EquippedWeapon.GetComponent<WeaponProperties>().weapon = saveGame.weapon;

        Assert.IsTrue(WeaponController.Instance.GetAllWeapons.Count == saveGame.weapons.Count, "Weapon size() doesn't match!");
        for (int i = 0; i < WeaponController.Instance.GetAllWeapons.Count; i++)
        {
            WeaponController.Instance.GetAllWeapons[i].GetComponent<WeaponProperties>().weapon = saveGame.weapons[i];
        }
        WeaponController.Instance.weaponType = saveGame.heldWeapon;
        WeaponController.Instance.SetActiveWeapon(saveGame.heldWeapon);
        WeaponController.Instance.RefreshWeaponProperties();

        
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
                zombie.Seek(WeaponController.Instance.transform, AgentSpeed.Walk, saveGame.waves[j].zombies[i].currentHealth);
                waves[j].AddZombie(zombie);
            }
        }
        WeaponController.Instance.PlayerCtrl.zombiesKilledThisRound = saveGame.zombiesKilledThisRound;

        WeaponController.Instance.PlayerCtrl.sceneIndex = saveGame.sceneIndex;

        //waves[1].gameObject.SetActive(false);
        //waves[2].gameObject.SetActive(false);
        
        

        //Debug.Log(WeaponController.Instance.transform.position.ToString());
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
        //p.position = new Vector3(0f, 100f, 0f);
        //Debug.Log(p.position);
    }
}
