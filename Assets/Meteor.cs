using System.Collections;
using Dalek;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    [Header("Meteor Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private GameObject trailEffect;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private AudioClip fallingSound;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float damage = 100f;

    private Transform target;
    private float speed;
    private float damageRadius;
    private LayerMask enemyLayer;
    private bool hasHit = false;

    private AudioSource audioSource;
    private GameObject trailInstance;

    public void Initialize(Transform targetEnemy, float meteorSpeed, float radius, LayerMask layer)
    {
        target = targetEnemy;
        speed = meteorSpeed;
        damageRadius = radius;
        enemyLayer = layer;

        // Set up audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Play falling sound
        if (fallingSound != null)
        {
            audioSource.clip = fallingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Create trail effect
        if (trailEffect != null)
        {
            trailInstance = Instantiate(trailEffect, transform.position, Quaternion.identity);
            trailInstance.transform.parent = transform;
        }

        // Start looking at target
        StartCoroutine(HomingRoutine());
    }

    void Update()
    {
        if (target == null || hasHit) return;

        // Move towards target
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Rotate for visual effect
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        transform.Rotate(Vector3.right * rotationSpeed * 0.5f * Time.deltaTime);

        // Check if reached target
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            Impact();
        }
    }

    IEnumerator HomingRoutine()
    {
        while (target != null && !hasHit)
        {
            // Update rotation to face target
            Vector3 direction = (target.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
            }

            yield return null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Vihu"))
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.CrushEnemy();
            }

            Impact();
            return;
        }

        if (other.CompareTag("Ground"))
        {
            Impact();
        }
    }


    void Impact()
    {
        if (hasHit) return;
        hasHit = true;

        // Stop trail effect
        if (trailInstance != null)
        {
            trailInstance.transform.parent = null;
            Destroy(trailInstance, 2f);
        }

        // Play impact sound
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position, 2f);
        }

        // Stop falling sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Create impact effect
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // Damage enemies in radius
        DamageEnemiesInRadius();

        // Destroy meteor
        Destroy(gameObject);
    }

    void DamageEnemiesInRadius()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius, enemyLayer);

        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Vihu"))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.CrushEnemy();
                }
                else
                {
                    // If no Enemy script, just destroy
                    Destroy(col.gameObject);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}