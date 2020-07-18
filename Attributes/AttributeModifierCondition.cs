using System;

namespace LegendaryTools.Systems
{
    [Serializable]
    public class AttributeModifierCondition<T>
    {
        public T AttributeName;
        public AttributeModOperator Operator;
        public float Value;
    }
}