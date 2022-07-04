using OCore.Entities.Data;

namespace OCore.Samples.Minimal.Host
{
    [DataEntity("ShortenedUrl", keyStrategy: KeyStrategy.Identity, dataEntityMethods: DataEntityMethods.All)]
    public interface IShortenedUrl : IDataEntity<ShortenedUrl>
    {
        Task<string> Visit();
    }
}