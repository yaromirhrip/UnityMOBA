using System;
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

        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        /// <summary>Raised once, the moment this unit's health reaches zero.</summary>
        public event Action<Health, TeamId> OnDeath;

        private void Awake()
        {
            CurrentHealth = maxHealth;
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
                    // Keep the player object around (single session, no respawn needed
                    // for the prototype) but stop it from acting further.
                    var movement = GetComponent<PlayerMovement>();
                    if (movement != null) movement.enabled = false;
                    var combat = GetComponent<PlayerCombat>();
                    if (combat != null) combat.enabled = false;
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

        public float HealthPercent01 => maxHealth <= 0 ? 0f : (float)CurrentHealth / maxHealth;
    }
}
