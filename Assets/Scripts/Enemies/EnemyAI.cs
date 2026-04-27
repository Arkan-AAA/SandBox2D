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
    
    private enum State
    {
        Idle,
        Roaming
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
        state = startingState;
        roamingTime = UnityEngine.Random.Range(0f, roamingTimerMax);
    }

    private void Update()
    {
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
        state = State.Roaming;
        roamingTime = UnityEngine.Random.Range(1f, 3f);
    }
    
    private Vector3 GetRoamingPosition()
    {
        Vector3 randomDirection = Utils.GetRandomDir() * UnityEngine.Random.Range(roamingDistanceMin, roamingDistanceMax);
        Vector3 targetPosition = startingPosition + randomDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }
}


