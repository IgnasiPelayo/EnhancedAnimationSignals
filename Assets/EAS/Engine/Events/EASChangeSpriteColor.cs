using UnityEngine;

namespace EAS
{
    [System.Serializable]
    [EASEventColor("#DE5C5C"), EASEventCategory("Visual Effects"), EASEventTooltip("Changes the color of the specified sprite renderer")]
    public class EASChangeSpriteColor : EASBaseEvent
    {
        [Header("Sprite Settings")]
        [SerializeField, Tooltip("The SpriteRenderer component whose color will be changed.")]
        protected EASReference<SpriteRenderer> m_SpriteRenderer = new EASReference<SpriteRenderer>();

        [Space(10)]

        [Header("Color Settings")]
        [SerializeField, Tooltip("The target color to apply to the sprite.")]
        protected Color m_TargetColor = Color.white;

        public override void OnStart(float currentFrame)
        {
            if (m_SpriteRenderer != null && m_SpriteRenderer.Instance != null)
            {
                m_SpriteRenderer.Instance.color = m_TargetColor;
            }
        }

#if UNITY_EDITOR
        public override string GetErrorMessage(EASBaseController owner)
        {
            string errorMessage = string.Empty;
            if (m_SpriteRenderer != null)
            {
                SpriteRenderer spriteRenderer = m_SpriteRenderer.Resolve(owner);
                if (spriteRenderer == null)
                {
                    errorMessage += "SpriteRenderer is not set";
                }
            }

            return errorMessage;
        }

        public override void OnStartEditor(int currentFrame, IEASEditorBridge editorBridge)
        {
            SpriteRenderer spriteRenderer = m_SpriteRenderer.Resolve(editorBridge.Controller);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = m_TargetColor;
            }
        }
#endif // UNITY_EDITOR
    }
}

