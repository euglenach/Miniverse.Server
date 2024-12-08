namespace Miniverse.LogicLooperServer;

public static class MajorityGameLifetimeScope
{
    public static void Configure(IServiceCollection services)
    {
        services.AddScoped<MajorityGameRoom>();
    }
}
