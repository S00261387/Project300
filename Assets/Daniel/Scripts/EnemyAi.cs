using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;



  

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet = false;
    public float walkPointRange;
    public float MoveCd;
    private float MoveCdTimer;
    Vector3 Home;
    public float HomeRadius;
    public float PathUpdateDelay = 0.2f;
    private float PathUpdateDeadline;

    //States
    public float sightRange;
    public bool playerInSightRange;

    public Animator animator;

    public bool Chasing;
    public bool Attacking;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        Home = transform.position;
    }

    private void Update()
    {
        playerInSightRange = Vector3.Distance(transform.position, player.transform.position) <= sightRange;

        if (!playerInSightRange)
        { 
            Chasing = false;
            Patrolling(); 
        }

        if (playerInSightRange)
        {
            Chasing = true;
            ChasePlayer(); 
        }

        if (MoveCdTimer > 0)
        {
            MoveCdTimer -= Time.deltaTime;
        }

        animator.SetFloat("Speed", agent.desiredVelocity.sqrMagnitude);
        animator.SetBool("Chasing", Chasing);
        animator.SetBool("Attacking", Attacking);
    }

    private void Patrolling()
    {
        if (Vector3.Distance(transform.position, Home) > HomeRadius)
        {
            agent.SetDestination(Home);
        }


        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            if (MoveCdTimer > 0)
            {
                return;
            }
            else
            {
                MoveCdTimer = MoveCd;

                agent.SetDestination(walkPoint);

                Vector3 distanceToWalkPoint = transform.position - walkPoint;

                if (distanceToWalkPoint.magnitude < 1f)
                    walkPointSet = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == player)
        {
            Attacking = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == player)
        {
            Attacking = false;
        }
    }

    private void SearchWalkPoint()
        {
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround) && Vector3.Distance(walkPoint, Home) <= HomeRadius && Vector3.Distance(transform.position, walkPoint) >= walkPointRange/2)
                walkPointSet = true;

        }

        private void ChasePlayer()
        {
        if (Time.deltaTime >= PathUpdateDeadline)
        {
            PathUpdateDeadline = Time.deltaTime + PathUpdateDelay;
        }
            agent.SetDestination(player.position);
        }
    
}
