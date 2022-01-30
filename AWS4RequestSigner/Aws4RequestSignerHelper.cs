using AWS4RequestSigner.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace AWS4RequestSigner
{
    internal class Aws4RequestSignerHelper : IAws4RequestSignerHelper
    {
        private const string AMZ_DATE_FORMAT = "yyyyMMddTHHmmssZ";
        private const string AWS4_HMAC_SHA256_ALGORITHM = "AWS4-HMAC-SHA256";
        private const string AWS4_LITERAL = "AWS4";
        private const string AWS4_REQUEST_LITERAL = "aws4_request";
        private const string EMPTY_STRING_PAYLOAD_HASH = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
        private const string X_AMZ_DATE_HEADER_NAME = "X-Amz-Date";

        public void AddSignatureToRequest(ref HttpRequestMessage httpRequestMessage, string accessKey, string xAmzDate, string region, string serviceName, string signedHeaders, string signature)
        {
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(AWS4_HMAC_SHA256_ALGORITHM, $"Credential={accessKey}/{xAmzDate.Substring(0, 8)}/{region}/{serviceName}/{AWS4_REQUEST_LITERAL}, SignedHeaders={signedHeaders}, Signature={signature}");
        }

        public string GetCanonicalHeaders(ref HttpRequestMessage httpRequestMessage, out string xAmzDate, out string signedHeaders)
        {
            if (string.IsNullOrEmpty(httpRequestMessage.Headers.Host))
                httpRequestMessage.Headers.Host = httpRequestMessage.RequestUri.Host;

            if (!httpRequestMessage.Headers.Contains(X_AMZ_DATE_HEADER_NAME))
            {
                xAmzDate = DateTime.UtcNow.ToString(AMZ_DATE_FORMAT);

                httpRequestMessage.Headers.Add(X_AMZ_DATE_HEADER_NAME, xAmzDate);
            }
            else
            {
                xAmzDate = httpRequestMessage.Headers.First(f => f.Key.Equals(X_AMZ_DATE_HEADER_NAME)).Value.First();
            }

            var canonicalHeadersSb = new StringBuilder();
            var canonicalSignedHeadersList = new List<string>();

            foreach (var header in httpRequestMessage.Headers.OrderBy(o => o.Key.ToLowerInvariant().Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var key = header.Key.ToLowerInvariant().Trim();

                canonicalHeadersSb.Append(key);
                canonicalHeadersSb.Append(":");
                canonicalHeadersSb.Append(string.Join(",", header.Value.Select(s => string.Join(" ", s.Trim().Split(' ').Where(w => !string.IsNullOrWhiteSpace(w))))));
                canonicalHeadersSb.Append("\n");

                canonicalSignedHeadersList.Add(key);
            }

            canonicalHeadersSb.Append("\n");
            signedHeaders = string.Join(";", canonicalSignedHeadersList);
            canonicalHeadersSb.Append(signedHeaders);
            canonicalHeadersSb.Append("\n");

            return canonicalHeadersSb.ToString();
        }

        public string GetCanonicalHttpRequestMethod(HttpMethod httpMethod)
        {
            return httpMethod.Method + "\n";
        }

        public string GetCanonicalPayloadHash(HttpContent httpContent)
        {
            if (httpContent is null)
            {
                return EMPTY_STRING_PAYLOAD_HASH;
            }
            else
            {
                return HexEncode(Sha256Hash(httpContent.ReadAsByteArrayAsync().Result));
            }
        }

        public string GetCanonicalQueryParameters(NameValueCollection queryParameters)
        {
            if (queryParameters.Count == 0)
                return "\n";

            var sortedDictionary = new SortedDictionary<string, IOrderedEnumerable<string>>(StringComparer.Ordinal);

            foreach (var key in queryParameters.AllKeys)
            {
                var qValues = queryParameters[key]
                    .Split(',')
                    .OrderBy(o => o, StringComparer.Ordinal);

                sortedDictionary.Add(key, qValues);
            }

            return string.Join("&", sortedDictionary.SelectMany(sm => sm.Value.Select(s => $"{Uri.EscapeDataString(sm.Key)}={Uri.EscapeDataString(s)}"))) + "\n";
        }

        public string GetCanonicalUri(Uri uri)
        {
            return string.Join("/", uri.AbsolutePath.Split('/').Select(s => Uri.EscapeDataString(s))) + "\n";
        }

        public string GetSignature(string stringToSign, string secretKey, string xAmzDate, string region, string serviceName)
        {
            var kSecret = Encoding.UTF8.GetBytes($"{AWS4_LITERAL}{secretKey}");
            var kDate = HmacSha256(kSecret, xAmzDate.Substring(0, 8));
            var kRegion = HmacSha256(kDate, region);
            var kService = HmacSha256(kRegion, serviceName);
            var kSigning = HmacSha256(kService, AWS4_REQUEST_LITERAL);

            return HexEncode(HmacSha256(kSigning, stringToSign));
        }

        public string GetStringToSign(string canonicalRequest, string xAmzDate, string region, string serviceName)
        {
            var stringToSignSb = new StringBuilder();

            stringToSignSb.Append(AWS4_HMAC_SHA256_ALGORITHM);
            stringToSignSb.Append("\n");
            stringToSignSb.Append(xAmzDate);
            stringToSignSb.Append("\n");
            stringToSignSb.Append($"{xAmzDate.Substring(0, 8)}/{region}/{serviceName}/{AWS4_REQUEST_LITERAL}");
            stringToSignSb.Append("\n");
            stringToSignSb.Append(HexEncode(Sha256Hash(Encoding.UTF8.GetBytes(canonicalRequest))));

            return stringToSignSb.ToString();
        }

        private string HexEncode(byte[] arrayToHexEncode)
        {
            var hexSb = new StringBuilder(arrayToHexEncode.Length * 2);

            foreach (var b in arrayToHexEncode)
            {
                hexSb.AppendFormat("{0:x2}", b);
            }

            return hexSb.ToString();
        }

        private byte[] HmacSha256(byte[] key, string data)
        {
            using (var hmacSha256 = new HMACSHA256(key))
            {
                return hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            }
        }

        private byte[] Sha256Hash(byte[] arrayToHash)
        {
            using (var sha256 = SHA256.Create())
            {
                var sha256HashedArray = sha256.ComputeHash(arrayToHash);

                return sha256HashedArray;
            }
        }
    }
}
