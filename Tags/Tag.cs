using UnityEngine;

namespace LegendaryTools.TagSystem
{
    [CreateAssetMenu(menuName = "Tools/Tag System/Tag")]
    public class Tag : ScriptableObject
    {
        public string Id;
        public string Name;
        public string Category;
        public string Description;
    }
}