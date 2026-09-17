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

        public bool TryAddItem(ItemData item)
        {
            if (Active == null || item == null || string.IsNullOrWhiteSpace(item.id) || item.quantity <= 0) return false;
            if (Active.inventory == null) Active.inventory = new System.Collections.Generic.List<ItemData>();

            var existing = Active.inventory.Find(i => i != null && string.Equals(i.id, item.id, StringComparison.Ordinal));
            if (existing != null)
            {
                existing.quantity = Math.Max(1, existing.quantity) + item.quantity;
            }
            else
            {
                Active.inventory.Add(item);
            }

            CharacterChanged?.Invoke();
            return true;
        }

        public bool TryRemoveItem(string itemId, int quantity = 1)
        {
            if (Active == null || Active.inventory == null || string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return false;
            var index = Active.inventory.FindIndex(i => i != null && string.Equals(i.id, itemId, StringComparison.Ordinal));
            if (index < 0) return false;

            var item = Active.inventory[index];
            if (item.quantity < quantity) return false;
            item.quantity -= quantity;
            if (item.quantity == 0) Active.inventory.RemoveAt(index);

            CharacterChanged?.Invoke();
            return true;
        }

        public bool TryEquipItem(string itemId)
        {
            if (Active == null || Active.inventory == null || string.IsNullOrWhiteSpace(itemId)) return false;
            var target = Active.inventory.Find(i => i != null && string.Equals(i.id, itemId, StringComparison.Ordinal));
            if (target == null) return false;

            for (int i = 0; i < Active.inventory.Count; i++)
            {
                var item = Active.inventory[i];
                if (item != null && item.equipped) item.equipped = false;
            }

            target.equipped = true;
            CharacterChanged?.Invoke();
            return true;
        }
    }
}
