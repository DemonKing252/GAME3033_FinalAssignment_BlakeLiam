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

    [SerializeField] private Gradient roundGradiant;
    [SerializeField] private Gradient roundEndGradiant;
    [SerializeField] private Gradient roundStartGradiant;

    public MonoBehaviour[] LevelCompleteListeners => levelCompleteListeners;
    public List<ZombieController> Zombies => zombiesInScene;
    public void AddZombie(ZombieController z) { zombiesInScene.Add(z); }
    public int Level { get { return level; } set { level = value; } }

    public int kills = 0;

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

    public IEnumerator SpawnHorde()
    {
        
        int round = 1;
        while (true)
        {
            // Next round
            
            if (round != 1)
            {
                int previousRound = round - 1;

                currentWaveText.text = previousRound.ToString();
                float t = 0f;
                // round transition
                while (t < 5f)
                {
                    currentWaveText.color = roundGradiant.Evaluate(t % 1.0f);
                    t += Time.deltaTime;
                    yield return null;
                }

                // round end
                t = 0f;
                while (t < 1f)
                {
                    currentWaveText.color = roundEndGradiant.Evaluate(t);
                    t += Time.deltaTime;
                    yield return null;
                }

                yield return new WaitForSeconds(2f);
                currentWaveText.text = round.ToString();

                // round startup
                t = 0f;
                while (t < 2f)
                {
                    currentWaveText.color = roundStartGradiant.Evaluate(t/2f);
                    t += Time.deltaTime;
                    yield return null;
                }

            }
            else
                currentWaveText.text = round.ToString();
            
            
            float temp = 0;
            for (int i = 1; i <= round; i++)
            {
                if (i < 5)
                    temp += 4f;
                else if (i < 10)
                    temp += 12f;
                else
                    temp *= 1.01f;

            }
            int totalZombies = (int)temp;
            Debug.Log("total zombies should be: " + totalZombies);
            kills = 0;
            while (kills < totalZombies)
            {
                /* Actions
                   -------
                1. If zombie count is less then total kills
                
                */
                
                if (zombiesInScene.Count < 10 && (zombiesInScene.Count + kills) < totalZombies)
                {
                    var zombie = Instantiate(zombiePrefab, horde.GetRandomPointInSpawners(), Quaternion.identity);
                    zombie.Seek(playerTransform, AgentSpeed.Walk, 100f, this);
                    zombiesInScene.Add(zombie);
                    zombiesLeftText.text = kills.ToString() + "/" + totalZombies.ToString();
                }

                yield return new WaitForSeconds(5f);
            }
            Debug.Log("Ending round: " + round);
            yield return new WaitForSeconds(3f);
            round = 163;
        }


        //yield return new WaitForSeconds(horde.spawnStartDelay);
        //
        //if (waveIndex < horde.waves.Length)
        //{
        //    do
        //    {
        //        zombiesLeftText.text = horde.waves[waveIndex].numZombies.ToString();
        //        currentWaveText.text = (waveIndex + 1).ToString() + " / " + horde.waves.Length.ToString();
        //
        //        zombiesRemaining = (horde.waves[waveIndex].numZombies - zombiesInScene.Count) - WeaponController.Instance.PlayerCtrl.zombiesKilledThisRound;
        //        Debug.Log(zombiesRemaining);
        //
        //        while (zombiesRemaining > 0)
        //        {
        //            ZombieController zController = Instantiate(zombiePrefab, horde.GetRandomPointInSpawners(), Quaternion.identity);
        //            zController.Seek(playerTransform, horde.waves[waveIndex].agentSpeed, horde.waves[waveIndex].health);
        //
        //
        //            zombiesInScene.Add(zController);
        //
        //            zombiesRemaining--;
        //
        //            yield return new WaitForSeconds(horde.waves[waveIndex].delayBetweenSpawns);
        //        }
        //
        //        // Wait indefinately until the player kills all of the zombies, then start the next wave.
        //        while (zombiesInScene.Count != 0)
        //        {
        //            yield return null;
        //        }
        //        WeaponController.Instance.PlayerCtrl.zombiesKilledThisRound = 0;
        //        yield return new WaitForSeconds(horde.delayToNextWave);
        //
        //        waveIndex++;
        //        if (waveIndex <= horde.waves.Length - 1)
        //            currentWaveText.text = (waveIndex + 1).ToString() + " / " + horde.waves.Length.ToString();
        //
        //    } while (waveIndex < horde.waves.Length);
        //
        //    foreach (ILevelCompleteInterface listener in levelCompleteListeners)
        //        listener.OnLevelCompleted(level);
        //}
        //else
        //{
        //
        //    foreach (ILevelCompleteInterface listener in levelCompleteListeners)
        //        listener.OnLevelCompleted(level);
        //}


    }

    // Update is called once per frame
    void Update()
    {
    }
}
