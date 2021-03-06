using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator zombieAnimator;
    private CapsuleCollider capsuleCollider;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minDist;
    [SerializeField] private Transform fromTransform;
    private float currentHealth;
    public float CurrentHealth { get { return currentHealth; } set { currentHealth = value; } }

    public int waveIndex = 0;
    public WaveSpawner[] waveSpawners;
    public bool isAttacking;

    private AgentSpeed speed;
    public AgentSpeed Speed => speed;

    public void Seek(Transform transf, AgentSpeed speed, float health)
    {
        playerTransform = transf;
        currentHealth = health;

        agent.speed = speed switch
        {
            AgentSpeed.Walk => 0.6f,
            AgentSpeed.Sprint => 1.8f,        
        };
        this.speed = speed;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        waveSpawners = FindObjectsOfType<WaveSpawner>();
    }

    void FixedUpdate()
    {
        agent.SetDestination(playerTransform.position);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {

            currentHealth -= WeaponController.Instance.EquippedWeapon.GetComponent<WeaponProperties>().weapon.damage;
            AudioManager.Instance.PlaySound(Sfx.Hit);
            collision.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;

            try
            {
                if (collision.transform.GetComponentInChildren<ParticleSystem>() != null)
                {
                    collision.transform.GetComponentInChildren<ParticleSystem>().Play();
                    Destroy(collision.gameObject, 4f);
                }
            }
            catch
            {
                // were okay.
            }

            if (currentHealth <= 0f)
            {
                capsuleCollider.enabled = false;
                agent.isStopped = true;
                zombieAnimator.SetTrigger("Death");
                WeaponController.Instance.PlayerCtrl.zombiesKilledThisRound++;
                Destroy(gameObject, 5f);
            }
        }
    }
    private void OnDestroy()
    {
        waveSpawners[waveIndex].OnZombieKilled(this);
    }

    // Update is called once per frame
    void Update()
    {
        float distToPlayer = Vector3.Distance(fromTransform.position, playerTransform.position);
        if (distToPlayer <= minDist)
        {
            zombieAnimator.SetFloat("Blend", 0f);
            zombieAnimator.SetTrigger("Attack");
            if (!IsInvoking(nameof(SetAttackingTrue)))
            {
                Invoke(nameof(SetAttackingTrue), 1f);
            }
        }
        else
        {
            if (agent.speed == 1.8f)
                zombieAnimator.SetFloat("Blend", 1f);
            if (agent.speed == 0.6f)
                zombieAnimator.SetFloat("Blend", 0.5f);

            zombieAnimator.SetTrigger("StopAttack");
            isAttacking = false;
            if (!IsInvoking(nameof(SetAttackingTrue)))
            {
                CancelInvoke(nameof(SetAttackingTrue));
            }
        }
    }
    private void SetAttackingTrue()
    {
        isAttacking = true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(fromTransform.position, minDist);
    }
}
