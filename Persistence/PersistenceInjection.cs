using Application;
using Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repos;

namespace Persistence;

public static class PersistenceInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddDbContext<DataContext>(options=>options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        service.AddScoped<IKatoInfoRepository, KatoInfoRepository>();
        service.AddScoped<IKatoInfoService, KatoInfoService>();
        return service;
    }
}