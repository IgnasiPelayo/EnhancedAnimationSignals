
namespace EAS
{
    [System.Serializable]
    [EASEventColor("#FFA500"), EASEventCategory("Visual Effects"), EASEventTooltip("Play Particle Systems")]
    public class EASPlayParticlesEvent : EASEvent
    {
#if UNITY_EDITOR
        public override string GetErrorMessage(EASBaseController owner)
        {
            return base.GetErrorMessage(owner);
        }
#endif // UNITY_EDITOR
    }
}
