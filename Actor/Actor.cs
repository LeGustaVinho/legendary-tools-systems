using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace LegendaryTools.Systems.Actor
{
    [Serializable]
    public abstract class Actor : IActor
    {
        protected static readonly Dictionary<Type, List<Actor>> allActorsByType = new Dictionary<Type, List<Actor>>();
        protected ActorMonoBehaviour actorBehaviour;
        protected readonly GameObject prefab;

        public Actor()
        {
            GameObject newGO = CreateGameObject();
            Init(newGO);
        }

        public Actor(GameObject prefab = null, string name = "")
        {
            this.prefab = prefab;
            GameObject newGO = CreateGameObject(name, prefab);
            Init(newGO);
        }

        public Actor(ActorMonoBehaviour actorBehaviour)
        {
            Possess(actorBehaviour);
            RegisterActor();
        }

        private void Init(GameObject gameObject)
        {
            actorBehaviour = AddActorBehaviour(gameObject);
            actorBehaviour.BindActor(this);
            RegisterActorBehaviourEvents();
            RegisterActor();
        }

        public static void Destroy(Actor actor)
        {
            actor.Dispose();
        }

        public static Actor FindObjectOfType(Type type)
        {
            if (allActorsByType.TryGetValue(type, out List<Actor> actors))
            {
                return actors.FirstOrDefault();
            }

            return null;
        }

        public static T FindObjectOfType<T>() where T : Actor<T>
        {
            if (allActorsByType.TryGetValue(typeof(T), out List<Actor> actors))
            {
                return actors.FirstOrDefault() as T;
            }

            return null;
        }

        public static Actor[] FindObjectsOfType(Type type)
        {
            if (allActorsByType.TryGetValue(type, out List<Actor> actors))
            {
                return actors.ToArray();
            }

            return null;
        }

        public static Actor[] FindObjectsOfType<T>() where T : Actor<T>
        {
            if (allActorsByType.TryGetValue(typeof(T), out List<Actor> actors))
            {
                return actors.ToArray();
            }

            return null;
        }

        public bool Possess(ActorMonoBehaviour target)
        {
            if (target.Actor != null)
            {
                return false;
            }

            Eject();

            actorBehaviour = target;
            actorBehaviour.BindActor(this);
            RegisterActorBehaviourEvents();

            return true;
        }

        public void Eject()
        {
            if (actorBehaviour != null)
            {
                UnRegisterActorBehaviourEvents();
                actorBehaviour.UnBindActor();
                actorBehaviour = null;
            }
        }

        public void RegenerateBody()
        {
            if (actorBehaviour == null)
            {
                GameObject newGO = CreateGameObject(prefab: prefab);
                Init(newGO);
            }
        }
        
        protected void RegisterActorBehaviourEvents()
        {
            actorBehaviour.WhenAwake += Awake;
            actorBehaviour.WhenStart += Start;
            actorBehaviour.WhenUpdate += Update;
            actorBehaviour.WhenDestroy += InternalOnDestroy;
            actorBehaviour.WhenEnable += OnEnable;
            actorBehaviour.WhenDisable += OnDisable;
            actorBehaviour.WhenTriggerEnter += OnTriggerEnter;
            actorBehaviour.WhenTriggerExit += OnTriggerExit;
            actorBehaviour.WhenCollisionEnter += OnCollisionEnter;
            actorBehaviour.WhenCollisionExit += OnCollisionExit;
            actorBehaviour.WhenLateUpdate += LateUpdate;
            actorBehaviour.WhenFixedUpdate += FixedUpdate;
            actorBehaviour.WhenCollisionStay += OnCollisionStay;
            actorBehaviour.WhenApplicationFocus += OnApplicationFocus;
            actorBehaviour.WhenApplicationPause += OnApplicationPause;
            actorBehaviour.WhenApplicationQuit += OnApplicationQuit;
            actorBehaviour.WhenBecameVisible += OnBecameVisible;
            actorBehaviour.WhenBecameInvisible += OnBecameInvisible;
            actorBehaviour.WhenCollisionEnter2D += OnCollisionEnter2D;
            actorBehaviour.WhenCollisionStay2D += OnCollisionStay2D;
            actorBehaviour.WhenCollisionExit2D += OnCollisionExit2D;
            actorBehaviour.WhenDrawGizmos += OnDrawGizmos;
            actorBehaviour.WhenDrawGizmosSelected += OnDrawGizmosSelected;
            actorBehaviour.WhenGUI += OnGUI;
            actorBehaviour.WhenPreCull += OnPreCull;
            actorBehaviour.WhenPreRender += OnPreRender;
            actorBehaviour.WhenPostRender += OnPostRender;
            actorBehaviour.WhenRenderImage += OnRenderImage;
            actorBehaviour.WhenRenderObject += OnRenderObject;
            actorBehaviour.WhenTransformChildrenChanged += OnTransformChildrenChanged;
            actorBehaviour.WhenTransformParentChanged += OnTransformParentChanged;
            actorBehaviour.WhenTriggerEnter2D += OnTriggerEnter2D;
            actorBehaviour.WhenTriggerStay2D += OnTriggerStay2D;
            actorBehaviour.WhenTriggerExit2D += OnTriggerExit2D;
            actorBehaviour.WhenValidate += OnValidate;
            actorBehaviour.WhenWillRenderObject += OnWillRenderObject;
        }

        protected void UnRegisterActorBehaviourEvents()
        {
            actorBehaviour.WhenAwake -= Awake;
            actorBehaviour.WhenStart -= Start;
            actorBehaviour.WhenUpdate -= Update;
            actorBehaviour.WhenDestroy -= InternalOnDestroy;
            actorBehaviour.WhenEnable -= OnEnable;
            actorBehaviour.WhenDisable -= OnDisable;
            actorBehaviour.WhenTriggerEnter -= OnTriggerEnter;
            actorBehaviour.WhenTriggerExit -= OnTriggerExit;
            actorBehaviour.WhenCollisionEnter -= OnCollisionEnter;
            actorBehaviour.WhenCollisionExit -= OnCollisionExit;
            actorBehaviour.WhenLateUpdate -= LateUpdate;
            actorBehaviour.WhenFixedUpdate -= FixedUpdate;
            actorBehaviour.WhenCollisionStay -= OnCollisionStay;
            actorBehaviour.WhenApplicationFocus -= OnApplicationFocus;
            actorBehaviour.WhenApplicationPause -= OnApplicationPause;
            actorBehaviour.WhenApplicationQuit -= OnApplicationQuit;
            actorBehaviour.WhenBecameVisible -= OnBecameVisible;
            actorBehaviour.WhenBecameInvisible -= OnBecameInvisible;
            actorBehaviour.WhenCollisionEnter2D -= OnCollisionEnter2D;
            actorBehaviour.WhenCollisionStay2D -= OnCollisionStay2D;
            actorBehaviour.WhenCollisionExit2D -= OnCollisionExit2D;
            actorBehaviour.WhenDrawGizmos -= OnDrawGizmos;
            actorBehaviour.WhenDrawGizmosSelected -= OnDrawGizmosSelected;
            actorBehaviour.WhenGUI -= OnGUI;
            actorBehaviour.WhenPreCull -= OnPreCull;
            actorBehaviour.WhenPreRender -= OnPreRender;
            actorBehaviour.WhenPostRender -= OnPostRender;
            actorBehaviour.WhenRenderImage -= OnRenderImage;
            actorBehaviour.WhenRenderObject -= OnRenderObject;
            actorBehaviour.WhenTransformChildrenChanged -= OnTransformChildrenChanged;
            actorBehaviour.WhenTransformParentChanged -= OnTransformParentChanged;
            actorBehaviour.WhenTriggerEnter2D -= OnTriggerEnter2D;
            actorBehaviour.WhenTriggerStay2D -= OnTriggerStay2D;
            actorBehaviour.WhenTriggerExit2D -= OnTriggerExit2D;
            actorBehaviour.WhenValidate -= OnValidate;
            actorBehaviour.WhenWillRenderObject -= OnWillRenderObject;
        }

        protected abstract void RegisterActor();

        protected abstract void UnRegisterActor();

        protected virtual GameObject CreateGameObject(string name = "", GameObject prefab = null)
        {
            if (prefab == null)
            {
                return new GameObject(name);
            }

            return Object.Instantiate(prefab);
        }

        protected virtual ActorMonoBehaviour AddActorBehaviour(GameObject gameObject)
        {
            return gameObject.AddComponent<ActorMonoBehaviour>();
        }

        protected virtual void DestroyGameObject(ActorMonoBehaviour actorBehaviour)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(actorBehaviour.GameObject);
#else
            Object.Destroy(actorBehaviour.GameObject);
#endif
        }
        
        #region MonoBehaviour calls

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
        }

        private void InternalOnDestroy()
        {
            OnDestroy();

            StopAllCoroutines();
            UnRegisterActorBehaviourEvents();
            UnRegisterActor();
            actorBehaviour.UnBindActor();
            actorBehaviour = null;
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
        }

        protected virtual void OnTriggerExit(Collider other)
        {
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
        }

        protected virtual void OnCollisionExit(Collision other)
        {
        }

        protected virtual void LateUpdate()
        {
        }

        protected virtual void FixedUpdate()
        {
        }

        protected virtual void OnCollisionStay(Collision obj)
        {
        }

        protected virtual void OnApplicationFocus(bool obj)
        {
        }

        protected virtual void OnApplicationPause(bool obj)
        {
        }

        protected virtual void OnApplicationQuit()
        {
        }

        protected virtual void OnBecameVisible()
        {
        }

        protected virtual void OnBecameInvisible()
        {
        }

        protected virtual void OnCollisionEnter2D(Collision2D obj)
        {
        }

        protected virtual void OnCollisionStay2D(Collision2D obj)
        {
        }

        protected virtual void OnCollisionExit2D(Collision2D obj)
        {
        }

        protected virtual void OnDrawGizmos()
        {
        }

        protected virtual void OnDrawGizmosSelected()
        {
        }

        protected virtual void OnGUI()
        {
        }

        protected virtual void OnPreCull()
        {
        }

        protected virtual void OnPreRender()
        {
        }

        protected virtual void OnPostRender()
        {
        }

        protected virtual void OnRenderImage(RenderTexture arg1, RenderTexture arg2)
        {
        }

        protected virtual void OnRenderObject()
        {
        }

        protected virtual void OnTransformChildrenChanged()
        {
        }

        protected virtual void OnTransformParentChanged()
        {
        }

        protected virtual void OnTriggerEnter2D(Collider2D obj)
        {
        }

        protected virtual void OnTriggerStay2D(Collider2D obj)
        {
        }

        protected virtual void OnTriggerExit2D(Collider2D obj)
        {
        }

        protected virtual void OnValidate()
        {
        }

        protected virtual void OnWillRenderObject()
        {
        }

        #endregion

        #region Interfaces Implementations

        public string Name
        {
            get => actorBehaviour.GameObject.name;
            set => actorBehaviour.GameObject.name = value;
        }

        public string Tag
        {
            get => actorBehaviour.GameObject.tag;
            set => actorBehaviour.GameObject.tag = value;
        }

        public HideFlags HideFlags
        {
            get => actorBehaviour.GameObject.hideFlags;
            set => actorBehaviour.GameObject.hideFlags = value;
        }

        public bool ActiveInHierarchy => actorBehaviour.GameObject.activeInHierarchy;
        public bool ActiveSelf => actorBehaviour.GameObject.activeInHierarchy;

        public int Layer
        {
            get => actorBehaviour.GameObject.layer;
            set => actorBehaviour.GameObject.layer = value;
        }

        public Scene Scene => actorBehaviour.GameObject.scene;

        public void SetActive(bool value)
        {
            actorBehaviour.GameObject.SetActive(value);
        }

        public Component AddComponent(Type componentType)
        {
            return actorBehaviour.GameObject.AddComponent(componentType);
        }

        public T AddComponent<T>() where T : Component
        {
            return actorBehaviour.GameObject.AddComponent<T>();
        }

        public int GetInstanceID()
        {
            return actorBehaviour.GameObject.GetInstanceID();
        }

        public T GetComponent<T>() where T : Component
        {
            return actorBehaviour.GameObject.GetComponent<T>();
        }

        public Component GetComponent(Type type)
        {
            return actorBehaviour.GameObject.GetComponent(type);
        }

        public Component GetComponent(string type)
        {
            return actorBehaviour.GameObject.GetComponent(type);
        }

        public Component GetComponentInChildren(Type t)
        {
            return actorBehaviour.GameObject.GetComponentInChildren(t);
        }

        public T GetComponentInChildren<T>() where T : Component
        {
            return actorBehaviour.GameObject.GetComponentInChildren<T>();
        }

        public Component GetComponentInParent(Type t)
        {
            return actorBehaviour.GameObject.GetComponentInParent(t);
        }

        public T GetComponentInParent<T>() where T : Component
        {
            return actorBehaviour.GameObject.GetComponentInParent<T>();
        }

        public Component[] GetComponents(Type type)
        {
            return actorBehaviour.GameObject.GetComponents(type);
        }

        public T[] GetComponents<T>() where T : Component
        {
            return actorBehaviour.GameObject.GetComponents<T>();
        }

        public Component[] GetComponentsInChildren(Type t, bool includeInactive)
        {
            return actorBehaviour.GameObject.GetComponentsInChildren(t, includeInactive);
        }

        public T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component
        {
            return actorBehaviour.GameObject.GetComponentsInChildren<T>(includeInactive);
        }

        public Component[] GetComponentsInParent(Type t, bool includeInactive = false)
        {
            return actorBehaviour.GameObject.GetComponentsInParent(t, includeInactive);
        }

        public T[] GetComponentsInParent<T>(bool includeInactive = false) where T : Component
        {
            return actorBehaviour.GameObject.GetComponentsInParent<T>(includeInactive);
        }

        public bool Enabled
        {
            get => actorBehaviour.enabled;
            set => actorBehaviour.enabled = true;
        }

        public bool IsActiveAndEnabled => actorBehaviour.isActiveAndEnabled;

        public void CancelInvoke()
        {
            actorBehaviour.CancelInvoke();
        }

        public void CancelInvoke(string methodName)
        {
            actorBehaviour.CancelInvoke(methodName);
        }

        public void Invoke(string methodName, float time)
        {
            actorBehaviour.Invoke(methodName, time);
        }

        public void InvokeRepeating(string methodName, float time, float repeatRate)
        {
            actorBehaviour.InvokeRepeating(methodName, time, repeatRate);
        }

        public bool IsInvoking(string methodName)
        {
            return actorBehaviour.IsInvoking(methodName);
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            return actorBehaviour.StartCoroutine(routine);
        }

        public Coroutine StartCoroutine(string methodName, object value = null)
        {
            return actorBehaviour.StartCoroutine(methodName, value);
        }

        public void StopAllCoroutines()
        {
            actorBehaviour.StopAllCoroutines();
        }

        public void StopCoroutine(string methodName)
        {
            actorBehaviour.StopCoroutine(methodName);
        }

        public void StopCoroutine(IEnumerator routine)
        {
            actorBehaviour.StopCoroutine(routine);
        }

        public void StopCoroutine(Coroutine routine)
        {
            actorBehaviour.StopCoroutine(routine);
        }

        public int ChildCount => actorBehaviour.Transform.childCount;

        public Vector3 EulerAngles
        {
            get => actorBehaviour.Transform.eulerAngles;
            set => actorBehaviour.Transform.eulerAngles = value;
        }

        public Vector3 Forward
        {
            get => actorBehaviour.Transform.forward;
            set => actorBehaviour.Transform.forward = value;
        }

        public bool HasChanged
        {
            get => actorBehaviour.Transform.hasChanged;
            set => actorBehaviour.Transform.hasChanged = value;
        }

        public int HierarchyCapacity => actorBehaviour.Transform.hierarchyCount;

        public int HierarchyCount => actorBehaviour.Transform.hierarchyCount;

        public Vector3 LocalEulerAngles
        {
            get => actorBehaviour.Transform.localEulerAngles;
            set => actorBehaviour.Transform.localEulerAngles = value;
        }

        public Vector3 LocalPosition
        {
            get => actorBehaviour.Transform.localPosition;
            set => actorBehaviour.Transform.localPosition = value;
        }

        public Quaternion LocalRotation
        {
            get => actorBehaviour.Transform.localRotation;
            set => actorBehaviour.Transform.localRotation = value;
        }

        public Vector3 LocalScale
        {
            get => actorBehaviour.Transform.localScale;
            set => actorBehaviour.Transform.localScale = value;
        }

        public Matrix4x4 LocalToWorldMatrix => actorBehaviour.Transform.localToWorldMatrix;
        public Vector3 LossyScale => actorBehaviour.Transform.lossyScale;

        public Transform Parent
        {
            get => actorBehaviour.Transform.parent;
            set => actorBehaviour.Transform.parent = value;
        }

        public Vector3 Position
        {
            get => actorBehaviour.Transform.position;
            set => actorBehaviour.Transform.position = value;
        }

        public Vector3 Right
        {
            get => actorBehaviour.Transform.right;
            set => actorBehaviour.Transform.right = value;
        }

        public Transform Root => actorBehaviour.Transform.root;

        public Quaternion Rotation
        {
            get => actorBehaviour.Transform.rotation;
            set => actorBehaviour.Transform.rotation = value;
        }

        public Vector3 Up
        {
            get => actorBehaviour.Transform.up;
            set => actorBehaviour.Transform.up = value;
        }

        public Matrix4x4 WorldToLocalMatrix => actorBehaviour.Transform.worldToLocalMatrix;

        public void DetachChildren()
        {
            actorBehaviour.Transform.DetachChildren();
        }

        public Transform Find(string name)
        {
            return actorBehaviour.Transform.Find(name);
        }

        public Transform GetChild(int index)
        {
            return actorBehaviour.Transform.GetChild(index);
        }

        public int GetSiblingIndex()
        {
            return actorBehaviour.Transform.GetSiblingIndex();
        }

        public Vector3 InverseTransformDirection(Vector3 direction)
        {
            return actorBehaviour.Transform.InverseTransformDirection(direction);
        }

        public Vector3 InverseTransformPoint(Vector3 position)
        {
            return actorBehaviour.Transform.InverseTransformDirection(position);
        }

        public Vector3 InverseTransformVector(Vector3 vector)
        {
            return actorBehaviour.Transform.InverseTransformDirection(vector);
        }

        public bool IsChildOf(Transform parent)
        {
            return actorBehaviour.Transform.IsChildOf(parent);
        }

        public void LookAt(Transform target)
        {
            actorBehaviour.Transform.LookAt(target);
        }

        public void LookAt(Transform target, Vector3 worldUp)
        {
            actorBehaviour.Transform.LookAt(target, worldUp);
        }

        public void Rotate(Vector3 eulers, Space relativeTo = Space.Self)
        {
            actorBehaviour.Transform.Rotate(eulers, relativeTo);
        }

        public void Rotate(float xAngle, float yAngle, float zAngle, Space relativeTo = Space.Self)
        {
            actorBehaviour.Transform.Rotate(xAngle, yAngle, zAngle, relativeTo);
        }

        public void Rotate(Vector3 axis, float angle, Space relativeTo = Space.Self)
        {
            actorBehaviour.Transform.Rotate(axis, angle, relativeTo);
        }

        public void RotateAround(Vector3 point, Vector3 axis, float angle)
        {
            actorBehaviour.Transform.RotateAround(point, axis, angle);
        }

        public void SetAsFirstSibling()
        {
            actorBehaviour.Transform.SetAsFirstSibling();
        }

        public void SetAsLastSibling()
        {
            actorBehaviour.Transform.SetAsLastSibling();
        }

        public void SetParent(Transform parent)
        {
            actorBehaviour.Transform.SetParent(parent);
        }

        public void SetParent(Transform parent, bool worldPositionStays)
        {
            actorBehaviour.Transform.SetParent(parent, worldPositionStays);
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            actorBehaviour.Transform.SetPositionAndRotation(position, rotation);
        }

        public void SetSiblingIndex(int index)
        {
            actorBehaviour.Transform.SetSiblingIndex(index);
        }

        public Vector3 TransformDirection(Vector3 direction)
        {
            return actorBehaviour.Transform.TransformDirection(direction);
        }

        public Vector3 TransformPoint(Vector3 position)
        {
            return actorBehaviour.Transform.TransformDirection(position);
        }

        public Vector3 TransformVector(Vector3 vector)
        {
            return actorBehaviour.Transform.TransformDirection(vector);
        }

        public void Translate(Vector3 translation)
        {
            actorBehaviour.Transform.Translate(translation);
        }

        public void Translate(Vector3 translation, Space relativeTo = Space.Self)
        {
            actorBehaviour.Transform.Translate(translation, relativeTo);
        }

        public Transform Transform => actorBehaviour.Transform;
        public RectTransform RectTransform => actorBehaviour.RectTransform;
        public GameObject GameObject => actorBehaviour.GameObject;

        public Vector2 AnchoredPosition
        {
            get => RectTransform.anchoredPosition;
            set => RectTransform.anchoredPosition = value;
        }

        public Vector3 AnchoredPosition3D
        {
            get => RectTransform.anchoredPosition3D;
            set => RectTransform.anchoredPosition3D = value;
        }

        public Vector2 AnchorMax
        {
            get => RectTransform.anchorMax;
            set => RectTransform.anchorMax = value;
        }

        public Vector2 AnchorMin
        {
            get => RectTransform.anchorMin;
            set => RectTransform.anchorMin = value;
        }

        public Vector2 OffsetMax
        {
            get => RectTransform.offsetMax;
            set => RectTransform.offsetMin = value;
        }

        public Vector2 OffsetMin
        {
            get => RectTransform.offsetMin;
            set => RectTransform.offsetMin = value;
        }

        public Vector2 Pivot
        {
            get => RectTransform.pivot;
            set => RectTransform.pivot = value;
        }

        public Rect Rect => RectTransform.rect;

        public Vector2 SizeDelta
        {
            get => RectTransform.sizeDelta;
            set => RectTransform.sizeDelta = value;
        }

        public void ForceUpdateRectTransforms()
        {
            if (RectTransform != null)
            {
                RectTransform.ForceUpdateRectTransforms();
            }
        }

        public void GetLocalCorners(Vector3[] fourCornersArray)
        {
            if (RectTransform != null)
            {
                RectTransform.GetLocalCorners(fourCornersArray);
            }
        }

        public void GetWorldCorners(Vector3[] fourCornersArray)
        {
            if (RectTransform != null)
            {
                RectTransform.GetWorldCorners(fourCornersArray);
            }
        }

        public void SetInsetAndSizeFromParentEdge(RectTransform.Edge edge, float inset, float size)
        {
            if (RectTransform != null)
            {
                RectTransform.SetInsetAndSizeFromParentEdge(edge, inset, size);
            }
        }

        public void SetSizeWithCurrentAnchors(RectTransform.Axis axis, float size)
        {
            if (RectTransform != null)
            {
                RectTransform.SetSizeWithCurrentAnchors(axis, size);
            }
        }
        
        public virtual void Dispose()
        {
            if (actorBehaviour != null)
            {
                DestroyGameObject(actorBehaviour);
            }
            else
            {
                UnRegisterActor();
            }
        }
        
        #endregion
    }

    [Serializable]
    public class Actor<TClass> : Actor
    {
        public Actor() : base()
        {
        }

        public Actor(GameObject prefab = null, string name = "") : base(prefab, name)
        {
        }

        protected override void RegisterActor()
        {
            Type type = typeof(TClass);
            if (!allActorsByType.ContainsKey(type))
            {
                allActorsByType.Add(type, new List<Actor>());
            }

            if (!allActorsByType[type].Contains(this))
            {
                allActorsByType[type].Add(this);
            }
        }

        protected override void UnRegisterActor()
        {
            Type type = typeof(TClass);
            if (allActorsByType.ContainsKey(type))
            {
                if (allActorsByType[type].Contains(this))
                {
                    allActorsByType[type].Remove(this);
                }
            }
        }
    }

    [Serializable]
    public class Actor<TClass, TBehaviour> : Actor<TClass>
        where TBehaviour : ActorMonoBehaviour
    {
        public TBehaviour BodyBehaviour { get; private set; }

        public Actor() : base()
        {
            BodyBehaviour = actorBehaviour as TBehaviour;
        }

        public Actor(GameObject prefab = null, string name = "") : base(prefab, name)
        {
            BodyBehaviour = actorBehaviour as TBehaviour;
        }

        protected override ActorMonoBehaviour AddActorBehaviour(GameObject gameObject)
        {
            return gameObject.AddComponent<TBehaviour>();
        }
    }
}