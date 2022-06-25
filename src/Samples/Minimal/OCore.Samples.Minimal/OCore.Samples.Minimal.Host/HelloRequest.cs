using Orleans;

namespace OCore.Samples.Minimal.Host
{
    [Serializable]
    [GenerateSerializer]
    public class HelloRequest
    {
        [Id(0)]
        public string Name { get; set; }
    }
}