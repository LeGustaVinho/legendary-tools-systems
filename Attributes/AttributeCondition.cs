using System;
using System.Collections.Generic;

namespace LegendaryTools.Systems
{
    [Serializable]
    public class AttributeCondition<T>
    {
        /// Lists all rules that must be met for the modifier to be applied to the attribute of the target entity
        public List<AttributeModifierCondition<T>> ModApplicationConditions = new List<AttributeModifierCondition<T>>();

        /// Designates which attribute this modifier will change
        public T TargetAttributeID;
    }
}