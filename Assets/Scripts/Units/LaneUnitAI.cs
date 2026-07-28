using UnityEngine;

namespace MobaPrototype
{
    /// <summary>
    /// Simple lane-pushing creep: walks straight down the lane toward the enemy base,
    /// and stops to fight the closest enemy unit if one comes into range.
    /// The same script is used for both the Ally and Enemy creep prefabs -
    /// only the team on the Health component differs.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class LaneUnitAI : MonoBehaviour
    {
        public float moveSpeed = 3f;
        public float attackRange = 2f;
        public int attackDamage = 8;
        public float attackInterval = 1f;
        public float detectionRange = 6f;

        private Health myHealth;
        private Transform destination;
        private float attackTimer;

        private void Awake()
        {
            myHealth = GetComponent<Health>();
        }

        /// <summary>Called by SpawnManager right after Instantiate.</summary>
        public void Init(Transform lanedestination)
        {
            destination = lanedestination;
        }

        private void Update()
        {
            if (myHealth.IsDead) return;

            Health target = FindClosestEnemy();
            if (target != null)
            {
                FaceTarget(target.transform.position);
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    target.TakeDamage(attackDamage, myHealth.team);
                    GameManager.Instance?.AddDamage(myHealth.team, attackDamage);
                    attackTimer = attackInterval;
                }
                return;
            }

            MoveTowardDestination();
        }

        private void MoveTowardDestination()
        {
            if (destination == null) return;

            Vector3 dir = destination.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude <= 0.05f) return;

            dir.Normalize();
            transform.position += dir * moveSpeed * Time.deltaTime;
            FaceTarget(transform.position + dir);
        }

        private void FaceTarget(Vector3 worldPoint)
        {
            Vector3 flatDir = worldPoint - transform.position;
            flatDir.y = 0f;
            if (flatDir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(flatDir);
        }

        private Health FindClosestEnemy()
        {
            Health best = null;
            float bestDist = detectionRange;

            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
            foreach (var hit in hits)
            {
                Health hp = hit.GetComponent<Health>();
                if (hp == null || hp == myHealth || hp.IsDead || hp.team == myHealth.team)
                    continue;

                float dist = Vector3.Distance(transform.position, hp.transform.position);
                bool inAttackRange = dist <= attackRange;

                // Prefer whichever enemy is already in attack range, otherwise
                // just track the closest one so the creep keeps approaching it.
                if (inAttackRange || best == null)
                {
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = hp;
                    }
                }
            }

            // Only "commit" to fighting if the closest enemy is actually reachable.
            return (best != null && Vector3.Distance(transform.position, best.transform.position) <= attackRange) ? best : null;
        }
    }
}
