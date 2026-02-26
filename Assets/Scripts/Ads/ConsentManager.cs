using UnityEngine;
using System;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Ump.Api;
#endif

namespace ComBoom.Ads
{
    public class ConsentManager
    {
        private static ConsentManager _instance;
        public static ConsentManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConsentManager();
                return _instance;
            }
        }

        public bool CanRequestAds { get; private set; }
        public bool IsResolved { get; private set; }
        public event Action OnConsentResolved;

        public void Initialize(Action<bool> onComplete)
        {
#if GOOGLE_MOBILE_ADS
            try
            {
                var requestParameters = new ConsentRequestParameters();

#if UNITY_EDITOR
                // Debug: EEA cografi bolge simule et (test icin)
                var debugSettings = new ConsentDebugSettings
                {
                    DebugGeography = DebugGeography.EEA,
                };
                requestParameters.ConsentDebugSettings = debugSettings;
#endif

                ConsentInformation.Update(requestParameters, (FormError updateError) =>
                {
                    if (updateError != null)
                    {
                        Debug.LogWarning($"[ConsentManager] Update error: {updateError.Message}");
                        // Fail-open: consent basarisiz olursa reklamlara izin ver
                        ResolveConsent(true, onComplete);
                        return;
                    }

                    ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
                    {
                        if (formError != null)
                        {
                            Debug.LogWarning($"[ConsentManager] Form error: {formError.Message}");
                        }

                        bool canRequest = ConsentInformation.CanRequestAds();
                        Debug.Log($"[ConsentManager] Consent resolved. CanRequestAds: {canRequest}");
                        ResolveConsent(canRequest, onComplete);
                    });
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConsentManager] Exception: {e.Message}");
                // Fail-open
                ResolveConsent(true, onComplete);
            }
#else
            Debug.Log("[ConsentManager] Google Mobile Ads SDK not installed. Skipping consent.");
            ResolveConsent(false, onComplete);
#endif
        }

        private void ResolveConsent(bool canRequestAds, Action<bool> onComplete)
        {
            CanRequestAds = canRequestAds;
            IsResolved = true;
            onComplete?.Invoke(canRequestAds);
            OnConsentResolved?.Invoke();
        }

        public void ResetConsent()
        {
#if GOOGLE_MOBILE_ADS
            ConsentInformation.Reset();
            CanRequestAds = false;
            IsResolved = false;
            Debug.Log("[ConsentManager] Consent reset.");
#endif
        }
    }
}
