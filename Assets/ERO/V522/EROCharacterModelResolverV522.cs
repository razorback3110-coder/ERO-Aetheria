using UnityEngine;

namespace EternalRealmsOnline.V522
{
    /// <summary>
    /// Resolves ERO-owned Character Select preview models.
    /// V523 first loads the real ERO OBJ model from Resources; Boss Room is only a last-resort fallback.
    /// </summary>
    public static class EROCharacterModelResolverV522
    {
        static readonly string[] Names = { "Knight", "Assassin", "Ranger", "Mage", "Priest", "Monk", "Summoner", "Paladin" };

        public static GameObject Load(int classIndex, bool male)
        {
            classIndex = Mathf.Clamp(classIndex, 0, Names.Length - 1);
            string gender = male ? "Male" : "Female";
            string eroPath = "EROCharacters/" + Names[classIndex] + "_" + gender;

            // Unity imports OBJ files as GameObject assets. This is the actual ERO model,
            // not the Boss Room CharacterSelect prefab.
            var eroModel = Resources.Load<GameObject>(eroPath);
            if (eroModel != null) return eroModel;

            // Some Unity import configurations expose the model as a component asset.
            var transform = Resources.Load<Transform>(eroPath);
            if (transform != null) return transform.gameObject;

            // Last-resort compatibility fallback for old projects.
            string family = classIndex == 0 ? "Tank" : classIndex == 1 ? "Rogue" : classIndex == 2 ? "Archer" :
                             classIndex == 3 ? "Mage" : classIndex == 4 ? "Mage" : classIndex == 5 ? "Rogue" :
                             classIndex == 6 ? "Mage" : "Tank";
            return Resources.Load<GameObject>("EROCharacters/PlayerGraphics_" + family + "_" + (male ? "Boy" : "Girl") + "_CharacterSelect");
        }
    }
}
