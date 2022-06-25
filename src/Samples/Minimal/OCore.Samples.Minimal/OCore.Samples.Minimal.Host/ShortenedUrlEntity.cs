using OCore.Entities.Data;

namespace OCore.Samples.Minimal.Host
{
    public class ShortenedUrlEntity : DataEntity<ShortenedUrl>, IShortenedUrl
    {
        public async Task<string> Visit()
        {
            State.TimesVisited++;
            await WriteStateAsync();
            return State.RedirectTo;
        }
    }
}