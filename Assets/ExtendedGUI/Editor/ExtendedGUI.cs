using UnityEditor;
using UnityEngine;

namespace ExtendedGUI
{
    public class ExtendedBaseGUI
    {
        public abstract class TabSwitcher
        {
            protected static float NotSelectedTabColorValue = 40.0f / 255.0f;
            protected static float NotSelectedHoverTabColorValue = 48.0f / 255f;
            public static Color NotSelectedTabColor = new Color(NotSelectedTabColorValue, NotSelectedTabColorValue, NotSelectedTabColorValue);
            public static Color NotSelectedHoverTabColor = new Color(NotSelectedHoverTabColorValue, NotSelectedHoverTabColorValue, NotSelectedHoverTabColorValue);

            protected class Tab
            {
                public const int kTabContentPadding = 8;

                protected GUIContent m_Content;
                public GUIContent Content { get => m_Content; }

                protected GUIStyle m_Style;
                public GUIStyle Style { get => m_Style; }

                protected GUIStyle m_NotSelectedGUIStyle;
                public GUIStyle NotSelectedStyle { get => m_NotSelectedGUIStyle; }

                protected Texture2D m_IconTexture;
                public Texture2D IconTexture { get => m_IconTexture; }

                public Tab(GUIContent content, GUIStyle style, Texture2D iconTexture = null)
                {
                    m_Content = content;
                    m_Style = style;

                    m_NotSelectedGUIStyle = new GUIStyle(m_Style);
                    m_NotSelectedGUIStyle.normal.background = ExtendedGUI.CreateSingleColorTexture(NotSelectedTabColor);
                    m_NotSelectedGUIStyle.hover.background = ExtendedGUI.CreateSingleColorTexture(NotSelectedHoverTabColor);

                    m_IconTexture = iconTexture;
                }

                public Tab(GUIContent content, GUIStyle style, GUIStyle notSelectedGUIStyle, Texture2D iconTexture = null)
                {
                    m_Content = content;
                    m_Style = style;
                    m_NotSelectedGUIStyle = notSelectedGUIStyle;
                    m_IconTexture = iconTexture;
                }
            };

            protected Tab[] m_Tabs = System.Array.Empty<Tab>();
            protected int m_SelectedEntry = -1;
            public int SelectedEntry { get => m_SelectedEntry; }

            protected Vector2 m_LastTabPosition = Vector2.zero;
            public Vector2 LastTabPosition { get => m_LastTabPosition; }

            public TabSwitcher()
            {

            }

            public TabSwitcher(int selectedEntry)
            {
                m_SelectedEntry = selectedEntry;
            }

            public void AddTab(GUIContent content, GUIStyle style, Texture2D iconTexture = null, GUIStyle notSelectedGUIStyle = null)
            {
                System.Array.Resize(ref m_Tabs, m_Tabs.Length + 1);
                m_Tabs[m_Tabs.Length - 1] = notSelectedGUIStyle == null ? new Tab(content, style, iconTexture) : new Tab(content, style, notSelectedGUIStyle, iconTexture);

                if (m_SelectedEntry == -1)
                {
                    m_SelectedEntry = 0;
                }
            }
        }

        public class ExtendedBaseGUIOption
        {
            public enum OptionType
            {
                Selected,
                IconPadding,
                IconIsOnTop
            }

            protected OptionType m_OptionType;
            public OptionType TypeOfOption { get => m_OptionType; }

            protected object m_Value;
            public object Value { get => m_Value; }

            public ExtendedBaseGUIOption(OptionType optionType, object value)
            {
                m_OptionType = optionType;
                m_Value = value;
            }
        };

        public static bool HasOptionOfType<T>(ExtendedBaseGUIOption[] options, ExtendedBaseGUIOption.OptionType optionType, out T value)
        {
            for (int i = 0; i < options.Length; ++i)
            {
                if (options[i] != null && options[i].TypeOfOption == optionType)
                {
                    value = (T)options[i].Value;
                    return true;
                }
            }

            value = (T)default;
            return false;
        }
    }

    public class ExtendedGUI
    {
        protected static float ms_ButtonContentInnerPadding = 4.0f;
        public static float ButtonContentInnerPadding { get => ms_ButtonContentInnerPadding; }

        protected static Texture2D ms_PreviewRectTexture;
        public static Texture2D PreviewRectTexture { get { if (ms_PreviewRectTexture == null) { ms_PreviewRectTexture = CreateSingleColorTexture(new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.4f)); } return ms_PreviewRectTexture; } }

        protected static Texture2D ms_SelectedButtonTexture;
        public static Texture2D SelectedButtonTexture { get { if (ms_SelectedButtonTexture == null) { ms_SelectedButtonTexture = CreateSingleColorTexture(new Color(0.17255f, 0.36471f, 0.529412f)); } return ms_SelectedButtonTexture; } }

        protected static GUIStyle ms_CenteredLabelGUIStyle;
        public static GUIStyle CenteredLabelGUIStyle { get { if (ms_CenteredLabelGUIStyle == null) { ms_CenteredLabelGUIStyle = new GUIStyle(EditorStyles.label); ms_CenteredLabelGUIStyle.alignment = TextAnchor.MiddleCenter; } return ms_CenteredLabelGUIStyle; } }

