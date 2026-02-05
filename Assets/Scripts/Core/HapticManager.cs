using UnityEngine;
using System.Runtime.InteropServices;

namespace ComBoom.Core
{
    public static class HapticManager
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _TriggerImpactHaptic(int style);

        [DllImport("__Internal")]
        private static extern void _TriggerNotificationHaptic(int type);

        [DllImport("__Internal")]
        private static extern void _TriggerSelectionHaptic();
#endif

        private static bool enabled = true;
        public static bool IsEnabled => enabled;

        public static void SetEnabled(bool value)
        {
            enabled = value;
        }

        public static void ImpactLight()
        {
            if (!enabled) return;
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerImpactHaptic(0);
#endif
        }

        public static void ImpactMedium()
        {
            if (!enabled) return;
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerImpactHaptic(1);
#endif
        }

        public static void ImpactHeavy()
        {
            if (!enabled) return;
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerImpactHaptic(2);
#endif
        }

        public static void NotificationSuccess()
        {
            if (!enabled) return;
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerNotificationHaptic(0);
#endif
        }

        public static void NotificationWarning()
        {
            if (!enabled) return;
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerNotificationHaptic(1);
#endif
        }

        public static void NotificationError()
        {
            if (!enabled) return;
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerNotificationHaptic(2);
#endif
        }

        public static void SelectionChanged()
        {
            if (!enabled) return;
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerSelectionHaptic();
#endif
        }
    }
}
