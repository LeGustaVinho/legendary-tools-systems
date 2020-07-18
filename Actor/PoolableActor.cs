using UnityEngine;

namespace LegendaryTools.Systems.Actor
{
    public class PoolableActor<TClass, TBehaviour> : Actor<TClass, TBehaviour>
        where TBehaviour : PoolableActorMonoBehaviour
    {
        private static GameObject EmptyGameObject = new GameObject("EmptyGameObject");
        
        public PoolableActor() : base()
        {
        }

        public PoolableActor(GameObject prefab = null, string name = "") : base(prefab, name)
        {
        }
        
        protected override GameObject CreateGameObject(string name = "", GameObject prefab = null)
        {
            if (prefab == null)
            {
                return Pool.Instantiate(EmptyGameObject);
            }

            return Pool.Instantiate(prefab);
        }

        protected override void DestroyGameObject(ActorMonoBehaviour actorBehaviour)
        {
            Pool.Destroy(actorBehaviour);
        }
    }
}