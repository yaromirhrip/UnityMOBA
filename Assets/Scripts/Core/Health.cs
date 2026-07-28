using System;
using System.Collections;
using UnityEngine;

namespace MobaPrototype
{
    /// <summary>
    /// Generic health/damage component used by every damageable unit in the prototype
    /// (player, lane creeps, towers and bases). Damage from the same team is ignored,
    /// which keeps friendly fire out of the prototype without extra checks elsewhere.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Setup")]
        public int maxHealth = 100;
        public TeamId team = TeamId.Ally;
        public UnitType unitType = UnitType.Creep;

        [Header("Player respawn (only used when Unit Type = Player)")]
        public float respawnDelay = 5f;
        public Transform respawnPoint; // optional - leave empty to respawn at the starting position

        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        public float RespawnTimeRemaining { get; private set; }

        /// <summary>Raised once, the moment this unit's health reaches zero.</summary>
        public event Action<Health, TeamId> OnDeath;

        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        public void TakeDamage(int amount, TeamId attackerTeam)
        {
            if (IsDead || amount <= 0) return;
            if (attackerTeam == team) return; // no friendly fire

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

            if (CurrentHealth <= 0)
            {
                IsDead = true;
                OnDeath?.Invoke(this, attackerTeam);

                if (GameManager.Instance != null)
                    GameManager.Instance.HandleUnitDeath(this, attackerTeam);

                HandleDeathPresentation();
            }
        }

        private void HandleDeathPresentation()
        {
            switch (unitType)
            {
                case UnitType.Creep:
                    // Creeps simply disappear from the field.
                    Destroy(gameObject);
                    break;

                case UnitType.Player:
                    // Freeze the hero briefly, then bring it back to life at the
                    // spawn point with full health instead of leaving it stuck dead.
                    var movement = GetComponent<PlayerMovement>();
                    if (movement != null) movement.enabled = false;
                    var combat = GetComponent<PlayerCombat>();
                    if (combat != null) combat.enabled = false;

                    StartCoroutine(RespawnPlayerRoutine());
                    break;

                case UnitType.Tower:
                case UnitType.Base:
                    // Structures stay in the hierarchy (for clarity) but stop
                    // functioning and visually signal they were destroyed.
                    var attack = GetComponent<TowerAttack>();
                    if (attack != null) attack.enabled = false;

                    var renderer = GetComponent<Renderer>();
                    if (renderer != null) renderer.material.color = Color.black;

                    var collider = GetComponent<Collider>();
                    if (collider != null) collider.enabled = false;
                    break;
            }
        }

        private IEnumerator RespawnPlayerRoutine()
        {
            RespawnTimeRemaining = respawnDelay;
            while (RespawnTimeRemaining > 0f)
            {
                yield return null;
                RespawnTimeRemaining -= Time.deltaTime;
            }

            Vector3 targetPos = respawnPoint != null ? respawnPoint.position : spawnPosition;
            Quaternion targetRot = respawnPoint != null ? respawnPoint.rotation : spawnRotation;

            var controller = GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            transform.SetPositionAndRotation(targetPos, targetRot);
            if (controller != null) controller.enabled = true;

            CurrentHealth = maxHealth;
            IsDead = false;

            var movement = GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = true;
            var combat = GetComponent<PlayerCombat>();
            if (combat != null) combat.enabled = true;
        }

        public float HealthPercent01 => maxHealth <= 0 ? 0f : (float)CurrentHealth / maxHealth;
    }
}
