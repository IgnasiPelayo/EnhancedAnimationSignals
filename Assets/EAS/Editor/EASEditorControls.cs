using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EAS
{
    public class EASEditorControls
    {
        public void OnGUI(Rect rect)
        {
            EditorGUI.DrawRect(rect, EASSkin.ToolbarBackgroundColor);
            Rect buttonRect = new Rect(0, 0, rect.height * 1.4f, rect.height);

            if (ControlRect(ref buttonRect, EASSkin.Icon("d_Animation.FirstKey")))
            {

            }

            if (ControlRect(ref buttonRect, EASSkin.Icon("d_Animation.PrevKey")))
            {

            }

            if (ControlRect(ref buttonRect, EASSkin.Icon(EASEditor.Instance.Playing ? "d_PauseButton" : "d_PlayButton"), 2, EASEditor.Instance.Playing))
            {
                EASEditor.Instance.Playing = !EASEditor.Instance.Playing;
            }

            if (ControlRect(ref buttonRect, EASSkin.Icon("d_Animation.NextKey")))
            {

            }

            if (ControlRect(ref buttonRect, EASSkin.Icon("d_Animation.LastKey")))
            {

            }

            Rect loopButtonRect = new Rect(buttonRect.x, buttonRect.y, 40, buttonRect.height);
            if (GUI.Button(loopButtonRect, GUIContent.none, EditorStyles.toolbarButton))
            {
                EASEditor.Instance.Loop = !EASEditor.Instance.Loop;
            }
            ExtendedGUI.ExtendedGUI.CenterLabel(loopButtonRect, new GUIContent("Loop"), EASEditor.Instance.Loop ? EditorStyles.whiteLabel : EditorStyles.label);
            if (EASEditor.Instance.Loop)
            {
                EditorGUI.DrawRect(loopButtonRect, EASSkin.SelectedWhiteColor);
            }

            Rect framesRect = new Rect(loopButtonRect.xMax + loopButtonRect.width / 2.0f, loopButtonRect.y, loopButtonRect.width, loopButtonRect.height);
            framesRect = ExtendedGUI.ExtendedGUI.GetInnerRect(framesRect, framesRect.width, framesRect.height - 4);
            int currentFrame = EASEditor.Instance.CurrentFrame;
            EditorGUI.BeginChangeCheck();
            currentFrame = EditorGUI.DelayedIntField(framesRect, currentFrame);
            if (EditorGUI.EndChangeCheck())
            {
                EASEditor.Instance.CurrentFrame = currentFrame;
                GUI.FocusControl(null);
            }

            Rect controllerRect = new Rect(rect.xMax - 200, rect.y, 200, rect.height);
            if (ExtendedGUI.ExtendedGUI.IconButton(controllerRect, new GUIContent(EASEditor.Instance.Controller.name), EditorStyles.toolbarButton, EASSkin.Icon(typeof(GameObject)), 3.0f, iconIsOnTop: true, isSelected: false))
            {
                EditorGUIUtility.PingObject(EASEditor.Instance.Controller);
                Selection.activeGameObject = EASEditor.Instance.Controller.gameObject;
            }

            buttonRect.x = controllerRect.x - buttonRect.width * 1.5f;
            if (ControlRect(ref buttonRect, EASSkin.Icon("d_UnityEditor.InspectorWindow"), 2, false, -1))
            {

            }

            if (ControlRect(ref buttonRect, EASSkin.CustomIcon("import_export"), 3, false, -1))
            {

            }

            buttonRect.x -= buttonRect.width / 2.0f;
            if (ControlRect(ref buttonRect, EASEditor.Instance.ShowParticleSystems ? EASSkin.Icon("d_Particle Effect") : EASSkin.CustomIcon("particles_off"), 3, false, -1))
            {
                EASEditor.Instance.ShowParticleSystems = !EASEditor.Instance.ShowParticleSystems;
            }

            if (ControlRect(ref buttonRect, EASSkin.Icon(EASEditor.Instance.Mute ? "d_SceneViewAudio" : "d_SceneViewAudio On"), 2, false, -1))
            {
                EASEditor.Instance.Mute = !EASEditor.Instance.Mute;
            }

            Rect separatorRect = new Rect(0, rect.yMax - 1.0f, rect.width, 1.0f);
            EditorGUI.DrawRect(separatorRect, EASSkin.SeparatorColor);

            HandleInput();
        }

        protected bool ControlRect(ref Rect rect, Texture2D icon = null, float iconPadding = 2, bool isSelected = false, int direction = 1)
        {
            bool buttonPressed = ExtendedGUI.ExtendedGUI.IconButton(rect, GUIContent.none, EditorStyles.toolbarButton, icon, iconPadding, iconIsOnTop: true, isSelected: false);
            if (isSelected)
            {
                EditorGUI.DrawRect(rect, EASSkin.SelectedWhiteColor);
            }

            rect.x += rect.width * direction;

            return buttonPressed;
        }

        protected void HandleInput()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Space)
                {
                    EASEditor.Instance.Playing = !EASEditor.Instance.Playing;
                    EASEditor.Instance.Repaint();
                }
            }
        }
    }
}
