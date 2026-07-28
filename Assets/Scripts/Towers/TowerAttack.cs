using UnityEngine;

namespace MobaPrototype
{
    /// <summary>
    /// Defensive tower: periodically finds the closest enemy unit in range and
    /// either fires a Projectile prefab at it, or (if no prefab assigned) applies
    /// damage directly - keeps the component usable even before a projectile
    /// prefab is wired up.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class TowerAttack : MonoBehaviour
    {
        public float range = 8f;
        public int damage = 10;
        public float fireInterval = 1f;
        public GameObject projectilePrefab;
        public Transform firePoint;

        private Health myHealth;
        private float timer;

        private void Awake()
        {
            myHealth = GetComponent<Health>();
        }

        private void Update()
        {
            if (myHealth.IsDead) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;

            Health target = FindTarget();
            if (target == null) return;

            timer = fireInterval;

            if (projectilePrefab != null)
            {
                Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1.5f;
                GameObject projectileGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                Projectile projectile = projectileGO.GetComponent<Projectile>();
                projectile?.Init(target, damage, myHealth.team);
            }
            else
            {
                target.TakeDamage(damage, myHealth.team);
                GameManager.Instance?.AddDamage(myHealth.team, damage);
            }
        }

        private Health FindTarget()
        {
            Health best = null;
            float bestDist = range;

            Collider[] hits = Physics.OverlapSphere(transform.position, range);
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