        public static void CenterLabel(Rect rect, GUIContent content)
        {
            CenterLabel(rect, content, EditorStyles.label);
        }

        public static void CenterLabel(Rect rect, GUIContent content, GUIStyle style)
        {
            GUI.Label(GetInnerRect(rect, CalcSize(content, style, 2)), content, style);
        }

        public static void CenterLabelVertically(Rect rect, GUIContent content, GUIStyle style, int innerPadding = 2)
        {
            Rect innerRect = GetInnerRect(rect, CalcSize(content, style, innerPadding));
            GUI.Label(new Rect(rect.x + innerPadding, innerRect.y, rect.width - 2 * innerPadding, innerRect.height), content, style);
        }

        protected static void Internal_SelectedItem(Rect itemRect)
        {
            Rect selectionRect = new Rect(itemRect.x, itemRect.y, itemRect.width - 1, 2);
            GUI.DrawTexture(selectionRect, SelectedButtonTexture);
        }

        protected static bool Internal_Button(Rect buttonRect, Rect contentButtonRect, Rect iconRect, GUIContent content, GUIStyle style, Texture2D iconTexture, bool iconIsOnTop, bool isSelected, bool enabled)
        {
            System.Func<bool> createButton = () =>
            {
                bool buttonPressed = Internal_LeftClickButton(buttonRect, new GUIContent(string.Empty, content.tooltip), style);

                return buttonPressed;
            };

            System.Action createLabel = () =>
            {
                GUI.Label(contentButtonRect, new GUIContent(content.text), CenteredLabelGUIStyle);
            };

            System.Action createIcon = () =>
            {
                if (iconTexture == null)
                {
                    return;
                }

                if (enabled)
                {
                    GUI.DrawTexture(iconRect, iconTexture);
                }
                else
                {
                    GUIStyle iconStyle = new GUIStyle();
                    iconStyle.normal.background = iconTexture;
                    GUI.Button(iconRect, new GUIContent(), iconStyle);
                }
            };

            bool buttonPressed = false;
            if (iconIsOnTop)
            {
                buttonPressed = createButton();
                createLabel();
                createIcon();
            }
            else
            {
                createLabel();
                createIcon();
                buttonPressed = createButton();
            }

            if (isSelected)
            {
                Internal_SelectedItem(buttonRect);
            }

            return buttonPressed;
        }

