using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegendaryTools.Systems
{
    public enum AttributeType
    {
        Attribute,
        Modifier
    }

    public enum AttributeModOperator
    {
        Equals,
        Greater,
        Less,
        GreaterOrEquals,
        LessOrEquals,
        NotEquals,
        ContainsFlag,
        NotContainsFlag
    }

    public enum AttributeFlagModOperator
    {
        AddFlag,
        RemoveFlag,
        Set
    }

    public class Attribute<T>
    {
        /// Lists all modifiers that are currently changing this attribute
        public readonly List<Attribute<T>> Modifiers = new List<Attribute<T>>();

        public float Capacity;
        public float Factor = 0;

        public AttributeFlagModOperator FlagOperator = AttributeFlagModOperator.AddFlag;

        public float Flat;

        /// List the conditions that this modifier needs to find to be applied
        public List<AttributeCondition<T>> TargetAttributeModifier = new List<AttributeCondition<T>>();

        public AttributeType Type = AttributeType.Attribute;

        public Attribute(AttributeSystem<T> parent, AttributeConfig<T> config)
        {
            Parent = parent;
            Config = config;
        }

        public AttributeSystem<T> Parent { get; protected set; }
        public AttributeConfig<T> Config { get; protected set; }

        /// Returns the current value of the attribute taking into account all modifiers currently applied
        public float Value => getValueWithModifiers();

        private bool CanUseCapacity => HasCapacity && Type == AttributeType.Attribute && !HasFlags;

        private bool HasFlags => Config?.HasFlags ?? false;

        private bool HasCapacity => Config?.HasCapacity ?? false;

        public event Action<Attribute<T>> OnAttributeModAdd;
        public event Action<Attribute<T>> OnAttributeModRemove;
        public event Action<float, float> OnAttributeCapacityChange;

        public void AddModifier(Attribute<T> attribute, AttributeCondition<T> modifier = null)
        {
            if (!ModApplicationCanBeAccepted(attribute, modifier))
            {
                return;
            }

            Modifiers.Add(attribute);

            OnAttributeModAdd?.Invoke(attribute);
        }

        public void RemoveModifier(Attribute<T> attribute)
        {
            if (!Modifiers.Contains(attribute))
            {
                return;
            }

            Modifiers.Remove(attribute);

            OnAttributeModRemove?.Invoke(attribute);
        }

        public void RemoveModifiers(AttributeSystem<T> attributeSystem)
        {
            List<Attribute<T>> modsToRemove = Modifiers.FindAll(item => item.Parent == attributeSystem);

            Modifiers.RemoveAll(item => item.Parent == attributeSystem);

            for (int i = 0; i < 0; i++)
            {
                OnAttributeModRemove?.Invoke(modsToRemove[i]);
            }
        }

        public bool CapacityAdd(float valueToAdd)
        {
            if (!CanUseCapacity)
            {
                return false;
            }

            if (!Config.AllowExceedCapacity && !(Capacity + valueToAdd <= Value))
            {
                return false;
            }

            Capacity += valueToAdd;
            OnAttributeCapacityChange?.Invoke(Capacity, Capacity - valueToAdd);

            return true;
        }

        public bool CapacityRemove(float valueToRemove)
        {
            if (!CanUseCapacity)
            {
                return false;
            }

            if (!(Capacity - valueToRemove >= Config.MinCapacity))
            {
                return false;
            }

            Capacity -= valueToRemove;
            OnAttributeCapacityChange?.Invoke(Capacity, Capacity + valueToRemove);

            return true;
        }

        /// Checks whether the mod can be applied to the target entity
        public bool ModApplicationCanBeAccepted(Attribute<T> attribute, AttributeCondition<T> modifier = null)
        {
            if (modifier == null)
            {
                modifier = attribute.TargetAttributeModifier.Find(item => item.TargetAttributeID.Equals(Config.ID));
            }

            if (modifier != null)
            {
                Attribute<T> currentAttribute = null;
                for (int i = 0; i < modifier.ModApplicationConditions.Count; i++)
                {
                    currentAttribute = Parent.GetAttributeByID(modifier.ModApplicationConditions[i].AttributeName);
                    switch (modifier.ModApplicationConditions[i].Operator)
                    {
                        case AttributeModOperator.Equals:
                            if (!(currentAttribute.Value == modifier.ModApplicationConditions[i].Value))
                            {
                                return false;
                            }

                            break;
                        case AttributeModOperator.Greater:
                            if (!(currentAttribute.Value > modifier.ModApplicationConditions[i].Value))
                            {
                                return false;
                            }

                            break;
                        case AttributeModOperator.Less:
                            if (!(currentAttribute.Value < modifier.ModApplicationConditions[i].Value))
                            {
                                return false;
                            }

                            break;
                        case AttributeModOperator.GreaterOrEquals:
                            if (!(currentAttribute.Value >= modifier.ModApplicationConditions[i].Value))
                            {
                                return false;
                            }

                            break;
                        case AttributeModOperator.LessOrEquals:
                            if (!(currentAttribute.Value <= modifier.ModApplicationConditions[i].Value))
                            {
                                return false;
                            }

                            break;
                        case AttributeModOperator.NotEquals:
                            if (!(currentAttribute.Value != modifier.ModApplicationConditions[i].Value))
                            {
                                return false;
                            }

                            break;
                        case AttributeModOperator.ContainsFlag:
                            if (!FlagUtil.Has(currentAttribute.Value, modifier.ModApplicationConditions[i].Value))
                            {
                                return false;
                            }

                            break;
                        case AttributeModOperator.NotContainsFlag:
                            if (FlagUtil.Has(currentAttribute.Value, modifier.ModApplicationConditions[i].Value))
                            {
                                return false;
                            }

                            break;
                    }
                }

                return true;
            }

            return false;
        }

        /// Returns the current value of the attribute taking into account all modifiers currently applied
        private float getValueWithModifiers()
        {
            if (Config == null)
            {
                return 0;
            }

            if (HasFlags)
            {
                float currentFlag = Flat;
                for (int i = 0; i < Modifiers.Count; i++)
                {
                    switch (Modifiers[i].FlagOperator)
                    {
                        case AttributeFlagModOperator.AddFlag:
                            currentFlag = FlagUtil.Add(currentFlag, Modifiers[i].Flat);
                            break;
                        case AttributeFlagModOperator.RemoveFlag:
                            currentFlag = FlagUtil.Remove(currentFlag, Modifiers[i].Flat);
                            break;
                        case AttributeFlagModOperator.Set:
                            currentFlag = Modifiers[i].Flat;
                            break;
                    }
                }

                return currentFlag;
            }

            Modifiers.Sort((a, b) => -1 * a.Factor.CompareTo(b.Factor)); //descending sort
            float totalFlat = 0;
            float totalFactor = 0;
            for (int i = 0; i < Modifiers.Count; i++)
            {
                totalFlat += Modifiers[i].Flat;

                if (Config.HasStackPenault)
                {
                    totalFactor += Modifiers[i].Factor *
                                   Config.StackPenaults[
                                       Mathf.Clamp(i, 0, Config.StackPenaults.Length - 1)];
                }
                else
                {
                    totalFactor += Modifiers[i].Factor;
                }
            }

            return Mathf.Clamp((Flat + totalFlat) * (1 + Factor + totalFactor),
                Config.MinMaxValue.x, Config.MinMaxValue.y);
        }
    }
}