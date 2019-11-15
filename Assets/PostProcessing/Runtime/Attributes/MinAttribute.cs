namespace UnityEngine.PostProcessing
{
    public sealed class PostMinAttribute : PropertyAttribute
    {
        public readonly float min;

        public PostMinAttribute(float min)
        {
            this.min = min;
        }
    }
}
