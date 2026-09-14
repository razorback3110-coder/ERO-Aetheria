using UnityEngine;

namespace ERO.UI
{
    public sealed class EROGroupFinderHotkey : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.G;
        [SerializeField] private GameObject groupFinderPanel;

        public bool IsOpen => groupFinderPanel != null && groupFinderPanel.activeSelf;

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) Toggle();
        }

        public void Toggle()
        {
            if (groupFinderPanel == null) return;
            groupFinderPanel.SetActive(!groupFinderPanel.activeSelf);
        }

        public void Open() { if (groupFinderPanel != null) groupFinderPanel.SetActive(true); }
        public void Close() { if (groupFinderPanel != null) groupFinderPanel.SetActive(false); }
    }
}
