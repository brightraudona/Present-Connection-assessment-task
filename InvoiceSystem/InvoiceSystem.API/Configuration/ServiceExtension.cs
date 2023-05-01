using InvoiceSystem.Core.ExternalServices;
using InvoiceSystem.Core.Service;
using InvoiceSystem.Core.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InvoiceSystem.API.Configuration
{
    public static class ServiceExtension
    {
        public static IServiceCollection ConfigureDependencyServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IInvoiceService>(provider => new InvoiceService(provider.GetRequiredService<ILogger<InvoiceService>>(), configuration));

            return services;
        }
    }
}
