using UnityEngine;
using UnityEngine.AI;

public class EnemyAi: MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    public float DistractRadius;
    public float DistractCd;
   [SerializeField] private float DistractCdTimer = 0;
    private Vector3 distractSource;


   



    public Vector3[] walkPoints;

    private Vector3 currentWalkPoint;

    public Vector3 nextWalkPoint;

    bool walkPointsSet = false;

    private int currentPoint = 0;

    //Patroling

    public Vector3 walkPoint;
    public bool walkPointSet = false;
    public float walkPointRange;
    public float MoveCd;
    [SerializeField] private float MoveCdTimer;
    [SerializeField] private bool NotMoving;
    Vector3 Home;
    public float HomeRadius;

    public float chaseCd;
    private float chaseCdTimer;

    [SerializeField] GameObject Question;
    [SerializeField] GameObject Exclamation;

    //Searching

    [SerializeField] float fullAwareness;
    [SerializeField] private float halfAwareness;
    [SerializeField] private float awareness;
    public bool searchingStarted = false;

    //Attacking

    [SerializeField] private float attackDistance;
    [SerializeField] private float attackCd;
    private float attackCdTimer;
    public float attackDamage;
    

    //Animation
    public Animator animator;
    public bool Chasing;
    public bool Attacking = false;

    //States
    [SerializeField] private bool Chase;
    [SerializeField] private bool Search;
    [SerializeField] private bool Patrol;
    [SerializeField] private bool Distract;

    //Audio
    [SerializeField] private AudioClip[] IdleAudios;
    [SerializeField] private AudioClip ChasingAudio;
    [SerializeField] private float SoundDelay = 10;
    private float SoundDelayTimer;
    private bool PlayedChaseSound;

    //FOV
    public FieldOfView fov;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        fov = GetComponent<FieldOfView>();
        Home = transform.position;
        halfAwareness = fullAwareness / 2;
        Question.SetActive(false);
        Exclamation.SetActive(false);
        walkPoints = new Vector3[4];
        //awareness = halfAwareness;

    }

    private void Update()
    {
     
        fov.FindVisibleTargets();

        if (MoveCdTimer > 0 && NotMoving)
        {
            MoveCdTimer -= Time.deltaTime;
        }

        if (SoundDelayTimer > 0)
        {
            SoundDelayTimer -= Time.deltaTime;
        }

        if (Distract && DistractCdTimer < DistractCd)
        {
            DistractCdTimer += Time.deltaTime;
        }

        animator.SetFloat("Speed", agent.desiredVelocity.sqrMagnitude);
        animator.SetBool("Chasing", Chasing);
        animator.SetBool("Attacking", Attacking);

        if (!Search && !Chase && !Distract)
        {
            Patrol = true;
        }
        else
        {
            Patrol = false;
        }

        if (fov.visible && !Chase)
        {
            Patrol = false;
            Distract = false;
            Search = true;
        }

        if (Chase)
        {
            Chasing = true;
            ChasePlayer();
        }
        if (Search)
        {
            Chasing = false;
            Searching();
        }
        if (Patrol)
        {
            Chasing = false;
            Patrolling();
        }
        if (Distract)
        {
            Distracted();
        }
       

        
    }

    public void Alert(Vector3 distractPoint)
    {
        if (Patrol)
        {
            Distract = true;
            Patrol = false;
            NotMoving = false;
            distractSource = distractPoint;
            agent.isStopped = true;
            transform.LookAt(distractPoint);    
            Question.SetActive(true);
        }
    }


  private void Distracted()
    {
      
          
            if (DistractCdTimer >= DistractCd)
            {
                
                agent.isStopped = false;
                agent.SetDestination(distractSource);
                if (Vector3.Distance(transform.position, distractSource) <= 2)
                {
                    Distract = false;
                    Patrol = true;
                    Question.SetActive(false);
                    DistractCdTimer = 0;
                }
            }
        

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Attacking = true;
            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            PlayerHealth health = Player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                if (!health.invincible)
                {
                    health.OnHit(attackDamage);
                }
            }
            else
            {
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        { Attacking = false; }
    }

    private void Searching()
    {

        if (!searchingStarted)
        {
            NotMoving = false;
            awareness = halfAwareness;          
            Question.SetActive(true);
            transform.LookAt(player);
            agent.isStopped = true;
            searchingStarted = true;
        }
       
        if (fov.visible)
        {
            awareness -= Time.deltaTime;
        }
        else
        {
            awareness += Time.deltaTime;
        }

        if (awareness <= 0)
        {
            Search = false;
            searchingStarted = false;
            agent.isStopped = false;
            Chase = true;
            Question.SetActive(false);
            Exclamation.SetActive(true);
        }

        if (awareness >= fullAwareness)
        {
            Search = false;
            searchingStarted = false;
            Patrol = true;
            agent.isStopped = false;
            chaseCdTimer = chaseCd;
            Question.SetActive(false);

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

        if (PlayedChaseSound)
        {
            PlayedChaseSound = false;
        }

        //if (!walkPointSet)
        //{
        //    SearchWalkPoint();
        //}

        //if (walkPointSet)
        //{
        //    if (MoveCdTimer > 0)
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        MoveCdTimer = MoveCd;

        //        agent.isStopped = false;

        //        agent.SetDestination(walkPoint);

        //        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //        if (distanceToWalkPoint.magnitude < 2f)
        //        {
        //            walkPointSet = false;
        //            agent.isStopped = true;
        //        }
        //    }
        //}
        //else { return; }

        if (!walkPointsSet)
        {
            walkPoints = SearchWalkPoint(walkPoints);
        }

        if (walkPointsSet)
        {  
                agent.SetDestination(walkPoints[currentPoint]);

                Vector3 distanceToWalkPoint = transform.position - walkPoints[currentPoint];

                if (distanceToWalkPoint.magnitude < 0.5f)
                {
                    NotMoving = true;
                    agent.isStopped = true;

                    if (MoveCdTimer > 0)
                    {
                        return;
                    }
                    else
                    {
                    NotMoving = false;
                    agent.isStopped = false;
                    MoveCdTimer = MoveCd;
                        if (currentPoint == walkPoints.Length - 1)
                        {
                            currentPoint = 0;
                        }

                        else
                        {
                            currentPoint++;
                        }
                    }
                
                }
        }
    }

    //private void SearchWalkPoint()
    //{
    //    do
    //    {
    //        float randomZ = Random.Range(-walkPointRange, walkPointRange);
    //        float randomX = Random.Range(-walkPointRange, walkPointRange);

    //        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

    //        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround) && Vector3.Distance(walkPoint, Home) <= HomeRadius && Vector3.Distance(transform.position, walkPoint) >= walkPointRange / 2)
    //        {
    //            walkPointSet = true;
    //        }
    //    }
    //    while (!walkPointSet);


    //}

    private Vector3[] SearchWalkPoint(Vector3[] WalkPoints)
    {
        bool walkPointValid;

        for (int i = 0; i < WalkPoints.Length; i++)
        {
            walkPointValid = false;

            do
            {
                if (i == 0)
                {
                    WalkPoints[i] = transform.position;
                    walkPointValid = true;
                }

                else
                {
                    float randomZ = Random.Range(-walkPointRange, walkPointRange);
                    float randomX = Random.Range(-walkPointRange, walkPointRange);

                    Vector3 walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

                    if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround) && Vector3.Distance(WalkPoints[i - 1], walkPoint) >= walkPointRange / 2 && Vector3.Distance(walkPoint, Home) <= HomeRadius)
                    {
                        WalkPoints[i] = walkPoint;
                        walkPointValid = true;
                    }
                
                }
            }
            while (!walkPointValid);
        }
        walkPointsSet = true;
        return WalkPoints;
    }


    private void ChasePlayer()
    {
        if (!PlayedChaseSound)
        {
            SoundFXManager.Instance.PlaySoundFXClip(ChasingAudio, transform);
            PlayedChaseSound = true;
        }

        chaseCdTimer -= Time.deltaTime;

        agent.SetDestination(player.position);


        if (!fov.visible && chaseCdTimer <= 0)
        {
            Chase = false;
            Search = true;
            Exclamation.SetActive(false);
        }
        else if (fov.visible && chaseCdTimer <= 0)
        {
            chaseCdTimer = chaseCd;
        }
    }
    
}
