
namespace EAS
{
    [System.Serializable]
    [EASEventColor("#FFA500"), EASEventCategory("Visual Effects"), EASTooltip("Play Particle Systems")]
    public class EASPlayParticlesEvent : EASEvent
    {
#if UNITY_EDITOR
        public override string GetErrorMessage()
        {
            return "";
        }

        public override bool HasError(EASBaseController owner)
        {
            return false;
        }
#endif // UNITY_EDITOR
    }
}