        protected static bool Internal_LeftClickButton(Rect buttonRect, GUIContent content, GUIStyle style)
        {
            Event currentEvent = Event.current;
            int buttonId = EditorGUIUtility.GetControlID(FocusType.Passive);

            if (EditorGUIUtility.hotControl == buttonId && buttonRect.Contains(currentEvent.mousePosition))
            {
                style = new GUIStyle(style);
                style.normal = style.active;
            }
            GUI.Label(buttonRect, content, style);

            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && buttonRect.Contains(currentEvent.mousePosition))
            {
                EditorGUIUtility.hotControl = buttonId;
                EditorWindow.focusedWindow.Repaint();
            }
            else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 && EditorGUIUtility.hotControl == buttonId)
            {
                EditorGUIUtility.hotControl = 0;
                if (buttonRect.Contains(currentEvent.mousePosition))
                {
                    EditorWindow.focusedWindow.Repaint();
                    return true;
                }
            }
            else if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 0 && EditorGUIUtility.hotControl == buttonId)
            {
                EditorWindow.focusedWindow.Repaint();
            }

            return false;
        }

        protected static bool Internal_Button(Rect buttonRect, GUIContent content, GUIStyle style, Texture2D iconTexture, float iconPadding, bool iconIsOnTop, bool isSelected, bool enabled)
        {
            bool wasEnabled = GUI.enabled;
            GUI.enabled = enabled;

            float iconButtonHeightWithPadding = buttonRect.height - (iconPadding * 2.0f);
            Rect iconButtonRect = new Rect(buttonRect.x + ((buttonRect.width - iconButtonHeightWithPadding) / 2.0f), buttonRect.y + iconPadding, iconButtonHeightWithPadding, iconButtonHeightWithPadding);

            bool buttonPressed = Internal_Button(buttonRect, buttonRect, iconButtonRect, content, style, iconTexture, iconIsOnTop, isSelected, enabled);

            GUI.enabled = wasEnabled;

            return buttonPressed;
        }

        protected static bool Internal_ButtonWithIcon(Rect buttonRect, GUIContent content, GUIStyle style, Texture2D iconTexture, float iconPadding, bool iconIsOnTop, bool isSelected, bool enabled)
        {
            float contentWidth = CalcSize(content, CenteredLabelGUIStyle).x;
            float buttonIconAndContentWidth = buttonRect.height + ButtonContentInnerPadding + contentWidth;
            float buttonHorizontalPadding = (buttonRect.width - buttonIconAndContentWidth) / 2.0f;

            Rect iconRect = new Rect(buttonRect.x + buttonHorizontalPadding, buttonRect.y, buttonRect.height, buttonRect.height);
            float iconButtonHeightWithPadding = iconRect.height - (iconPadding * 2.0f);
            Rect iconWithPaddingRect = new Rect(iconRect.x + ((iconRect.width - iconButtonHeightWithPadding) / 2.0f), iconRect.y + iconPadding, iconButtonHeightWithPadding, iconButtonHeightWithPadding);

            Rect contentRect = new Rect(iconRect.x + iconRect.width + ButtonContentInnerPadding, buttonRect.y, contentWidth, buttonRect.height);

            bool wasEnabled = GUI.enabled;
            GUI.enabled = enabled;

            bool buttonPressed = Internal_Button(buttonRect, contentRect, iconWithPaddingRect, content, style, iconTexture, iconIsOnTop, isSelected, enabled);

            GUI.enabled = wasEnabled;

            return buttonPressed;
        }

        public static bool Button(Rect buttonRect, GUIContent content, GUIStyle style, bool isSelected, bool labelIsInOnTop = false, bool enabled = true)
        {
            return Internal_Button(buttonRect, content, style, iconTexture: null, iconPadding: 0.0f, iconIsOnTop: labelIsInOnTop, isSelected, enabled);
        }

        public static bool IconButton(Rect buttonRect, GUIContent content, GUIStyle style, Texture2D iconTexture, float iconPadding, bool iconIsOnTop, bool isSelected, bool enabled = true)
        {
            if (!string.IsNullOrEmpty(content.text) && !string.IsNullOrWhiteSpace(content.text))
            {
                return Internal_ButtonWithIcon(buttonRect, content, style, iconTexture, iconPadding, iconIsOnTop, isSelected, enabled);
            }

            return Internal_Button(buttonRect, content, style, iconTexture, iconPadding, iconIsOnTop, isSelected, enabled);
        }

        public static bool IconButton(Rect buttonRect, GUIContent content, GUIStyle style, Texture2D iconTexture, float iconPadding, bool iconIsOnTop, bool isSelected)
        {
            return IconButton(buttonRect, content, style, iconTexture, iconPadding, iconIsOnTop, isSelected, GUI.enabled);
        }

        public static void BeginSplitter(SplitterState state)
        {
            state.ResolveDirty();

            Rect splitterRectWithOffset = state.GetSplitterDragRect();

            EventType eventType = Event.current.GetTypeForControl(state.Id);
            if (eventType == EventType.MouseDown)
            {
                if (Event.current.button == 0 && Event.current.clickCount == 1 && splitterRectWithOffset.Contains(Event.current.mousePosition))
                {
                    state.BeginDrag(Event.current.mousePosition);

                    Event.current.Use();
                }
            }
            else if (eventType == EventType.MouseUp)
            {
                if (state.Dragging)
                {
                    state.Dragging = false;

                    Event.current.Use();
                }
            }
            else if (eventType == EventType.MouseDrag)
            {
                if (state.Dragging)
                {
                    float dragDistance = state.GetDragDistance(Event.current.mousePosition);
                    state.PerformDrag(dragDistance);

                    Event.current.Use();
                    EditorWindow.focusedWindow.Repaint();
                }
            }
            else if (eventType == EventType.Repaint)
            {
                EditorGUIUtility.AddCursorRect(splitterRectWithOffset, state.IsVertical ? MouseCursor.ResizeVertical : MouseCursor.ResizeHorizontal, state.Id);
                EditorWindow.focusedWindow.Repaint();
            }

            state.DrawSplitter();
        }

        public static Vector2 CalcSize(GUIContent content, GUIStyle style, float contentPadding = 0.0f)
        {
            Vector2 size = style.CalcSize(content);
            size.x += contentPadding * 2;
            return size;
        }

        public static Texture2D CreateSingleColorTexture(Color color)
        {
            Texture2D singleColorTexture = new Texture2D(1, 1);
            singleColorTexture.SetPixels(new Color[2] { color, color });
            singleColorTexture.Apply();

            return singleColorTexture;
        }

        public static void PreviewRect(Rect rectToPreview)
        {
            GUI.DrawTexture(rectToPreview, PreviewRectTexture);
        }

        public static Rect GetInnerRect(Rect rect, float width, float height)
        {
            if (rect.width >= width && rect.height >= height)
            {
                float widthDifference = (rect.width - width);
                float heightDifference = (rect.height - height);

                rect = new Rect(rect.x + widthDifference / 2.0f, rect.y + heightDifference / 2.0f, width, height);
            }

            return rect;
        }

        public static Rect GetInnerRect(Rect rect, Vector2 size)
        {
            return GetInnerRect(rect, size.x, size.y);
        }

        public static Rect GetInnerRect(Rect rect, float aspectRatio)
        {
            float originalRectAspectRatio = rect.width / rect.height;
            Rect innerRect = rect;

            if (originalRectAspectRatio <= aspectRatio)
            {
                innerRect.height = innerRect.width / aspectRatio;
                innerRect.y += (rect.height / 2.0f) - (innerRect.height / 2.0f);
            }
            else
            {
                innerRect.width = innerRect.height * aspectRatio;
                innerRect.x += (rect.width / 2.0f) - (innerRect.width / 2.0f);
            }

            return innerRect;
        }

        public static void DrawOutlineRect(Rect rect, Color outlineColor, RectOffset outlineWidths)
        {
            Rect lineRect = new Rect(rect.x, rect.y, rect.width, outlineWidths.top);
            EditorGUI.DrawRect(lineRect, outlineColor);

            lineRect = new Rect(rect.x, rect.yMax - outlineWidths.bottom, rect.width, outlineWidths.bottom);
            EditorGUI.DrawRect(lineRect, outlineColor);

            lineRect = new Rect(rect.x, rect.y, outlineWidths.left, rect.height);
            EditorGUI.DrawRect(lineRect, outlineColor);

            lineRect = new Rect(rect.xMax - outlineWidths.right, rect.y, outlineWidths.right, rect.height);
            EditorGUI.DrawRect(lineRect, outlineColor);
        }

        public static void DrawOutlineRect(Rect rect, Color outlineColor, int outlineWidth)
        {
            DrawOutlineRect(rect, outlineColor, new RectOffset(outlineWidth, outlineWidth, outlineWidth, outlineWidth));
        }

        public static Texture2D GetTextureFromCamera(Camera camera)
        {
            RenderTexture currentRenderTexture = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;

            camera.Render();
            Texture2D texture = RenderTextureToTexture2D(camera.targetTexture);

            RenderTexture.active = currentRenderTexture;

            return texture;
        }

        public static Texture2D RenderTextureToTexture2D(RenderTexture renderTexture)
        {
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
            texture.Apply();

            return texture;
        }
        
        public static void DownloadTexture(Texture2D textureToDownload, string path)
        {
            Texture2D exportTexture = new Texture2D(textureToDownload.width, textureToDownload.height, textureToDownload.format, textureToDownload.mipmapCount, true);
            Graphics.CopyTexture(textureToDownload, exportTexture);

            System.IO.File.WriteAllBytes(path, exportTexture.EncodeToPNG());
        }

        public static void DrawVerticalDottedLine(Rect rect, Color color, int dotLength, int spacing)
        {
            Rect dotLineRect = new Rect(rect.x, rect.y, rect.width, dotLength);
            while (dotLineRect.yMax < rect.yMax)
            {
                EditorGUI.DrawRect(dotLineRect, color);
                dotLineRect.y += spacing;
            }
        }

        public static void GenericMenuAddItem(GenericMenu menu, GUIContent content, bool enabled, GenericMenu.MenuFunction func)
        {
            if (enabled)
            {
                menu.AddItem(content, false, func);
            }
            else
            {
                menu.AddDisabledItem(content, false);
            }
        }

        public class ZoomTextureRect
        {
            protected Rect m_ZoomRect = new Rect(0, 0, 1, 1);

            protected Vector2 m_TextureSizeInPixels;

            protected Vector2 m_PanInitialMousePosition;

            protected float CurrentZoom { get => m_ZoomRect.width; }

            protected Vector2 ZoomOffset { get => m_ZoomRect.position; }

            protected bool IsPanning { get => m_PanInitialMousePosition != Vector2.one * -1; }

            public ZoomTextureRect(Vector2 textureSizeInPixels)
            {
                m_TextureSizeInPixels = textureSizeInPixels;
                ResetPan();
            }

            public ZoomTextureRect(Texture2D texture)
            {
                m_TextureSizeInPixels = new Vector2(texture.width, texture.height);
                ResetPan();
            }

            public ZoomTextureRect(ZoomTextureRect other)
            {
                m_ZoomRect = other.m_ZoomRect;
                m_TextureSizeInPixels = other.m_TextureSizeInPixels;
                ResetPan();
            }

            public void Show(Rect rect, Texture2D texture)
            {
                GUI.DrawTextureWithTexCoords(rect, texture, m_ZoomRect);

                Vector2 mousePosition = Event.current.mousePosition;
                if (rect.Contains(mousePosition))
                {
                    if (Event.current.isMouse && (Event.current.button == 1 || Event.current.button == 2))
                    {
                        if (Event.current.type == EventType.MouseDown)
                        {
                            m_PanInitialMousePosition = mousePosition;
                        }
                        else if (Event.current.type == EventType.MouseDrag)
                        {
                            PanTextureAtRect(rect, Event.current.delta);
                            EditorWindow.focusedWindow.Repaint();
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            ResetPan();
                            Event.current.Use();
                        }
                    }
                    else if (Event.current.type == EventType.ScrollWheel)
                    {
                        float newZoom = Mathf.Clamp(CurrentZoom + Mathf.Sign(Event.current.delta.y) * 0.05f, 0.1f, 1.0f);
                        ZoomTextureAtRect(rect, Event.current.mousePosition, newZoom);
                        EditorWindow.focusedWindow.Repaint();
                    }
                }
                else
                {
                    ResetPan();
                }

                EditorGUIUtility.AddCursorRect(rect, IsPanning ? MouseCursor.Pan : MouseCursor.Zoom);
            }

            protected void ResetPan()
            {
                m_PanInitialMousePosition = Vector2.one * -1;
            }

            protected void ZoomTextureAtRect(Rect rect, Vector2 zoomFocusPosition, float targetZoom)
            {
                if (targetZoom == 1)
                {
                    m_ZoomRect = new Rect(0, 0, 1, 1);
                }

                Vector2 zoomFocusCoordinates = GetTextureFocusCoordinates(rect, zoomFocusPosition);
                Vector2 zoomFocusTexturePixel = GetTexturePixel(zoomFocusCoordinates);

                m_ZoomRect = GetZoomRect(zoomFocusCoordinates, zoomFocusTexturePixel, targetZoom);
            }

            protected void PanTextureAtRect(Rect rect, Vector2 panDelta)
            {
                Vector2 panInitialFocusCoordinats = GetTextureFocusCoordinates(rect, m_PanInitialMousePosition);
                Vector2 panFocusCoordinates = GetTextureFocusCoordinates(rect, m_PanInitialMousePosition - panDelta);
                Vector2 panFocusTexturePixel = GetTexturePixel(panFocusCoordinates);

                m_ZoomRect = GetZoomRect(panInitialFocusCoordinats, panFocusTexturePixel, CurrentZoom);
            }

            protected Vector2 GetTextureFocusCoordinates(Rect rect, Vector2 focusPosition)
            {
                return new Vector2((focusPosition.x - rect.x) / rect.width, (focusPosition.y - rect.y) / rect.height);
            }

            protected Vector2 GetTexturePixel(Vector2 focusCoordinates)
            {
                return new Vector2((m_TextureSizeInPixels.x * CurrentZoom * focusCoordinates.x) + m_TextureSizeInPixels.x * ZoomOffset.x,
                    (m_TextureSizeInPixels.y * CurrentZoom * (1.0f - focusCoordinates.y)) + m_TextureSizeInPixels.y * ZoomOffset.y);
            }

            protected Rect GetZoomRect(Vector2 focusCoordinates, Vector2 focusTexturePixel, float desiredZoom)
            {
                Rect zoomRect = Rect.zero;
                zoomRect.x = Mathf.Clamp((focusTexturePixel.x - (m_TextureSizeInPixels.x * desiredZoom * focusCoordinates.x)) / m_TextureSizeInPixels.x, 0.0f, 1.0f - desiredZoom);
                zoomRect.y = Mathf.Clamp((focusTexturePixel.y - (m_TextureSizeInPixels.y * desiredZoom * (1.0f - focusCoordinates.y))) / m_TextureSizeInPixels.y, 0.0f, 1.0f - desiredZoom);
                zoomRect.width = zoomRect.height = desiredZoom;

                return zoomRect;
            }
        };

        public class TabSwitcher : ExtendedBaseGUI.TabSwitcher
        {
            public TabSwitcher(int selectedEntry) : base(selectedEntry)
            {

            }

            protected float[] GetCorrectTabsWidth(Rect tabSwitcherRect, bool expandTabsToFitRect)
            {
                float[] tabsWidth = new float[m_Tabs.Length];
                float accumulateTabsWidth = 0;
                for (int i = 0; i < m_Tabs.Length; ++i)
                {
                    Vector2 tabSize = CalcSize(m_Tabs[i].Content, m_SelectedEntry == i ? m_Tabs[i].Style : m_Tabs[i].NotSelectedStyle, Tab.kTabContentPadding);

                    tabsWidth[i] = tabSize.x;
                    if (m_Tabs[i].IconTexture != null)
                    {
                        tabsWidth[i] += ButtonContentInnerPadding + tabSize.y;
                    }

                    accumulateTabsWidth += tabsWidth[i];
                }

                if (accumulateTabsWidth != tabSwitcherRect.width)
                {
                    if (accumulateTabsWidth < tabSwitcherRect.width && expandTabsToFitRect)
                    {
                        float widthDifference = tabSwitcherRect.width - accumulateTabsWidth;
                        float tabWidthDifference = widthDifference / m_Tabs.Length;

                        for (int i = 0; i < tabsWidth.Length; ++i)
                        {
                            tabsWidth[i] += tabWidthDifference;
                        }
                    }
                }

                return tabsWidth;
            }

            public bool Show(Rect tabSwitcherRect, bool expandTabsToFitRect = true)
            {
                float[] tabsWidth = GetCorrectTabsWidth(tabSwitcherRect, expandTabsToFitRect);

                bool selectionChanged = false;

                Rect tabRect = new Rect(tabSwitcherRect.x, tabSwitcherRect.y, 0, tabSwitcherRect.height);
                for (int i = 0; i < m_Tabs.Length; ++i)
                {
                    tabRect.width = tabsWidth[i];
                    GUIStyle tabGUIStyle = i == m_SelectedEntry ? m_Tabs[i].Style : m_Tabs[i].NotSelectedStyle;

                    bool buttonPressed = (m_Tabs[i].IconTexture == null ? Button(tabRect, m_Tabs[i].Content, style: tabGUIStyle, isSelected: i == m_SelectedEntry) :
                        IconButton(tabRect, m_Tabs[i].Content, style: tabGUIStyle, m_Tabs[i].IconTexture, 2.0f, true, isSelected: i == m_SelectedEntry)) && i != m_SelectedEntry;
                    if (buttonPressed)
                    {
                        selectionChanged = true;
                        m_SelectedEntry = i;
                    }

                    tabRect.x += tabRect.width;
                }

                m_LastTabPosition = new Vector2(tabRect.x, tabRect.yMax);

                return selectionChanged;
            }
        }

        public class SplitterState
        {
            protected int m_Id;
            public int Id => GUIUtility.GetControlID(($"ExtendedBaseGUI.Splitter.{m_Id}").GetHashCode(), FocusType.Passive);

            protected bool m_IsVertical;
            public bool IsVertical { get => m_IsVertical; }

            protected SplitterRect m_SplitterRectA;
            public Rect SplitterRectA { get => m_SplitterRectA.rect; set { m_SplitterRectA.rect = value; m_IsDirty = true; } }
            public float SplitterRectAMinSize { get => m_SplitterRectA.minSize; }

            protected SplitterRect m_SplitterRectB;
            public Rect SplitterRectB { get => m_SplitterRectB.rect; set { m_SplitterRectB.rect = value; m_IsDirty = true; } }
            public float SplitterRectBMinSize { get => m_SplitterRectB.minSize; }

            protected Rect m_SplitterRect;

            protected bool m_Dragging;
            public bool Dragging { get => m_Dragging; set => m_Dragging = value; }

            protected float m_DragInitialPosition;
            protected float m_MouseDragInitialPosition;

            public float Position { get => m_IsVertical ? m_SplitterRect.y : m_SplitterRect.x; }

            protected Color m_Color;
            public Color Color { set => m_Color = value; }

            protected bool m_IsDirty;

            protected struct SplitterRect
            {
                public Rect rect;
                public float minSize;

                public float GetMinValueVertical() => rect.y + minSize;
                public float GetMinValueHorizontal() => rect.x + minSize;
                public float GetMaxValueVertical() => rect.yMax - minSize;
                public float GetMaxValueHorizontal() => rect.xMax - minSize;
            }

            public SplitterState(bool isVertical, float minSizeA = 20.0f, float minSizeB = 20.0f)
            {
                m_Id = Random.Range(int.MinValue, int.MaxValue);

                m_IsVertical = isVertical;

                m_SplitterRectA = new SplitterRect() { rect = new Rect(), minSize = minSizeA };
                m_SplitterRectB = new SplitterRect() { rect = new Rect(), minSize = minSizeB };

                m_SplitterRect = Rect.zero;

                m_Color = Color.black;

                m_IsDirty = true;
            }

            public SplitterState(bool isVertical, Color color, float minSizeA = 20.0f, float minSizeB = 20.0f) : this(isVertical, minSizeA, minSizeB)
            {
                m_Color = color;
            }

            public void ResolveDirty()
            {
                if (m_IsDirty)
                {
                    if (m_SplitterRect == Rect.zero)
                    {
                        m_SplitterRect = m_IsVertical ? new Rect(m_SplitterRectA.rect.x, m_SplitterRectA.rect.yMax - 1, m_SplitterRectA.rect.width, 1) : new Rect(m_SplitterRectA.rect.xMax - 1, m_SplitterRectA.rect.y, 1, m_SplitterRectA.rect.height);
                    }

                    PerformDrag(m_IsVertical ? m_SplitterRect.y : m_SplitterRect.x, dragDistance: 0.0f);

                    m_IsDirty = false;
                }
            }

            public void PerformDrag(float dragInitialPosition, float dragDistance)
            {
                if (m_IsVertical) 
                {
                    m_SplitterRect = new Rect(m_SplitterRect.x, Mathf.Clamp(dragInitialPosition - dragDistance, m_SplitterRectA.GetMinValueVertical(), m_SplitterRectB.GetMaxValueVertical()), m_SplitterRectA.rect.width, 1);

                    m_SplitterRectA.rect = new Rect(m_SplitterRectA.rect.x, m_SplitterRectA.rect.y, m_SplitterRectA.rect.width, m_SplitterRect.y - m_SplitterRectA.rect.y);
                    m_SplitterRectB.rect = new Rect(m_SplitterRectB.rect.x, m_SplitterRect.yMax, m_SplitterRectB.rect.width, m_SplitterRectB.rect.yMax - m_SplitterRect.yMax);
                }
                else
                {
                    m_SplitterRect = new Rect(Mathf.Clamp(dragInitialPosition - dragDistance, m_SplitterRectA.GetMinValueHorizontal(), m_SplitterRectB.GetMaxValueHorizontal()), m_SplitterRect.y, 1, m_SplitterRectA.rect.height);

                    m_SplitterRectA.rect = new Rect(m_SplitterRectA.rect.x, m_SplitterRectA.rect.y, m_SplitterRect.x - m_SplitterRectA.rect.x, m_SplitterRectA.rect.height);
                    m_SplitterRectB.rect = new Rect(m_SplitterRect.xMax, m_SplitterRectB.rect.y, m_SplitterRectB.rect.xMax - m_SplitterRect.xMax, m_SplitterRectB.rect.height);
                }
            }

            public void PerformDrag(float dragDistance)
            {
                PerformDrag(m_DragInitialPosition, dragDistance);
            }

            protected void ResizeRects()
            {
                if (m_IsVertical)
                {
                    m_SplitterRectA.rect = new Rect(m_SplitterRectA.rect.x, m_SplitterRectA.rect.y, m_SplitterRectA.rect.width, m_SplitterRect.y - m_SplitterRectA.rect.y);
                    m_SplitterRectB.rect = new Rect(m_SplitterRectB.rect.x, m_SplitterRect.yMax, m_SplitterRectB.rect.width, m_SplitterRectB.rect.yMax - m_SplitterRect.yMax);
                }
                else
                {
                    m_SplitterRectA.rect = new Rect(m_SplitterRectA.rect.x, m_SplitterRectA.rect.y, m_SplitterRect.x - m_SplitterRectA.rect.x, m_SplitterRectA.rect.height);
                    m_SplitterRectB.rect = new Rect(m_SplitterRect.xMax, m_SplitterRectB.rect.y, m_SplitterRectB.rect.xMax - m_SplitterRect.xMax, m_SplitterRectB.rect.height);
                }
            }

            public Rect GetSplitterDragRect(int offset = 2)
            {
                return m_IsVertical ? new Rect(m_SplitterRect.x, m_SplitterRect.y - offset, m_SplitterRect.width, m_SplitterRect.height + 2 * offset) :
                    new Rect(m_SplitterRect.x - offset, m_SplitterRect.y, m_SplitterRect.width + 2 * offset, m_SplitterRect.height);
            }

            public void BeginDrag(Vector2 mousePosition)
            {
                m_DragInitialPosition = m_IsVertical ? m_SplitterRect.y : m_SplitterRect.x;
                m_MouseDragInitialPosition = m_IsVertical ? mousePosition.y : mousePosition.x;

                m_Dragging = true;
            }

            public float GetDragDistance(Vector2 mousePosition)
            {
                float currentMousePosition = m_IsVertical ? mousePosition.y : mousePosition.x;
                return m_MouseDragInitialPosition - currentMousePosition;
            }

            public void DrawSplitter()
            {
                EditorGUI.DrawRect(m_SplitterRect, m_Color);
            }
        }

    }

    public class ExtendedGUILayout
    {
        public static Vector2 CalcSize(GUIContent content, GUIStyle style, float contentPadding = 0.0f)
        {
            return ExtendedGUI.CalcSize(content, style, contentPadding);
        }

        public static Texture2D CreateSingleColorTexture(Color color)
        {
            return ExtendedGUI.CreateSingleColorTexture(color);
        }

        protected static void Internal_CenterLabelHorizontally(GUIContent content, GUIStyle style)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(content, style);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        protected static void Internal_CenterLabelVertically(GUIContent content, GUIStyle style)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.Label(content, style);

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        protected static void Internal_CenterLabel(GUIContent content, GUIStyle style)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            Internal_CenterLabelHorizontally(content, style);

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        public static void CenterLabelHorizontally(GUIContent content, GUIStyle style)
        {
            Internal_CenterLabelHorizontally(content, style);
        }

        public static void CenterLabelHorizontally(GUIContent content)
        {
            Internal_CenterLabelHorizontally(content, EditorStyles.label);
        }

        public static void CenterLabelHorizontally(string label, GUIStyle style)
        {
            Internal_CenterLabelHorizontally(new GUIContent(label), style);
        }

        public static void CenterLabelHorizontally(string label)
        {
            Internal_CenterLabelHorizontally(new GUIContent(label), EditorStyles.label);
        }

        public static void CenterLabelVertically(GUIContent content, GUIStyle style)
        {
            Internal_CenterLabelVertically(content, style);
        }

        public static void CenterLabelVertically(GUIContent content)
        {
            Internal_CenterLabelVertically(content, EditorStyles.label);
        }

        public static void CenterLabelVertically(string label, GUIStyle style)
        {
            Internal_CenterLabelVertically(new GUIContent(label), style);
        }

        public static void CenterLabelVertically(string label)
        {
            Internal_CenterLabelVertically(new GUIContent(label), EditorStyles.label);
        }

        public static void CenterLabel(GUIContent content, GUIStyle style)
        {
            Internal_CenterLabel(content, style);
        }

        public static void CenterLabel(GUIContent content)
        {
            Internal_CenterLabel(content, EditorStyles.label);
        }

        public static void CenterLabel(string label, GUIStyle style)
        {
            Internal_CenterLabel(new GUIContent(label), style);
        }

        public static void CenterLabel(string label)
        {
            Internal_CenterLabel(new GUIContent(label), EditorStyles.label);
        }

        public static bool Button(GUIContent content, GUIStyle style, bool fixedContentSize, bool isSelected, bool labelIsOnTop = true, bool enabled = true)
        {
            Vector2 buttonSize = CalcSize(content, style, ExtendedGUI.ButtonContentInnerPadding);

            GUILayoutOption[] buttonRectParams = fixedContentSize ? new GUILayoutOption[] { GUILayout.MinWidth(buttonSize.x), GUILayout.MaxWidth(buttonSize.x), GUILayout.MinHeight(buttonSize.y), GUILayout.MaxHeight(buttonSize.y) } : new GUILayoutOption[] { };
            Rect buttonRect = GUILayoutUtility.GetRect(buttonSize.x, buttonSize.y, buttonRectParams);

            return ExtendedGUI.Button(buttonRect, content, style, isSelected, labelIsOnTop, enabled);
        }

        public static bool Button(GUIContent content, bool fixedContentSize, bool isSelected, bool labelIsOnTop = true, bool enabled = true)
        {
            return Button(content, GUIStyle.none, fixedContentSize, isSelected, labelIsOnTop, enabled);
        }

        public static bool Button(string label, GUIStyle style, bool fixedContentSize, bool isSelected, bool labelIsOnTop = true, bool enabled = true)
        {
            return Button(new GUIContent(label), style, fixedContentSize, isSelected, labelIsOnTop, enabled);
        }

        public static bool Button(string label, bool fixedContentSize, bool isSelected, bool labelIsOnTop = true, bool enabled = true)
        {
            return Button(new GUIContent(label), GUIStyle.none, fixedContentSize, isSelected, labelIsOnTop, enabled);
        }

        public static bool IconButton(GUIContent content, GUIStyle style, bool fixedContentSize, Texture2D iconTexture, float iconPadding, bool iconIsOnTop, bool isSelected, bool enabled = true)
        {
            Vector2 buttonSize = CalcSize(content, style, ExtendedGUI.ButtonContentInnerPadding);

            if (!string.IsNullOrEmpty(content.text) && !string.IsNullOrWhiteSpace(content.text))
            {
                buttonSize.x += buttonSize.y;
            }

            GUILayoutOption[] buttonRectParams = fixedContentSize ? new GUILayoutOption[] { GUILayout.MinWidth(buttonSize.x), GUILayout.MaxWidth(buttonSize.x), GUILayout.MinHeight(buttonSize.y), GUILayout.MaxHeight(buttonSize.y) } : new GUILayoutOption[] { };
            Rect buttonRect = GUILayoutUtility.GetRect(buttonSize.x, buttonSize.y, buttonRectParams);

            return ExtendedGUI.IconButton(buttonRect, content, style, iconTexture, iconPadding, iconIsOnTop, isSelected, enabled);
        }

        public static bool IconButton(GUIContent content, bool fixedContentSize, Texture2D iconTexture, float iconPadding, bool iconIsOnTop, bool isSelected, bool enabled = true)
        {
            return IconButton(content, GUIStyle.none, fixedContentSize, iconTexture, iconPadding, iconIsOnTop, isSelected, enabled);
        }

        public static bool IconButton(string label, GUIStyle style, bool fixedContentSize, Texture2D iconTexture, float iconPadding, bool iconIsOnTop, bool isSelected, bool enabled = true)
        {
            return IconButton(new GUIContent(label), style, fixedContentSize, iconTexture, iconPadding, iconIsOnTop, isSelected, enabled);
        }

        public static bool IconButton(string label, bool fixedContentSize, Texture2D iconTexture, float iconPadding, bool iconIsOnTop, bool isSelected, bool enabled = true)
        {
            return IconButton(new GUIContent(label), GUIStyle.none, fixedContentSize, iconTexture, iconPadding, iconIsOnTop, isSelected, enabled);
        }

        public class TabSwitcher : ExtendedBaseGUI.TabSwitcher
        {
            public TabSwitcher(int selectedEntry) : base(selectedEntry)
            {

            }

            public bool Show(params ExtendedBaseGUI.ExtendedBaseGUIOption[] options)
            {
                GUILayout.BeginHorizontal();

                bool hasSelectedOption = ExtendedBaseGUI.HasOptionOfType(options, ExtendedBaseGUI.ExtendedBaseGUIOption.OptionType.Selected, out bool select);
                bool hasIconPaddingOption = ExtendedBaseGUI.HasOptionOfType(options, ExtendedBaseGUI.ExtendedBaseGUIOption.OptionType.IconPadding, out float iconPadding);
                bool hasIconIsOnTopOption = ExtendedBaseGUI.HasOptionOfType(options, ExtendedBaseGUI.ExtendedBaseGUIOption.OptionType.IconIsOnTop, out bool iconIsOnTop);

                bool selectionChanged = false;
                for (int i = 0; i < m_Tabs.Length; ++i)
                {
                    GUIStyle tabGUIStyle = i == m_SelectedEntry ? m_Tabs[i].Style : m_Tabs[i].NotSelectedStyle;
                    bool iconIsOnTopInTab = i == m_SelectedEntry ? true : (hasIconIsOnTopOption ? iconIsOnTop : true);

                    bool buttonPressed = m_Tabs[i].IconTexture != null ? IconButton(m_Tabs[i].Content, style: tabGUIStyle, fixedContentSize: false, m_Tabs[i].IconTexture, iconPadding: hasIconPaddingOption ? iconPadding : 0,
                        iconIsOnTop: iconIsOnTopInTab, isSelected: hasSelectedOption ? select : i == m_SelectedEntry, enabled: true) : Button(m_Tabs[i].Content, style: tabGUIStyle, fixedContentSize: false,
                        isSelected: hasSelectedOption ? select : i == m_SelectedEntry, labelIsOnTop: iconIsOnTopInTab, enabled: true);

                    if (buttonPressed && i != m_SelectedEntry)
                    {
                        selectionChanged = true;
                        m_SelectedEntry = i;
                    }
                }

                GUILayout.EndHorizontal();

                return selectionChanged;
            }
        }

        public static ExtendedBaseGUI.ExtendedBaseGUIOption Selected(bool value)
        {
            return new ExtendedBaseGUI.ExtendedBaseGUIOption(ExtendedBaseGUI.ExtendedBaseGUIOption.OptionType.Selected, value);
        }

        public static ExtendedBaseGUI.ExtendedBaseGUIOption IconPadding(float value)
        {
            return new ExtendedBaseGUI.ExtendedBaseGUIOption(ExtendedBaseGUI.ExtendedBaseGUIOption.OptionType.IconPadding, value);
        }

        public static ExtendedBaseGUI.ExtendedBaseGUIOption IconIsOnTop(bool value)
        {
            return new ExtendedBaseGUI.ExtendedBaseGUIOption(ExtendedBaseGUI.ExtendedBaseGUIOption.OptionType.IconIsOnTop, value);
        }
    }
}
