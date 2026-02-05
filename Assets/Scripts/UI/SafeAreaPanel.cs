using UnityEngine;

namespace ComBoom.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPanel : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            if (safeArea == lastSafeArea) return;
            lastSafeArea = safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
