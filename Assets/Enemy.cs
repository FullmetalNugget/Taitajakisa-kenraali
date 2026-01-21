using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Dalek
{
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
        private Animator animator;
        private Transform player;
        private bool isAlive = true;
        private float timer;

        [Header("Crush Detection")]
        [SerializeField] private GameObject headTriggerArea;

        void Start()
        {
            // Get required components
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            // Find player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("No GameObject with tag 'Player' found in the scene!");
            }

            // Create head trigger if not assigned
            if (headTriggerArea == null)
            {
                CreateHeadTrigger();
            }

            // Initialize wander timer
            timer = wanderTimer;
        }

        void CreateHeadTrigger()
        {
            // Create a trigger collider on top of the enemy
            headTriggerArea = new GameObject("HeadTrigger");
            headTriggerArea.transform.parent = transform;
            headTriggerArea.transform.localPosition = Vector3.up * 1f; // Adjust based on enemy height
            headTriggerArea.layer = LayerMask.NameToLayer("Ignore Raycast"); // Optional: prevent raycast issues

            BoxCollider triggerCollider = headTriggerArea.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(1f, 0.2f, 1f); // Thin trigger area on top

            // Add a rigidbody (kinematic) for better trigger detection
            Rigidbody triggerRb = headTriggerArea.AddComponent<Rigidbody>();
            triggerRb.isKinematic = true;
            triggerRb.useGravity = false;
        }

        void Update()
        {
            if (!isAlive) return;
            if (player == null) return;

            if (agent == null)
            {
                Debug.LogError("NavMeshAgent component is missing!");
                return;
            }

            if (!agent.enabled) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Check if player is in detection range
            if (distanceToPlayer <= detectionRange)
            {
                FleeFromPlayer();

                // Set animation
                if (animator != null)
                {
                    animator.SetBool("IsRunning", true);
                }
            }
            else
            {
                Wander();

                // Set animation
                if (animator != null)
                {
                    animator.SetBool("IsRunning", false);
                }
            }
        }

        void FleeFromPlayer()
        {
            if (player == null || agent == null || !agent.isActiveAndEnabled) return;

            Vector3 fleeDirection = (transform.position - player.position).normalized;
            Vector3 fleePosition = transform.position + fleeDirection * fleeDistance;

            // Use NavMesh to find a valid flee position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                // If no valid flee position found, try a closer position
                fleePosition = transform.position + fleeDirection * (fleeDistance / 2f);
                if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance / 2f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }

        void Wander()
        {
            if (agent == null || !agent.isActiveAndEnabled) return;

            timer += Time.deltaTime;

            if (timer >= wanderTimer)
            {
                // Check if agent is already moving to a destination
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
                    randomDirection += transform.position;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                    }
                }

                timer = 0f;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!isAlive) return;

            if (collision.gameObject.CompareTag("Player"))
            {
                Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
                if (playerRb == null) return;

                Movement movement = collision.gameObject.GetComponent<Movement>();

                // Simple crush detection: check if player is above and falling
                float playerHeightAboveEnemy = playerRb.position.y - transform.position.y;

                // Debug to help tune values
                Debug.Log($"Collision - PlayerY: {playerRb.position.y}, EnemyY: {transform.position.y}, Diff: {playerHeightAboveEnemy}, VelY: {playerRb.linearVelocity.y}");

                // Adjust 1.0f based on your enemy height (should be about half the enemy's height)
                if (playerHeightAboveEnemy > 0.5f && playerRb.linearVelocity.y < -0.5f)
                {
                    CrushEnemy();

                    // Bounce the player
                    playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 8f, playerRb.linearVelocity.z);
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!isAlive) return;

            // Only use trigger if it's from the head area (optional)
            if (other.gameObject == headTriggerArea) return;

            if (other.CompareTag("Player"))
            {
                Rigidbody playerRb = other.GetComponent<Rigidbody>();
                if (playerRb == null) return;

                // Simple check: if player is moving downward
                if (playerRb.linearVelocity.y < -1f)
                {
                    CrushEnemy();

                    // Bounce player
                    playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 8f, playerRb.linearVelocity.z);
                }
            }
        }

        public void CrushEnemy()
        {
            if (!isAlive) return;

            isAlive = false;

            // Stop AI
            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            // Play crush animation
            if (animator != null)
            {
                animator.SetTrigger("Crushed");
            }

            // Spawn effects
            if (crushEffect != null)
            {
                Instantiate(crushEffect, transform.position, Quaternion.identity);
            }

            // Play sound
            if (crushSound != null)
            {
                AudioSource.PlayClipAtPoint(crushSound, transform.position);
            }

            // Optional: Disable collider to prevent further collisions
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            // Destroy after delay
            StartCoroutine(DestroyAfterDelay(2f));

            // Add score (if you have a score manager)
            AddScore();
        }

        void AddScore()
        {
            // If you have a ScoreManager, call it here
            // Example: ScoreManager.instance.AddScore(1);
            ScoreManager.instance.AddScore(1);
            HungerManager.instance?.RestoreHunger();
        }

        IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
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
}