using UnityEngine;

namespace MobaPrototype
{
    /// <summary>
    /// Homes toward its target and applies damage on impact. Spawned and
    /// initialised by TowerAttack.Init().
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        public float speed = 15f;
        public float maxLifetime = 3f;

        private Health target;
        private int damage;
        private TeamId attackerTeam;
        private float lifeTimer;

        public void Init(Health targetHealth, int damageAmount, TeamId team)
        {
            target = targetHealth;
            damage = damageAmount;
            attackerTeam = team;
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= maxLifetime || target == null || target.IsDead)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 aimPoint = target.transform.position + Vector3.up;
            Vector3 toTarget = aimPoint - transform.position;
            float distance = toTarget.magnitude;
            float step = speed * Time.deltaTime;

            if (step >= distance)
            {
                target.TakeDamage(damage, attackerTeam);
                GameManager.Instance?.AddDamage(attackerTeam, damage);
                Destroy(gameObject);
                return;
            }

            transform.position += toTarget.normalized * step;
        }
    }
}
