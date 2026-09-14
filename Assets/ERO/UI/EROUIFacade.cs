using UnityEngine;
using ERO.Core;
using ERO.Data;

namespace ERO.UI
{
    public sealed class EROUIFacade : MonoBehaviour
    {
        public void SetLanguage(int index) => EROGameRoot.Instance.Systems.Localization.SetLanguage((Language)Mathf.Clamp(index, 0, 9));
        public void ChooseClass(int index)
        {
            var classId = (EROClass)Mathf.Clamp(index, 0, 6);
            if (EROGameRoot.Instance.Systems.Character.Active == null) EROGameRoot.Instance.Systems.Character.NewCharacter("Hero", classId, Gender.Male);
            else EROGameRoot.Instance.Systems.Character.Active.classId = classId;
        }
        public void SetGender(bool female) => EROGameRoot.Instance.Systems.Character.SetGender(female ? Gender.Female : Gender.Male);
        public void Save() => EROGameRoot.Instance.Systems.Save.Save(EROGameRoot.Instance.Systems.Character.Active);
    }
}
