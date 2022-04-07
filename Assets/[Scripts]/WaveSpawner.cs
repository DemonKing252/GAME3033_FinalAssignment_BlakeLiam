using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Wave
{
    public int numZombies;
    public float agentSpeed = 0.6f;
    public float delayBetweenSpawns = 3f;

}
[System.Serializable]
public class ZombieHorde
{
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


public class WaveSpawner : MonoBehaviour
{
    public float delayForFirstWave;

    public ZombieHorde horde;
    private Wave currentWave;
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
    [SerializeField] private WaveSpawner nextSceneSpawner;

    public MonoBehaviour[] LevelCompleteListeners
    {
        get { return levelCompleteListeners; }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting wave spawner..");
        currentWave = horde.waves[waveIndex];
        StartCoroutine(SpawnHorde());
    }
    //private void OnEnable()
    //{
    //    currentWave = horde.waves[waveIndex];
    //    StartCoroutine(SpawnHorde());
    //}

    public void OnZombieKilled(ZombieController zombie)
    {
        zombiesInScene.Remove(zombie);
        zombiesLeftText.text = zombiesInScene.Count.ToString();
    }
    public void OnNextScene()
    {
        nextSceneSpawner.gameObject.SetActive(true);
        this.gameObject.SetActive(false);

        foreach (ILevelCompleteInterface listener in LevelCompleteListeners)
            listener.OnCloseDoors();
    }

    public IEnumerator SpawnHorde()
    {
        do
        {
            zombiesLeftText.text = horde.waves[waveIndex].numZombies.ToString();
            currentWaveText.text = (waveIndex + 1).ToString() + " / " + horde.waves.Length.ToString();

            int zombiesRemaining = horde.waves[waveIndex].numZombies;
            do
            {
                ZombieController zController = Instantiate(zombiePrefab, horde.GetRandomPointInSpawners(), Quaternion.identity);
                zController.Seek(playerTransform);
                zombiesInScene.Add(zController);

                zombiesRemaining--;

                yield return new WaitForSeconds(horde.waves[waveIndex].delayBetweenSpawns);
            } while (zombiesRemaining >= 1);

            // Wait indefinately until the player killed all of the zombies, then start the next wave.
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
