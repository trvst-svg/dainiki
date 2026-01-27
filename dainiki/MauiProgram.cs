using System.IO;
using dainiki.Components.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using QuestPDF.Infrastructure;

namespace dainiki
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "dainiki.db");
            builder.Services.AddDbContext<DainikiDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddScoped<ISettings, Settings>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJournalService, JournalService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
            builder.Services.AddScoped<IPdfExportService, PdfExportService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            QuestPDF.Settings.License = LicenseType.Community;

            MauiApp app = builder.Build();
            InitializeDatabase(app);

            return app;
        }

        private static void InitializeDatabase(MauiApp app)
        {
            using IServiceScope scope = app.Services.CreateScope();
            DainikiDbContext dbContext = scope.ServiceProvider.GetRequiredService<DainikiDbContext>();
            dbContext.Database.EnsureCreated();
            DainikiSeedData.SeedAsync(dbContext).GetAwaiter().GetResult();
        }
    }
}
