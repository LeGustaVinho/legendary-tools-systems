namespace LegendaryTools.TagSystem
{
    public interface ITaggable
    {
        Tag[] Tags { get; }

        bool ContainsTag(string tagId);
    }
}