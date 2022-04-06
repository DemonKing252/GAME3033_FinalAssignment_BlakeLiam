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
    public Collider[] spawnColliders;
    public float delayBetweenSpawns = 3f;

}
[System.Serializable]
public class ZombieHorde
{
    public Wave[] waves;
    public float delayToNextWave;

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

    // Start is called before the first frame update
    void Start()
    {
        currentWave = horde.waves[waveIndex];
        StartCoroutine(SpawnHorde());
    }

    public void OnZombieKilled(ZombieController zombie)
    {
        zombiesInScene.Remove(zombie);
        zombiesLeftText.text = zombiesInScene.Count.ToString();
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
                ZombieController zController = Instantiate(zombiePrefab, spawnTransform.position, Quaternion.identity);
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
            currentWaveText.text = (waveIndex + 1).ToString() + " / " + horde.waves.Length.ToString();

        } while (waveIndex < horde.waves.Length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
