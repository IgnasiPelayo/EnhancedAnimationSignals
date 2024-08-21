using UnityEngine;

namespace EAS
{
    [System.Serializable]
    public abstract class EASBaseReference
    {
        public abstract System.Type GetObjectType();

        public abstract object Resolve(EASBaseController controller);

        public abstract void UpdateValue(object newInstance, Transform rootTransform);
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

        public override object Resolve(EASBaseController controller)
        {
            m_Instance = InternalResolve(controller);
            return m_Instance;
        }

        protected T InternalResolve(EASBaseController controller)
        {
            if (string.IsNullOrEmpty(m_Path))
            {
                return null;
            }

            if (m_Instance != null)
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
    }
}

