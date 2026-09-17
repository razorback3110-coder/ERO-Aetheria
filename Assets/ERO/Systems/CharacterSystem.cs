using System;
using UnityEngine;
using ERO.Data;
namespace ERO.Systems
{
    public sealed class CharacterSystem : MonoBehaviour
    {
        public CharacterData Active { get; private set; }
        public event Action CharacterChanged;

        public CharacterData NewCharacter(string name, EROClass c, Gender g)
        {
            Active = new CharacterData { id = Guid.NewGuid().ToString("N"), name = name, classId = c, appearance = new Appearance { gender = g } };
            CharacterChanged?.Invoke();
            return Active;
        }

        public bool TryRestore(CharacterData character)
        {
            if (character == null) return false;
            Active = character;
            CharacterChanged?.Invoke();
            return true;
        }

        public void SetGender(Gender g)
        {
            if (Active == null || Active.appearance.gender == g) return;
            Active.appearance.gender = g;
            CharacterChanged?.Invoke();
        }

        public void SetAppearance(int face, int hair, int hairColor, int eyeColor, int skin)
        {
            if (Active == null) return;
            if (Active.appearance.face == face && Active.appearance.hair == hair &&
                Active.appearance.hairColor == hairColor && Active.appearance.eyeColor == eyeColor &&
                Active.appearance.skinTone == skin) return;

            Active.appearance.face = face;
            Active.appearance.hair = hair;
            Active.appearance.hairColor = hairColor;
            Active.appearance.eyeColor = eyeColor;
            Active.appearance.skinTone = skin;
            CharacterChanged?.Invoke();
        }
    }
}
