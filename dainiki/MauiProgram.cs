using System.IO;
using dainiki.Components.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace dainiki;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "dainiki.db");
        builder.Services.AddDbContext<DainikiDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
        
#endif

        var app = builder.Build();
        InitializeDatabase(app);

        return app;
    }

    private static void InitializeDatabase(MauiApp app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DainikiDbContext>();
        dbContext.Database.EnsureCreated();
    }
} 
