using UnityEngine;
using System.Collections;

    public class MeteorSpawner : MonoBehaviour
    {
        [Header("Meteor Settings")]
        [SerializeField] private GameObject meteorPrefab;
        [SerializeField] private float spawnHeight = 28f;
        [SerializeField] private float meteorSpeed = 20f;
        [SerializeField] private float meteorDamageRadius = 3f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Cooldown Settings")]
        [SerializeField] private float cooldownTime = 10f;
        [SerializeField] private AudioClip summonSound;
        [SerializeField] private GameObject summonEffect;

        private float currentCooldown = 0f;
        private bool isOnCooldown = false;

        [Header("UI Settings")]
        [SerializeField] private UnityEngine.UI.Image cooldownUI;
        [SerializeField] private TMPro.TextMeshProUGUI cooldownText;

        void Update()
        {
            // Update cooldown
            if (isOnCooldown)
            {
                currentCooldown -= Time.deltaTime;

                if (currentCooldown <= 0f)
                {
                    isOnCooldown = false;
                    currentCooldown = 0f;
                }

                UpdateCooldownUI();
            }

            // Check for summon input
            if (Input.GetKeyDown(KeyCode.E) && !isOnCooldown)
            {
                SummonMeteor();
            }
        }

        void SummonMeteor()
        {
            // Find nearest enemy
            GameObject nearestEnemy = FindNearestEnemy();

            if (nearestEnemy == null)
            {
                Debug.Log("No enemies found to target!");
                return;
            }

            // Calculate spawn position (above the enemy)
            Vector3 spawnPosition = nearestEnemy.transform.position;
            spawnPosition.y = spawnHeight;

            // Instantiate meteor
            GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);

            // Set up meteor
            Meteor meteorScript = meteor.GetComponent<Meteor>();
            if (meteorScript != null)
            {
                meteorScript.Initialize(nearestEnemy.transform, meteorSpeed, meteorDamageRadius, enemyLayer);
            }
            else
            {
                // Add Meteor component if missing
                meteorScript = meteor.AddComponent<Meteor>();
                meteorScript.Initialize(nearestEnemy.transform, meteorSpeed, meteorDamageRadius, enemyLayer);
            }

            // Play summon effects
            if (summonSound != null)
            {
                AudioSource.PlayClipAtPoint(summonSound, transform.position);
            }

            if (summonEffect != null)
            {
                Instantiate(summonEffect, transform.position + Vector3.up * 1f, Quaternion.identity);
            }

            // Start cooldown
            StartCooldown();

            Debug.Log($"Summoned meteor targeting {nearestEnemy.name}");
        }

        GameObject FindNearestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Vihu");

            if (enemies.Length == 0) return null;

            GameObject nearestEnemy = null;
            float nearestDistance = Mathf.Infinity;

            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            return nearestEnemy;
        }

        void StartCooldown()
        {
            isOnCooldown = true;
            currentCooldown = cooldownTime;
        }

        void UpdateCooldownUI()
        {
            if (cooldownUI != null)
            {
                cooldownUI.fillAmount = 1f - (currentCooldown / cooldownTime);
            }

            if (cooldownText != null)
            {
                if (isOnCooldown)
                {
                    cooldownText.text = Mathf.Ceil(currentCooldown).ToString();
                }
                else
                {
                    cooldownText.text = "READY";
                }
            }
        }

        // Public method to check if ability is ready
        public bool IsAbilityReady()
        {
            return !isOnCooldown;
        }

        // Public method to get cooldown percentage (for UI)
        public float GetCooldownPercentage()
        {
            return 1f - (currentCooldown / cooldownTime);
        }
    }
