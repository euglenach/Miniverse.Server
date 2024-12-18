using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Mvc;
using Miniverse.ServiceDiscovery;
using MiniverseShared.WebAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// DI
if(Environment.GetEnvironmentVariable("MAGICONION_ADDRESS") is {})
{
    builder.Services.AddSingleton<IMagicOnionURLProvider, EnvironmentVariableURLProvider>();
}
else
{
    builder.Services.AddSingleton<IMagicOnionURLProvider, MagicOnionURLProvider>();
}
builder.Services.AddSingleton<IMagicOnionURLResolver, RandomURLResolver>();

// MessagePack設定 https://spacekey.info/posts/2167/ https://www.misuzilla.org/Blog/page/19/

builder.Services.AddMvc()
       .AddMvcOptions(option =>
       {
           option.OutputFormatters.Clear();
           option.InputFormatters.Clear();
           option.OutputFormatters.Add(new MessagePackOutputFormatter(ContractlessStandardResolver.Options));
           option.InputFormatters.Add(new MessagePackInputFormatter(ContractlessStandardResolver.Options));
       });

builder.WebHost.UseKestrel(options =>
{
    if(Environment.GetEnvironmentVariable("PORT") is {} port && int.TryParse(port, out var result))
    {
        options.ListenAnyIP(result);
    }
});

var app = builder.Build();
// ミドルウェアのパイプライン設定
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // HTTPSリダイレクト
app.UseStaticFiles();      // 静的ファイルのサポート

app.UseRouting();          // ルーティングを有効化

// エンドポイントの定義
app.UseEndpoints(endpoints =>
{
    endpoints?.MapControllers(); // 属性ルーティングを有効化
});

app.Run();
