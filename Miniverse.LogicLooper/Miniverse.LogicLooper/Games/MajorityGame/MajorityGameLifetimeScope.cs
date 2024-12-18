﻿using Miniverse.LogicLooper.LooperTasks;

namespace Miniverse.LogicLooperServer;

public static class MajorityGameLifetimeScope
{
    public static void Configure(IServiceCollection services)
    {
        services.AddScoped<MajorityGameRoom>();
        services.AddScoped<MajorityGameMessageReceiver>();
        services.AddScoped<RoomInfoProvider>();
        services.AddScoped<QuestionService>();
        services.AddTransient<QuestionSession>();
    }
}
