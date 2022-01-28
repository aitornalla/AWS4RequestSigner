namespace AWS4RequestSigner
{
    public class Aws4SignSettings
    {
        public string AccessKey { get; set; }
        public string Region { get; set; }
        public string SecretKey { get; set; }
        public string ServiceName { get; set; }
    }
}
