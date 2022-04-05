using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator zombieAnimator;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minDist;
    [SerializeField] private Transform fromTransform;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //InvokeRepeating(nameof(SetDestination), 0f, 2f);
    }

    void FixedUpdate()
    {
        agent.SetDestination(playerTransform.position);
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
