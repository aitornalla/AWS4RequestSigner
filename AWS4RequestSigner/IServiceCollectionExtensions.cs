using AWS4RequestSigner.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AWS4RequestSigner
{
    public static class IServiceCollectionExtensions
    {
        public static void AddAws4RequestSigner(this IServiceCollection services)
        {
            services.AddTransient<IAws4RequestSignerHelper, Aws4RequestSignerHelper>();
            services.AddTransient<IAws4RequestSigner, Aws4RequestSigner>();
        }
    }
}
