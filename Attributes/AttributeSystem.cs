using System.Collections.Generic;
using UnityEngine;

namespace LegendaryTools.Systems
{
    public class AttributeSystem<T>
    {
        private readonly Dictionary<T, Attribute<T>> attributesFinderCache = new Dictionary<T, Attribute<T>>();
        public List<Attribute<T>> Attributes = new List<Attribute<T>>();

        public void AddModifiers(AttributeSystem<T> attributeSystem)
        {
            List<Attribute<T>> allModifiers =
                attributeSystem.Attributes.FindAll(item => item.Type == AttributeType.Modifier);

            Attribute<T> currentAttribute = null;
            for (int i = 0; i < allModifiers.Count; i++)
            {
                for (int j = 0; j < allModifiers[i].TargetAttributeModifier.Count; j++)
                {
                    currentAttribute = GetAttributeByID(allModifiers[i].TargetAttributeModifier[j].TargetAttributeID);
                    if (currentAttribute != null)
                    {
                        currentAttribute.AddModifier(allModifiers[i], allModifiers[i].TargetAttributeModifier[j]);
                    }
                }
            }
        }

        public void RemoveModifiers(AttributeSystem<T> attributeSystem)
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                if (Attributes[i].Modifiers.Count > 0)
                {
                    Attributes[i].RemoveModifiers(attributeSystem);
                }
            }
        }

        public Attribute<T> GetAttributeByID(T attributeName)
        {
            if (attributesFinderCache.ContainsKey(attributeName))
            {
                return attributesFinderCache[attributeName];
            }

            Attribute<T> attr = null;
            foreach (Attribute<T> attribute in Attributes)
            {
                if (attribute.Config.ID.Equals(attributeName))
                {
                    attr = attribute;
                }
            }

            if (attr != null)
            {
                attributesFinderCache.Add(attributeName, attr);
                return attr;
            }

            Debug.LogError("[AttributeSystem:GetAttributeByID(" + attributeName + ") -> Not found");
            return null;
        }

        protected void updateAttributeCache()
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                attributesFinderCache.Add(Attributes[i].Config.ID, Attributes[i]);
            }
        }
    }
}