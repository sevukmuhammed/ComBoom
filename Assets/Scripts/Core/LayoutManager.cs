using UnityEngine;

namespace ComBoom.Core
{
    [DefaultExecutionOrder(-10)]
    public class LayoutManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Transform gridTransform;
        [SerializeField] private Transform pieceSpawnerTransform;
        [SerializeField] private Transform[] slotTransforms;

        [Header("Layout Settings")]
        [SerializeField] private float scoreAreaPadding = 1.6f;
        [SerializeField] private float bottomPadding = 1.4f;
        [SerializeField] private float slotAreaHeight = 2.0f;

        private float gridTotalSize;

        private void Awake()
        {
            gridTotalSize = 8 * 0.7f; // 5.6 units
            AdjustLayout();
        }

        public void AdjustLayout()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            if (mainCamera == null) return;

            float screenAspect = (float)Screen.width / Screen.height;

            // Kamera: grid genisligi ekrana sigmali
            float requiredWidth = gridTotalSize + 0.6f;
            float orthoForWidth = (requiredWidth / 2f) / screenAspect;
            float targetOrtho = Mathf.Max(orthoForWidth, 5f);
            mainCamera.orthographicSize = targetOrtho;

            // Safe area hesapla (world units)
            Rect safeArea = Screen.safeArea;
            float safeWorldTop = targetOrtho - (Screen.height - safeArea.yMax) / Screen.height * targetOrtho * 2f;
            float safeWorldBottom = -targetOrtho + safeArea.yMin / Screen.height * targetOrtho * 2f;

            // Grid: ustten asagi (score alani icin bosluk birak)
            float gridY = safeWorldTop - scoreAreaPadding - gridTotalSize / 2f;
            if (gridTransform != null)
                gridTransform.position = new Vector3(0, gridY, 0);

            // Slot'lar: alttan yukari
            float slotsY = safeWorldBottom + bottomPadding + slotAreaHeight / 2f;
            if (pieceSpawnerTransform != null)
                pieceSpawnerTransform.position = new Vector3(0, slotsY, 0);

            // Slot dagitimi
            if (slotTransforms != null && slotTransforms.Length == 3)
            {
                float visibleWidth = targetOrtho * 2f * screenAspect;
                float slotSpacing = Mathf.Min(gridTotalSize / 3f, (visibleWidth * 0.9f) / 3f);

                for (int i = 0; i < 3; i++)
                {
                    if (slotTransforms[i] == null) continue;
                    float x = (i - 1) * slotSpacing;
                    slotTransforms[i].localPosition = new Vector3(x, 0, 0);
                    // Slot arka plan boyutu
                    float bgSize = slotSpacing * 0.85f;
                    slotTransforms[i].localScale = new Vector3(bgSize, bgSize, 1f);
                }
            }
        }
    }
}
