using UnityEngine;

namespace MobaPrototype
{
    /// <summary>
    /// Very small "auto target closest enemy in range" attack, triggered on left click.
    /// Keeps the hero's combat loop simple for the prototype while still feeding
    /// damage numbers into GameManager for the end-of-match stats.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerCombat : MonoBehaviour
    {
        public float attackRange = 3f;
        public int attackDamage = 15;
        public float attackCooldown = 0.6f;

        private Health myHealth;
        private float cooldownTimer;

        private void Awake()
        {
            myHealth = GetComponent<Health>();
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            if (cooldownTimer > 0f)
                cooldownTimer -= Time.deltaTime;

            if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1"))
                TryAttack();
        }

        private void TryAttack()
        {
            if (cooldownTimer > 0f || myHealth.IsDead) return;

            Health target = FindClosestEnemy();
            if (target == null) return;

            cooldownTimer = attackCooldown;
            target.TakeDamage(attackDamage, myHealth.team);
            GameManager.Instance?.AddDamage(myHealth.team, attackDamage);
        }

        private Health FindClosestEnemy()
        {
            Health best = null;
            float bestDist = attackRange;

            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
            foreach (var hit in hits)
            {
                Health hp = hit.GetComponent<Health>();
                if (hp == null || hp == myHealth || hp.IsDead || hp.team == myHealth.team)
                    continue;

                float dist = Vector3.Distance(transform.position, hp.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = hp;
                }
            }

            return best;
        }
    }
}
