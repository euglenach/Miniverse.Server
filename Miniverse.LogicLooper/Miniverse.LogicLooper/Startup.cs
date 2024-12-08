// // github.com/Cysharp/LogicLooper/blob/master/samples/LoopHostingApp/Startup.cs
//
// using Cysharp.Threading;
//
// namespace Miniverse.LogicLooperServer;
//
// public class Startup(IConfiguration configuration)
// {
//     public readonly IConfiguration Configuration = configuration;
//     
//     // This method gets called by the runtime. Use this method to add services to the container.
//     public void ConfigureServices(IServiceCollection services)
//     {
//         // Register a LooperPool to the service container.
//         services.AddSingleton<ILogicLooperPool>(_ => new LogicLooperPool(60, Environment.ProcessorCount, RoundRobinLogicLooperPoolBalancer.Instance));
//         services.AddHostedService<LoopHostedService>();
//
//         services.AddRazorPages();
//     }
//     
//     // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//     public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//     {
//         if (env.IsDevelopment())
//         {
//             app.UseDeveloperExceptionPage();
//         }
//         else
//         {
//             app.UseExceptionHandler("/Error");
//             // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//             app.UseHsts();
//         }
//
//         app.UseHttpsRedirection();
//         app.UseStaticFiles();
//
//         app.UseRouting();
//
//         app.UseAuthorization();
//
//         app.UseEndpoints(endpoints =>
//         {
//             endpoints.MapRazorPages();
//         });
//     }
// }
