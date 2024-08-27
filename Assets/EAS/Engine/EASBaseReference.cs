using UnityEngine;

namespace EAS
{
    [System.Serializable]
    public abstract class EASBaseReference
    {
        public abstract System.Type GetObjectType();

        public abstract object ResolveReference(EASBaseController controller);

        public abstract void UpdateValue(object newInstance, Transform rootTransform);

#if UNITY_EDITOR
        public abstract void GenerateRuntimeData(EASBaseController controller);
#endif // UNITY_EDITOR
    }

    [System.Serializable]
    public class EASReference<T> : EASBaseReference where T : UnityEngine.Object
    {
        [SerializeField, HideInInspector]
        protected string m_Path;
        public string Path { get => m_Path; set => m_Path = value; }

        protected static string m_RootPath = "Path@RootController";

        [SerializeField, HideInInspector]
        protected T m_Instance;
        public T Instance { get => m_Instance; set => m_Instance = value; }

        public override System.Type GetObjectType() => typeof(T);

        public override object ResolveReference(EASBaseController controller)
        {
            m_Instance = InternalResolve(controller);
            return m_Instance;
        }

        public T Resolve(EASBaseController controller)
        {
            return ResolveReference(controller) as T;
        }

        protected T InternalResolve(EASBaseController controller, bool forceFindComponent = false)
        {
            if (string.IsNullOrEmpty(m_Path))
            {
                return null;
            }

            if (m_Instance != null && !forceFindComponent)
            {
                return m_Instance;
            }

            Transform targetTransform = m_Path == m_RootPath ? controller.DataRoot : controller.DataRoot.Find(m_Path);
            if (targetTransform != null)
            {
                return typeof(T) == typeof(GameObject) ? targetTransform.gameObject as T : targetTransform.GetComponent<T>();
            }

            return null;
        }

        public override void UpdateValue(object newInstance, Transform rootTransform)
        {
            m_Instance = null;
            if (newInstance != null)
            {
                GameObject targetObject = null;
                if (newInstance is GameObject)
                {
                    targetObject = newInstance as GameObject;
                }
                else if (newInstance is Component)
                {
                    targetObject = (newInstance as Component).gameObject;
                }

                if (targetObject != null)
                {
                    if (targetObject.transform.IsChildOf(rootTransform))
                    {
                        m_Path = targetObject != rootTransform.gameObject ? GetRelativePath(rootTransform, targetObject.transform) : m_RootPath;
                        return;
                    }
                    else
                    {
                        Debug.LogError($"EASReference can only reference child Objects of Controller's DataRoot {rootTransform.name}");
                    }
                }
            }

            m_Path = string.Empty;
        }

        protected string GetRelativePath(Transform rootTransform, Transform targetTransform)
        {
            string path = "/" + targetTransform.name;
            while (targetTransform.parent != null && targetTransform.parent != rootTransform)
            {
                targetTransform = targetTransform.parent;
                path = "/" + targetTransform.name + path;
            }

            if (path[0] == '/')
            {
                path = path.Remove(0, 1);
            }

            return path;
        }

#if UNITY_EDITOR
        public override void GenerateRuntimeData(EASBaseController controller)
        {
            m_Instance = InternalResolve(controller, forceFindComponent: true);

            if (m_Instance == null)
            {
                Debug.LogError($"Couldn't find a valid Instance for EASReference<{GetObjectType().Name}> at path: {m_Path}", controller.DataRootGameObject);
            }
        }
#endif // UNITY_EDITOR
    }
}

