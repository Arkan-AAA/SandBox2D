using System;
using UnityEngine;
using UnityEngine.AI;
using Satyr.Utils;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private State startingState;
    [SerializeField] private float roamingDistanceMax = 7f;
    [SerializeField] private float roamingDistanceMin = 3f;
    [SerializeField] private float roamingTimerMax = 2f;

    private NavMeshAgent navMeshAgent;
    private State state;
    private float roamingTime;
    private Vector3 roamPosition;
    private Vector3 startingPosition;
    private Animator animator;
    
    private const string IS_MOVING = "IsMoving";
    
    private enum State
    {
        Idle,
        Roaming,
        Chasing,
        Attacking,
        Death
    }
    
    private void Start()
    {
        startingPosition = transform.position;
    }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.updateRotation = false;
        
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.freezeRotation = true; 
        
        state = startingState;
        roamingTime = UnityEngine.Random.Range(0f, roamingTimerMax);
        
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        bool isMoving = navMeshAgent.velocity.magnitude > 0.1f;
        animator.SetBool(IS_MOVING, isMoving);
        
        switch (state)
        {
            case State.Idle:
                roamingTime -= Time.deltaTime;
                if (roamingTime < 0)
                {
                    Roaming();
                }
                break;

            case State.Roaming:
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.2f)
                {
                    state = State.Idle;
                    roamingTime = UnityEngine.Random.Range(1f, roamingTimerMax);
                }
                break;
            
            case State.Chasing:
                break;
            case State.Attacking:
                break;
            case State.Death:
                break;
        }
        
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.2f)
        {
            Roaming();
        }
    }
    
    private void Roaming()
    {
        roamPosition = GetRoamingPosition();
        navMeshAgent.SetDestination(roamPosition);
        ChangeFacingDirection(startingPosition, roamPosition);
        roamingTime = UnityEngine.Random.Range(1f, 3f);
    }
    
    private Vector3 GetRoamingPosition() {
        Vector3 randomDirection = Utils.GetRandomDir() * UnityEngine.Random.Range(roamingDistanceMin, roamingDistanceMax);
        Vector3 targetPosition = startingPosition + randomDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    private void ChangeFacingDirection(Vector3 sourcePosition, Vector3 targetPosition)
    {
        if (targetPosition.x < sourcePosition.x)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}


