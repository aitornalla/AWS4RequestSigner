using System;
using System.Collections.Specialized;
using System.Net.Http;

namespace AWS4RequestSigner.Interfaces
{
    internal interface IAws4RequestSignerHelper
    {
        void AddSignatureToRequest(ref HttpRequestMessage httpRequestMessage, string accessKey, string xAmzDate, string region, string serviceName, string signedHeaders, string signature);
        string GetCanonicalHeaders(ref HttpRequestMessage httpRequestMessage, out string xAmzDate, out string signedHeaders);
        string GetCanonicalHttpRequestMethod(HttpMethod httpMethod);
        string GetCanonicalPayloadHash(HttpContent httpContent);
        string GetCanonicalQueryParameters(NameValueCollection queryParameters);
        string GetCanonicalUri(Uri uri);
        string GetSignature(string stringToSign, string secretKey, string xAmzDate, string region, string serviceName);
        string GetStringToSign(string canonicalRequest, string xAmzDate, string region, string serviceName);
    }
}
