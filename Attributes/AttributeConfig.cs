using System.Collections.Generic;
using UnityEngine;

namespace LegendaryTools.Systems
{
    public class AttributeConfig<T> : ScriptableObject
    {
        public bool AllowExceedCapacity;

        public List<string> FlagOptions = new List<string>();

        public bool HasCapacity;
        public T ID;

        public float MinCapacity;

        public Vector2 MinMaxValue;

        public float[] StackPenaults;

        public bool HasFlags => FlagOptions.Count > 0;

        public int FlagOptionEverythingValue => (int) Mathf.Pow(2, FlagOptions.Count) - 1;

        public bool HasStackPenault => StackPenaults != null && StackPenaults.Length > 0;
    }
}