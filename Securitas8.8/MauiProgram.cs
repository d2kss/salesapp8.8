using Microsoft.Extensions.Logging;
using Securitas8._8.Data;
using Securitas8._8.Data.Repositories;
using Securitas8._8.Services;
using Securitas8._8.Services.Interface;

namespace Securitas8._8
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });
            //DbInitializer.Initialize();
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<HttpClient>();
            // Add device-specific services used by the Securitas8.Shared project
            builder.Services.AddSingleton<OrderService>();
            builder.Services.AddScoped<OrderWorkflowStateService>();
            builder.Services.AddSingleton<LocalizationService>();
            builder.Services.AddSingleton<ILocalizationLoader, MauiLocalizationLoader>();
            builder.Services.AddSingleton<ProductRepository>();
            builder.Services.AddSingleton<OrderRepository>();

            builder.Services.AddSingleton<ProductService>();
            builder.Services.AddSingleton<OrderService>();
            builder.Services.AddScoped<CartState>();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
