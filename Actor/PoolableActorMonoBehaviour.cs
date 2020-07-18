namespace LegendaryTools.Systems.Actor
{
    public class PoolableActorMonoBehaviour : ActorMonoBehaviour, IPoolable
    {
        public void OnConstruct()
        {
        }

        public void OnCreate()
        {
            Start();
        }

        public void OnRecycle()
        {
            OnDestroy();
        }
    }
}