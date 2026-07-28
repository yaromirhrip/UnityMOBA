using UnityEngine;

namespace MobaPrototype
{
    /// <summary>
    /// Owns the single game session: match timer, kill/damage stats and the
    /// win condition (destroy the enemy base). Raises the match end so
    /// UIManager can show the results screen.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Bases (win condition)")]
        public Health allyBaseHealth;
        public Health enemyBaseHealth;

        [Header("Stats")]
        public int allyKills;
        public int enemyKills;
        public int allyDamageDealt;
        public int enemyDamageDealt;
        public float elapsedTime;

        public bool IsGameOver { get; private set; }
        public TeamId WinningTeam { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (allyBaseHealth != null)
                allyBaseHealth.OnDeath += (health, killer) => EndMatch(TeamId.Enemy);

            if (enemyBaseHealth != null)
                enemyBaseHealth.OnDeath += (health, killer) => EndMatch(TeamId.Ally);
        }

        private void Update()
        {
            if (IsGameOver) return;
            elapsedTime += Time.deltaTime;
        }

        /// <summary>Called by Health whenever any unit dies, to keep the kill count.</summary>
        public void HandleUnitDeath(Health died, TeamId killerTeam)
        {
            if (died.unitType != UnitType.Creep && died.unitType != UnitType.Player)
                return; // towers/bases affect the win condition, not the kill counter

            if (killerTeam == TeamId.Ally) allyKills++;
            else enemyKills++;
        }

        public void AddDamage(TeamId dealer, int amount)
        {
            if (dealer == TeamId.Ally) allyDamageDealt += amount;
            else enemyDamageDealt += amount;
        }

        private void EndMatch(TeamId winner)
        {
            if (IsGameOver) return;

            IsGameOver = true;
            WinningTeam = winner;

            if (UIManager.Instance != null)
                UIManager.Instance.ShowGameOver(this);
        }
    }
}
