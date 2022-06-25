using Orleans;

namespace OCore.Samples.Minimal.Host
{
    [Serializable]
    [GenerateSerializer]
    public class ShortenedUrl
    {
        [Id(0)]
        public string RedirectTo { get; set; }

        [Id(1)]
        public int TimesVisited { get; set; }
    }
}