using UnityEngine;
using UnityEngine.AI;

public class EnemyAiTest : MonoBehaviour
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
    //bool PlayerHidden = true;
    //[SerializeField] private float DetectAngle = 45;
    //private Vector3 side1;
    //private Vector3 side2;
    private bool Chase;

    //States
    //public float sightRange;
    //public bool playerInSightRange;

    //Animation
    public Animator animator;
    public bool Chasing;
    public bool Attacking;

    //Audio
    [SerializeField] private AudioClip[] IdleAudios;
    [SerializeField] private AudioClip ChasingAudio;
    [SerializeField] private float SoundDelay = 10;
    private float SoundDelayTimer;

    public FieldOfView fov;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        fov = GetComponent<FieldOfView>();
        Home = transform.position;

    }

    private void Update()
    {
        //playerInSightRange = Vector3.Distance(transform.position, player.transform.position) <= sightRange;

        fov.FindVisibleTargets();

        //if (!playerInSightRange)
        //{
        //    Chasing = false;
        //    Patrolling();
        //}

        if (MoveCdTimer > 0)
        {
            MoveCdTimer -= Time.deltaTime;
        }

        if (SoundDelayTimer > 0)
        {
            SoundDelayTimer -= Time.deltaTime;
        }

        animator.SetFloat("Speed", agent.desiredVelocity.sqrMagnitude);
        animator.SetBool("Chasing", Chasing);
        animator.SetBool("Attacking", Attacking);

        //RaycastHit hit;
        //if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit, Mathf.Infinity))
        //{
        //    if(hit.transform == player.transform) 
        //    { 
        //    PlayerHidden = false;
        //    }
        //    else
        //    {
        //        PlayerHidden = true;
        //    }
        //}

        //side1 = player.transform.position - transform.position;
        //side2 = transform.forward;
        //float angle = Vector3.SignedAngle(side1, side2, Vector3.up);
        //if (angle < DetectAngle && angle > -DetectAngle && playerInSightRange && !PlayerHidden)
        //{
        //    Chase = true;
        //}

        if (fov.visible)
        { 
            Chase = true;
        }

        if (Chase)
        {
            Chasing = true;
            ChasePlayer();
        }
        else
        {
            Chasing = false;
            Patrolling();
        }
    }

    private void Patrolling()
    {
        if (Vector3.Distance(transform.position, Home) > HomeRadius)
        {
            agent.SetDestination(Home);
        }

        if (SoundDelayTimer <= 0)
        {
            SoundDelayTimer = SoundDelay;
            SoundFXManager.Instance.PlayRandomSoundFXClip(IdleAudios, transform);
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
        if (SoundDelayTimer <= 0)
        {
            SoundDelayTimer = SoundDelay;
            SoundFXManager.Instance.PlaySoundFXClip(ChasingAudio, transform);
        }

        agent.SetDestination(player.position);

        if (!fov.visible)
        {
            Chase = false;
        }
    }
    
}
