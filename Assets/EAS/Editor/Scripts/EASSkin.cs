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

        public static GUIStyle SceneViewLabelStyle { get { GUIStyle sceneViewLabelStyle = new GUIStyle(EditorStyles.label); sceneViewLabelStyle.fontSize = 20; sceneViewLabelStyle.fontStyle = FontStyle.Bold;
                sceneViewLabelStyle.normal.textColor = sceneViewLabelStyle.hover.textColor = sceneViewLabelStyle.active.textColor = sceneViewLabelStyle.focused.textColor = SceneViewColor; return sceneViewLabelStyle; } }

        public static GUIStyle WhiteMiniLabelStyle { get { GUIStyle labelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel); labelStyle.fontSize = 9; labelStyle.normal.textColor = Color.white; return labelStyle; } }

        public static GUIStyle InspectorTooltipStyle { get { GUIStyle tooltipStyle = new GUIStyle(GUI.skin.box); tooltipStyle.wordWrap = true; tooltipStyle.padding.left = Mathf.RoundToInt(InspectorHeaderLeftMargin + 1); 
                tooltipStyle.alignment = TextAnchor.UpperLeft; tooltipStyle.fontSize = 11;
                tooltipStyle.normal.textColor = tooltipStyle.hover.textColor = tooltipStyle.active.textColor = tooltipStyle.focused.textColor = Color.gray; return tooltipStyle; } }

        public static GUIStyle InspectorErrorMessageStyle { get { GUIStyle errorMessageStyle = new GUIStyle(EditorStyles.label); errorMessageStyle.wordWrap = true; errorMessageStyle.padding.left = Mathf.RoundToInt(InspectorHeaderLeftMargin + 1);
                errorMessageStyle.padding.top = errorMessageStyle.padding.bottom = 5;
                errorMessageStyle.alignment = TextAnchor.UpperLeft; errorMessageStyle.fontSize = 11; errorMessageStyle.richText = true;
                errorMessageStyle.normal.textColor = errorMessageStyle.hover.textColor = errorMessageStyle.active.textColor = errorMessageStyle.focused.textColor = Color.white; return errorMessageStyle; } }

        public static GUIStyle InspectorFocusedLabelStyle { get { GUIStyle focusedLabelStyle = new GUIStyle(EditorStyles.label); focusedLabelStyle.normal.textColor = InspectorFocusedColor; return focusedLabelStyle; } }
        
        // ------ Colors ------
        public static Color SceneViewColor { get => new Color(72.0f / 255.0f, 201.0f / 255.0f, 176.0f / 255.0f, 1.0f); }

        public static Color SelectedWhiteColor { get => new Color(1, 1, 1, 0.35f); }
        public static Color SeparatorColor { get => Color.black; }
        public static Color BackgroundColor { get => new Color(41.0f / 255.0f, 41.0f / 255.0f, 41.0f / 255.0f); }
        public static Color ToolbarBackgroundColor { get => new Color(60.0f / 255.0f, 60.0f / 255.0f, 60.0f / 255.0f); }
        public static Color DraggedItemErrorColor { get => new Color(36.0f / 255.0f, 10.0f / 255.0f, 10.0f / 255.0f, 0.95f); }
        public static Color ErrorColor { get => new Color(150.0f / 255.0f, 15.0f / 255.0f, 15.0f / 255.0f, 1.0f); }

        public static Color HierarchyTrackGroupColor { get => new Color(38.0f / 255.0f, 138.0f / 255.0f, 111.0f / 255.0f, 0.5f); }
        public static Color HierarchyTrackColor { get => new Color(65.0f / 255.0f, 65.0f / 255.0f, 65.0f / 255.0f); }
        public static Color HierarchySelectedColor { get => new Color(61.0f / 255.0f, 94.0f / 255.0f, 152.0f / 255.0f); }
        public static Color HierarchyTrackControlsColor { get => new Color(43.0f / 255.0f, 43.0f / 255.0f, 43.0f / 255.0f); }

        public static Color TimelineFramesBackgroundColor { get => new Color(55.0f / 255.0f, 55.0f / 255.0f, 55.0f / 255.0f); }
        public static Color TimelineFrameColor { get => new Color(180.0f / 255.0f, 180.0f / 255.0f, 180.0f / 255.0f); }
        public static Color TimelineFrameLineColor { get => new Color(111f / 255.0f, 111f / 255.0f, 111f / 255.0f, 0.2f); }
        public static Color TimelineSecondaryFrameLineColor { get => new Color(TimelineFrameLineColor.r, TimelineFrameLineColor.g, TimelineFrameLineColor.b, 0.1f); }
        public static Color TimelineTertiaryFrameLineColor { get => new Color(TimelineFrameLineColor.r, TimelineFrameLineColor.g, TimelineFrameLineColor.b, 0.05f); }
        public static Color TimelineTrackGroupColor { get => new Color(35.0f / 255.0f, 65.0f / 255.0f, 57.0f / 255.0f); }
        public static Color TimelineTrackColor { get => new Color(50.0f / 255.0f, 50.0f / 255.0f, 50.0f / 255.0f); }
        public static Color TimelineSelectedColor { get => new Color(54.0f / 255.0f, 64.0f / 255.0f, 83.0f / 255.0f); }
        public static Color TimelineLockedMutedBackgroundColor { get => new Color(0.0f, 0.0f, 0.0f, 0.65f); }
        public static Color TimelineEventSeparatorColor { get => TimelineFrameLineColor; }
        public static Color TimelineEventSelectedBorderColor { get => Color.white; }
        public static Color TimelineEventSelectedColor { get => new Color(1.0f, 1.0f, 1.0f, 0.3f); }
        public static Color TimelineEventErrorColor { get => new Color(200.0f / 255.0f, 0.0f, 0.0f, 1.0f); }

        public static Color InspectorFocusedColor { get => new Color(124.0f / 255.0f, 171.0f / 255.0f, 240.0f / 255.0f, 1.0f); }
        public static Color InspectorErrorMessageBackgroundColor { get => new Color(51.0f / 255.0f, 51.0f / 255.0f, 51.0f / 255.0f, 1.0f); }

        // ------ Appearance ------
        public static int SceneViewPreviewMargin = 10;

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

        public static float TimelineLeftMargin = 10;
        public static float TimelineRightMargin = 50;
        public static float TimelineMainFrameHeight = ControlToolbarHeight * 0.6f;
        public static float TimelineMainFrameLabelLeftMargin = 2;
        public static float TimelineMainFrameLabelUpperMargin = -2;
        public static float TimelineSecondaryFrameHeight = ControlToolbarHeight * 0.2f;
        public static float TimelineTertiaryFrameHeight = 2.0f;
        public static float TimelineKeyFrameSize = 6;
        public static float TimelineHalfKeyFrameSize = TimelineKeyFrameSize / 2.0f;

        public static float TimelineEventLabelLeftMargin = 3.0f;
        public static float TimelineEventLabelRightMargin = 3.0f;
        public static float TimelineEventResizeRectsMaxWidth = 10.0f;

        public static float InspectorHeaderHeight = ControlToolbarHeight;
        public static float InspectorHeaderLeftMargin = 10;
        public static float InspectorRightMargin = 2;
        public static float InspectorUpperMargin = 5;
        public static float InspectorBottomMargin = 3;
        public static float InspectorHeaderSpacing = EditorGUIUtility.singleLineHeight / 3.0f;
        public static float InspectorFoldoutIndentLeftMargin = -12;
        public static float InspectorListSizeWidth = 60;

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

