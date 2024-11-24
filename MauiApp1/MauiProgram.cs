using Microsoft.Extensions.DependencyInjection;
using MauiApp1.Services;
using System.IO;

namespace MauiApp1
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

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "MauiApp1.db3");
            builder.Services.AddSingleton<DatabaseService>(s => new DatabaseService(dbPath));

            return builder.Build();
        }
    }
}