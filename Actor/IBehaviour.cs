namespace LegendaryTools.Systems.Actor
{
    public interface IBehaviour : IComponent
    {
        bool Enabled { get; set; }
        bool IsActiveAndEnabled { get; }
    }
}