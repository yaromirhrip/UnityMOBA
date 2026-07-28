using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MobaPrototype
{
    /// <summary>
    /// Builds the whole HUD (timer / kill counters / player health) and the
    /// end-of-match results screen purely in code, on top of the "Canvas"
    /// GameObject already present in the scene hierarchy. Building the UI in
    /// code keeps the scene file small and avoids hand authoring dozens of
    /// nested RectTransforms for a one-day prototype.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("References")]
        public Transform canvasRoot;
        public Health playerHealth;

        private Text timerText;
        private Text allyKillsText;
        private Text enemyKillsText;
        private Text playerHealthText;

        private GameObject gameOverPanel;
        private Text resultText;
        private Text statsText;

        private void Awake()
        {
            Instance = this;
            EnsureEventSystem();
            Canvas canvas = EnsureCanvas();
            BuildHud(canvas.transform);
            BuildGameOverPanel(canvas.transform);
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

            timerText.text = FormatTime(GameManager.Instance.elapsedTime);
            allyKillsText.text = $"Ally kills: {GameManager.Instance.allyKills}";
            enemyKillsText.text = $"Enemy kills: {GameManager.Instance.enemyKills}";

            if (playerHealth != null)
            {
                playerHealthText.text = playerHealth.IsDead
                    ? $"Respawning in {Mathf.CeilToInt(playerHealth.RespawnTimeRemaining)}s"
                    : $"HP: {playerHealth.CurrentHealth}/{playerHealth.maxHealth}";
            }
        }

        public void ShowGameOver(GameManager gm)
        {
            gameOverPanel.SetActive(true);

            bool allyWon = gm.WinningTeam == TeamId.Ally;
            resultText.text = allyWon ? "ALLY TEAM WINS" : "ENEMY TEAM WINS";
            resultText.color = allyWon ? new Color(0.35f, 0.75f, 1f) : new Color(1f, 0.4f, 0.35f);

            statsText.text =
                $"Match time: {FormatTime(gm.elapsedTime)}\n" +
                $"Ally kills: {gm.allyKills}    Ally damage dealt: {gm.allyDamageDealt}\n" +
                $"Enemy kills: {gm.enemyKills}    Enemy damage dealt: {gm.enemyDamageDealt}";

            Time.timeScale = 0f;
        }

        private void RestartMatch()
        {
            Time.timeScale = 1f;
            Scene current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.FloorToInt(seconds);
            return $"{total / 60:00}:{total % 60:00}";
        }

        // ---------------------------------------------------------------
        // Runtime UI construction
        // ---------------------------------------------------------------

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;

            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private Canvas EnsureCanvas()
        {
            GameObject canvasGO = canvasRoot != null ? canvasRoot.gameObject : gameObject;

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasGO.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        private void BuildHud(Transform parent)
        {
            GameObject hud = CreatePanel(parent, "HUD", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, transparent: true);

            timerText = CreateText(hud.transform, "TimerText", "00:00", 28, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(200, 50));

            allyKillsText = CreateText(hud.transform, "AllyKillsText", "Ally kills: 0", 22, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20, -20), new Vector2(300, 40));

            enemyKillsText = CreateText(hud.transform, "EnemyKillsText", "Enemy kills: 0", 22, TextAnchor.UpperRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20, -20), new Vector2(300, 40));

            playerHealthText = CreateText(hud.transform, "PlayerHealthText", "HP: 100/100", 22, TextAnchor.LowerLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20, 20), new Vector2(300, 40));
        }

        private void BuildGameOverPanel(Transform parent)
        {
            gameOverPanel = CreatePanel(parent, "GameOverPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, transparent: false);
            gameOverPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            resultText = CreateText(gameOverPanel.transform, "ResultText", "", 48, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(800, 80));
            resultText.fontStyle = FontStyle.Bold;

            statsText = CreateText(gameOverPanel.transform, "StatsText", "", 24, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(800, 140));

            GameObject buttonGO = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGO.transform.SetParent(gameOverPanel.transform, false);
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.3f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.3f);
            buttonRect.sizeDelta = new Vector2(220, 60);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonGO.GetComponent<Image>().color = new Color(0.2f, 0.55f, 0.95f);
            buttonGO.GetComponent<Button>().onClick.AddListener(RestartMatch);

            CreateText(buttonGO.transform, "Label", "Restart", 26, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            gameOverPanel.SetActive(false);
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, bool transparent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image image = go.GetComponent<Image>();
            image.color = transparent ? new Color(0, 0, 0, 0) : Color.white;
            image.raycastTarget = !transparent;

            return go;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor anchor,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Text text = go.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;

            return text;
        }
    }
}
