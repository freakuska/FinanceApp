using FinanceApp.Infrastructure.Services;

namespace FinanceApp.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Регистрация всех сервисов
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IFinancialOperationService, FinancialOperationService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}