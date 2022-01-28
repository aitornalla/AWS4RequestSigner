using System.Net.Http;
using System.Threading.Tasks;

namespace AWS4RequestSigner.Interfaces
{
    public interface IAws4RequestSigner
    {
        void SignRequest(ref HttpRequestMessage httpRequestMessage, Aws4SignSettings aws4SignSettings);
        Task SignRequestAsync(ref HttpRequestMessage httpRequestMessage, Aws4SignSettings aws4SignSettings);
    }
}
