using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EAS
{
    public class EASSkin
    {
        protected static Dictionary<string, Texture2D> m_Textures = new Dictionary<string, Texture2D>();
        public static Texture2D CustomIcon(string name)
        {
            if (!m_Textures.ContainsKey(name))
            {
                string path = $"Assets/EAS/Editor/Icons/{name}.png";

                Texture2D iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (iconTexture == null)
                {
                    Debug.LogError($"Can't find icon at path: {path}");
                    return null;
                }

                m_Textures.Add(name, iconTexture);
                return iconTexture;
            }

            return m_Textures[name];
        }

        public static Texture2D Icon(string key)
        {
            if (!m_Textures.ContainsKey(key))
            {
                Texture2D iconTexture = EditorGUIUtility.IconContent(key).image as Texture2D;
                if (iconTexture != null)
                {
                    m_Textures.Add(key, iconTexture);
                }

                return iconTexture;
            }

            return m_Textures[key];
        }

        public static Texture2D Icon(System.Type type)
        {
            string fullName = type.FullName;
            if (!m_Textures.ContainsKey(fullName))
            {
                Texture2D iconTexture = EditorGUIUtility.ObjectContent(null, type).image as Texture2D;
                if (iconTexture != null)
                {
                    m_Textures.Add(fullName, iconTexture);
                }

                return iconTexture;
            }

            return m_Textures[fullName];
        }

        public static GUIStyle LockStyle { get => "IN LockButton"; }

        public static Color SelectedColor { get => new Color(1, 1, 1, 0.35f); }

        public static Color SeparatorColor { get => Color.black; }
    }
}

