using UnityEngine;
using UnityEngine.UI;
using EternalRealmsOnline.V508;

namespace EternalRealmsOnline.V508
{
    public sealed class EROBilingualUIV508 : MonoBehaviour
    {
        [SerializeField] Text target;
        [SerializeField] string localizationKey;
        EROLocalizationV508 localization;
        void OnEnable()
        {
            localization = EROLocalizationV508.Instance;
            if (localization != null) localization.LanguageChanged += Refresh;
            Refresh(localization != null ? localization.CurrentLanguage : EROLanguage.English);
        }
        void OnDisable() { if (localization != null) localization.LanguageChanged -= Refresh; }
        void Refresh(EROLanguage _) { if (target != null && localization != null) target.text = localization.Get(localizationKey); }
    }
}
