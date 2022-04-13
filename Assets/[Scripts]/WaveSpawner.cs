using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public enum AgentSpeed 
{
    Walk,
    Sprint
}


[System.Serializable]
public class Wave
{
    public float health;
    public int numZombies;
    public AgentSpeed agentSpeed;
    public float delayBetweenSpawns = 3f;

}
[System.Serializable]
public class ZombieHorde
{
    public float spawnStartDelay;
    public Wave[] waves;
    public Collider[] spawnColliders;
    public float delayToNextWave;

    // Choose a random spawner, and get a random point in it:
    public Vector3 GetRandomPointInSpawners()
    {
        int randIdx = Random.Range(0, spawnColliders.Length);
        Vector3 min = new Vector3(spawnColliders[randIdx].bounds.min.x, spawnColliders[randIdx].bounds.min.y, spawnColliders[randIdx].bounds.min.z);
        Vector3 max = new Vector3(spawnColliders[randIdx].bounds.max.x, spawnColliders[randIdx].bounds.max.y, spawnColliders[randIdx].bounds.max.z);

        Vector3 point = new Vector3(Random.Range(min.x, max.x), spawnColliders[randIdx].transform.position.y, Random.Range(min.z, max.z));
        return point;
    }
}


public class WaveSpawner : MonoBehaviour, ILevelCompleteInterface
{
    public int zombiesRemaining;
    public ZombieHorde horde;
    public int waveIndex = 0;
    public ZombieController zombiePrefab;

    // temp
    public Transform playerTransform;
    public Transform spawnTransform;

    [SerializeField] private TMP_Text currentWaveText;
    [SerializeField] private TMP_Text zombiesLeftText;
    [SerializeField] private List<ZombieController> zombiesInScene;
    [SerializeField] private MonoBehaviour[] levelCompleteListeners;
    [SerializeField] private int level = 1;

    public MonoBehaviour[] LevelCompleteListeners => levelCompleteListeners;
    public List<ZombieController> Zombies => zombiesInScene;
    public void AddZombie(ZombieController z) { zombiesInScene.Add(z); }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnHorde());
    }
    public void OnLevelCompleted(int level)
    {
        // Do nothing.
    }
    public void OnCloseDoors()
    {
        // This object is the NEXT wave spawner that will get turned on
        gameObject.SetActive(true);
    }
    public void OnZombieKilled(ZombieController zombie)
    {
        zombiesInScene.Remove(zombie);
        zombiesLeftText.text = zombiesInScene.Count.ToString();
    }
    public void OnNextScene()
    {   
        foreach (ILevelCompleteInterface listener in LevelCompleteListeners)
            listener.OnCloseDoors();
        
        // Were done with this spawner
        gameObject.SetActive(false);
    }

    public IEnumerator SpawnHorde()
    {
        yield return new WaitForSeconds(horde.spawnStartDelay);
        do
        {
            zombiesLeftText.text = horde.waves[waveIndex].numZombies.ToString();
            currentWaveText.text = (waveIndex + 1).ToString() + " / " + horde.waves.Length.ToString();

            zombiesRemaining = horde.waves[waveIndex].numZombies;
            do
            {
                ZombieController zController = Instantiate(zombiePrefab, horde.GetRandomPointInSpawners(), Quaternion.identity);
                zController.Seek(playerTransform, horde.waves[waveIndex].agentSpeed, horde.waves[waveIndex].health);
                
                
                zombiesInScene.Add(zController);

                zombiesRemaining--;

                yield return new WaitForSeconds(horde.waves[waveIndex].delayBetweenSpawns);
            } while (zombiesRemaining >= 1);

            // Wait indefinately until the player kills all of the zombies, then start the next wave.
            while (zombiesInScene.Count != 0)
            {
                yield return null;
            }
            yield return new WaitForSeconds(horde.delayToNextWave);

            waveIndex++;
            if (waveIndex <= horde.waves.Length - 1)
                currentWaveText.text = (waveIndex + 1).ToString() + " / " + horde.waves.Length.ToString();

        } while (waveIndex < horde.waves.Length);

        foreach (ILevelCompleteInterface listener in levelCompleteListeners)
            listener.OnLevelCompleted(level);

    }

    // Update is called once per frame
    void Update()
    {
    }
}
