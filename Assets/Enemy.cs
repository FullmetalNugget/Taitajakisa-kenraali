using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float fleeDistance = 10f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderTimer = 3f;

    [Header("Crush Settings")]
    [SerializeField] private GameObject crushEffect;
    [SerializeField] private AudioClip crushSound;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private bool isAlive = true;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;
        }

        timer = wanderTimer;
    }

    void Update()
    {
        if (!isAlive) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is in detection range
        if (distanceToPlayer <= detectionRange)
        {
            FleeFromPlayer();

            // Optional: Add animation for fleeing
            if (animator != null)
            {
                animator.SetBool("IsRunning", true);
            }
        }
        else
        {
            Wander();

            if (animator != null)
            {
                animator.SetBool("IsRunning", false);
            }
        }
    }

    void FleeFromPlayer()
    {
        Vector3 fleeDirection = transform.position - player.position;
        Vector3 fleePosition = transform.position + fleeDirection.normalized * fleeDistance;

        // Ensure the flee position is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void Wander()
    {
        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

            timer = 0;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isAlive) return;

        // Check if collided with player
        if (collision.gameObject.CompareTag("Player"))
        {
            Movement Movement = collision.gameObject.GetComponent<Movement>();

            // Check if player is not grounded (jumping/falling) and coming from above
            if (Movement != null && Movement.IsPlayerFalling())
            {
                // Check if player hit the top of the enemy
                if (collision.relativeVelocity.y < -0.5f)
                {
                    CrushEnemy();
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isAlive) return;

        // Alternative trigger detection (if using trigger collider)
        if (other.CompareTag("Player"))
        {
            Movement Movement = other.GetComponent<Movement>();

            if (Movement != null && Movement.IsPlayerFalling())
            {
                CrushEnemy();
            }
        }
    }

    void CrushEnemy()
    {
        isAlive = false;

        // Stop AI and movement
        if (agent != null)
            agent.isStopped = true;

        if (agent != null)
            agent.enabled = false;

        // Play crush animation
        if (animator != null)
        {
            animator.SetTrigger("Crush");
        }

        // Add score
        ScoreManager.instance?.AddScore(1);

        // Play effects
        if (crushEffect != null)
        {
            Instantiate(crushEffect, transform.position, Quaternion.identity);
        }

        if (crushSound != null)
        {
            AudioSource.PlayClipAtPoint(crushSound, transform.position);
        }

        // Optional: Destroy enemy after delay
        StartCoroutine(DestroyAfterDelay(2f));
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // Check if object has the "vihu" tag
    bool HasVihuTag(GameObject obj)
    {
        // Check all tags including "vihu"
        if (obj.CompareTag("vihu"))
            return true;

        // Alternatively, check for the tag in children or components
        return false;
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw flee distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
    }
}