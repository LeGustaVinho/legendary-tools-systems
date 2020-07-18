using System;
using LegendaryTools.Systems;

public enum RPGAttributeExample
{
    Health,
    Mana,
    Shield,
    AttackPower,
    SpellPower,
    MovimentSpeed,
    AttackSpeed
}

[Serializable]
public class EnumAttribute : Attribute<RPGAttributeExample>
{
    public EnumAttribute(AttributeSystem<RPGAttributeExample> parent, AttributeConfig<RPGAttributeExample> config) :
        base(parent, config)
    {
    }
}

[Serializable]
public class EnumAttributeSystem : AttributeSystem<RPGAttributeExample>
{
}