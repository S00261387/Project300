using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator anim;
    public FieldOfView fov;
    public Transform player;

    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;

    [Header("Timers")]
    public float suspicionTime = 2f; // how long enemy looks before chasing
    public float surpriseDuration = 1.2f; // time to play the surprise animation before chasing
    public float lookAroundTime = 3f; // how long to look around after losing player

    private float suspicionTimer = 0f;
    private float lookAroundTimer = 0f;

    private bool playerVisible = false;
    private bool isSuspicious = false;
    private bool isChasing = false;
    private bool isLookingAround = false;
    private bool hasPlayedSurprise = false;

    private Vector3 lastKnownPosition;

    public float patrolRadius = 10f;

    public List<Vector3> patrolPoints;
    private int patrolIndex = 0;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (anim == null)
            anim = GetComponent<Animator>();
        if (fov == null)
            fov = GetComponentInChildren<FieldOfView>();

        // Automatically find the player if not assigned manually
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("EnemyAi: No Player found in scene! Make sure your Player has the 'Player' tag.");
        }

        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            GoToNextPatrolPoint();
        }
        else
        {
            PickNewPatrolPoint();
        }
    }

    void Update()
    {
        fov.FindVisibleTargets();
        playerVisible = fov.visible;

        // === PLAYER IN SIGHT ===
        if (playerVisible)
        {
            lastKnownPosition = player.position;

            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 3f);

            if (!isChasing && !hasPlayedSurprise)
            {
                // build suspicion
                isSuspicious = true;
                suspicionTimer += Time.deltaTime;
                agent.isStopped = true;
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", false);
                anim.SetTrigger("LookAround"); // use your "look around" animation

                if (suspicionTimer >= suspicionTime)
                {
                    // suspicion full, play surprise animation
                    suspicionTimer = 0f;
                    hasPlayedSurprise = true;
                    StartCoroutine(PlaySurpriseThenChase());
                }
            }
            else if (isChasing)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }
        else // === PLAYER NOT VISIBLE ===
        {
            if (isChasing)
            {
                // Go to last known position
                if (Vector3.Distance(transform.position, lastKnownPosition) > 0.5f)
                {
                    agent.isStopped = false;
                    agent.SetDestination(lastKnownPosition);
                }
                else
                {
                    // start look around phase
                    isChasing = false;
                    isLookingAround = true;
                    agent.isStopped = true;
                    lookAroundTimer = lookAroundTime;
                    anim.SetBool("isRunning", false);
                    anim.SetTrigger("LookAround");
                }
            }
            else if (isLookingAround)
            {
                // look around for a few seconds
                lookAroundTimer -= Time.deltaTime;
                if (lookAroundTimer <= 0f)
                {
                    isLookingAround = false;
                    agent.isStopped = false;
                    agent.speed = walkSpeed;
                    anim.SetBool("isWalking", true);
                    anim.SetBool("isRunning", false);
                    anim.ResetTrigger("LookAround");
                    GoToNextPatrolPoint();
                }
            }
            else if (isSuspicious)
            {
                // lost the player during suspicion phase: drop suspicion and resume patrol
                isSuspicious = false;
                suspicionTimer = 0f;
                agent.isStopped = false;
                GoToNextPatrolPoint();
            }
            else
            {
                // normal patrol
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    GoToNextPatrolPoint();
            }
        }

        // Update animation speed value (for Blend Trees etc.)
        anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    System.Collections.IEnumerator PlaySurpriseThenChase()
    {
        agent.isStopped = true;
        anim.SetTrigger("Surprise"); // play surprise animation
        yield return new WaitForSeconds(surpriseDuration);

        // start chasing
        isChasing = true;
        hasPlayedSurprise = false;
        agent.isStopped = false;
        agent.speed = runSpeed;
        anim.ResetTrigger("LookAround");
        anim.ResetTrigger("Surprise");
        anim.SetBool("isRunning", true);
    }

    public void InitializePatrol(List<Vector3> points)
    {
        patrolPoints = points;
        patrolIndex = 0;
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            PickNewPatrolPoint();
            return;
        }

        patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
        Vector3 nextPoint = patrolPoints[patrolIndex];

        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(nextPoint, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            agent.isStopped = false;
            agent.speed = walkSpeed;
            agent.SetDestination(nextPoint);
            anim.SetBool("isWalking", true);
            anim.SetBool("isRunning", false);
        }
        else
        {
            PickNewPatrolPoint();
        }

        // reset state
        isSuspicious = false;
        suspicionTimer = 0f;
        isChasing = false;
        isLookingAround = false;
        hasPlayedSurprise = false;
    }

    void PickNewPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, patrolRadius, NavMesh.AllAreas))
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                agent.isStopped = false;
                agent.speed = walkSpeed;
                agent.SetDestination(hit.position);
                anim.SetBool("isWalking", true);
                anim.SetBool("isRunning", false);
                anim.ResetTrigger("LookAround");
                anim.ResetTrigger("Surprise");
            }
        }

        // reset state
        isSuspicious = false;
        suspicionTimer = 0f;
        isChasing = false;
        isLookingAround = false;
        hasPlayedSurprise = false;
    }
}
