using AWS4RequestSigner;
using AWS4RequestSigner.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAws4RequestSigner(this IServiceCollection services)
        {
            services.AddTransient<IAws4RequestSignerHelper, Aws4RequestSignerHelper>();
            services.AddTransient<IAws4RequestSigner, Aws4RequestSigner>();

            return services;
        }
    }
}
