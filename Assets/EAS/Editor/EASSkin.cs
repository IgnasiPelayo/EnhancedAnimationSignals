using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EAS
{
    public class EASSkin
    {
        // ------ GUIStyles ------
        public static GUIStyle LockStyle { get => "IN LockButton"; }

        // ------ Colors ------
        public static Color SelectedWhiteColor { get => new Color(1, 1, 1, 0.35f); }
        public static Color SelectedBlueColor { get => new Color(61.0f / 255.0f, 94.0f / 255.0f, 152.0f / 255.0f); }
        public static Color SeparatorColor { get => Color.black; }
        public static Color BackgroundColor { get => new Color(41.0f / 255.0f, 41.0f / 255.0f, 41.0f / 255.0f); }
        public static Color ToolbarBackgroundColor { get => new Color(60.0f / 255.0f, 60.0f / 255.0f, 60.0f / 255.0f); }
        public static Color HierarchyTrackGroupColor { get => new Color(38.0f / 255.0f, 138.0f / 255.0f, 111.0f / 255.0f, 0.5f); }
        public static Color HierarchyTrackColor { get => new Color(65.0f / 255.0f, 65.0f / 255.0f, 65.0f / 255.0f); }
        public static Color HierarchyTrackControlsColor { get => new Color(43.0f / 255.0f, 43.0f / 255.0f, 43.0f / 255.0f); }

        // ------ Appearance ------
        public static float ControlToolbarHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public static int HierarchyUpperMargin = 10;
        public static int HierarchyLeftMargin = 15;

        public static float HierarchyTrackHeight = 1.8f * ControlToolbarHeight;
        public static float HierarchyTrackSpacing = 2.0f * EditorGUIUtility.standardVerticalSpacing;
        public static float HierarchyTrackSpacingSingle = HierarchyTrackSpacing / 2.0f;
        public static float HierarchyTrackGroupDropdownSize = 12.0f;
        public static float HierarchyTrackIconMultiplier = 0.6f;
        public static float HierarchyTrackLabelLeftMargin = 25.0f;
        public static float HierarchyTrackOptionsIconMargin = 5.0f;
        public static float HierarchyTrackControlsWidthMultiplier = 1.35f;
        public static float HierarchyTrackControlsVerticalPadding = 4.0f;

        // ------ Textures ------
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
    }
}

