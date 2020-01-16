namespace OCore.Authorization
{
    public class AuthorizeOptions
    {
        public string ApiKeyHeader { get; set; }
        public string TokenHeader { get; set; }
        public string TenantHeader { get; set; }
    }
}