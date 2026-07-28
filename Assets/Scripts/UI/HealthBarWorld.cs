using UnityEngine;
using UnityEngine.UI;

namespace MobaPrototype
{
    /// <summary>
    /// Builds a small world-space health bar above the unit entirely in code
    /// (same approach as UIManager for the HUD) and keeps it billboarded
    /// toward the camera and in sync with the Health component.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class HealthBarWorld : MonoBehaviour
    {
        [Header("Placement")]
        public Vector3 localOffset = new Vector3(0f, 1.3f, 0f);

        [Header("Size in world units")]
        public float worldWidth = 1.2f;
        public float worldHeight = 0.18f;

        private Health health;
        private Transform barPivot;
        private RectTransform fillRect;
        private Camera cachedCamera;

        private const float CanvasPixelWidth = 200f;
        private const float CanvasPixelHeight = 30f;

        private void Awake()
        {
            health = GetComponent<Health>();
            BuildBar();
        }

        private void BuildBar()
        {
            GameObject canvasGO = new GameObject("HealthBar", typeof(RectTransform), typeof(Canvas));
            canvasGO.transform.SetParent(transform, false);
            canvasGO.transform.localPosition = localOffset;

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(CanvasPixelWidth, CanvasPixelHeight);

            // Compensate for any non-uniform scale on the parent (e.g. bases are
            // scaled 4x/1x/4x) so the bar always renders at the intended world size.
            Vector3 parentScale = transform.lossyScale;
            canvasGO.transform.localScale = new Vector3(
                worldWidth / CanvasPixelWidth / Mathf.Max(parentScale.x, 0.0001f),
                worldHeight / CanvasPixelHeight / Mathf.Max(parentScale.y, 0.0001f),
                1f / Mathf.Max(parentScale.z, 0.0001f));

            GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(canvasGO.transform, false);
            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bgGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

            GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(canvasGO.transform, false);
            fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);

            bool isAlly = health.team == TeamId.Ally;
            fillGO.GetComponent<Image>().color = isAlly
                ? new Color(0.3f, 0.85f, 0.35f)
                : new Color(0.9f, 0.3f, 0.3f);

            barPivot = canvasGO.transform;
            cachedCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (barPivot == null) return;

            if (cachedCamera == null)
                cachedCamera = Camera.main;

            barPivot.position = transform.position + localOffset;

            if (cachedCamera != null)
                barPivot.rotation = cachedCamera.transform.rotation;

            float percent = Mathf.Clamp01(health.HealthPercent01);
            fillRect.anchorMax = new Vector2(percent, 1f);
        }
    }
}
