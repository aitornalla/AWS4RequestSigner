using AWS4RequestSigner.Interfaces;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AWS4RequestSigner
{
    internal class Aws4RequestSigner : IAws4RequestSigner
    {
        private readonly IAws4RequestSignerHelper _aws4RequestSignerHelper;

        public Aws4RequestSigner(IAws4RequestSignerHelper aws4RequestSignerHelper)
        {
            _aws4RequestSignerHelper = aws4RequestSignerHelper;
        }

        public void SignRequest(ref HttpRequestMessage httpRequestMessage, Aws4SignSettings aws4SignSettings)
        {
            SignRequestAsync(ref httpRequestMessage, aws4SignSettings).RunSynchronously();
        }

        public Task SignRequestAsync(ref HttpRequestMessage httpRequestMessage, Aws4SignSettings aws4SignSettings)
        {
            if (httpRequestMessage is null)
                return Task.CompletedTask;

            var canonicalRequestSb = new StringBuilder();

            // Append canonical http request method
            canonicalRequestSb.Append(_aws4RequestSignerHelper.GetCanonicalHttpRequestMethod(httpRequestMessage.Method));
            // Append canonical uri
            canonicalRequestSb.Append(_aws4RequestSignerHelper.GetCanonicalUri(httpRequestMessage.RequestUri));
            // Append canonical query parameters
            canonicalRequestSb.Append(_aws4RequestSignerHelper.GetCanonicalQueryParameters(HttpUtility.ParseQueryString(httpRequestMessage.RequestUri.Query)));
            // Append canonical headers
            canonicalRequestSb.Append(_aws4RequestSignerHelper.GetCanonicalHeaders(ref httpRequestMessage, out var xAmzDate, out var signedHeaders));
            // Append canonical payload hash
            canonicalRequestSb.Append(_aws4RequestSignerHelper.GetCanonicalPayloadHash(httpRequestMessage.Content));
            // Get string to sign
            var stringToSign = _aws4RequestSignerHelper.GetStringToSign(canonicalRequestSb.ToString(), xAmzDate, aws4SignSettings.Region, aws4SignSettings.ServiceName);
            // Get signature
            var signature = _aws4RequestSignerHelper.GetSignature(stringToSign, aws4SignSettings.SecretKey, xAmzDate, aws4SignSettings.Region, aws4SignSettings.ServiceName);
            // Add signature to request
            _aws4RequestSignerHelper.AddSignatureToRequest(ref httpRequestMessage, aws4SignSettings.AccessKey, xAmzDate, aws4SignSettings.Region, aws4SignSettings.ServiceName, signedHeaders, signature);

            return Task.CompletedTask;
        }
    }
}
