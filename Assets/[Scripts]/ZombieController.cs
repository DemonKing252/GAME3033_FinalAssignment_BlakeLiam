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
    [SerializeField] private float startingHealth = 100f;
    private float currentHealth;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = startingHealth;
        //InvokeRepeating(nameof(SetDestination), 0f, 2f);
    }

    void FixedUpdate()
    {
        agent.SetDestination(playerTransform.position);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("bullet collision");
            Destroy(collision.gameObject);

            currentHealth -= 10f;
            if (currentHealth <= 0f)
            {
                capsuleCollider.enabled = false;
                agent.isStopped = true;
                zombieAnimator.SetTrigger("Death");
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        float distToPlayer = Vector3.Distance(fromTransform.position, playerTransform.position);
        if (distToPlayer <= minDist)
        {
            zombieAnimator.SetFloat("Blend", 0f);
            zombieAnimator.SetTrigger("Attack");
        }
        else
        {
            zombieAnimator.SetFloat("Blend", 1f);
            zombieAnimator.SetTrigger("StopAttack");
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(fromTransform.position, minDist);
    }
}
