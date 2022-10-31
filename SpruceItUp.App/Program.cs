using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.Modal;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
namespace SpruceItUp.App
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri("http://localhost/api/")});
            builder.Services.AddBlazoredModal();
            await builder.Build().RunAsync();
        }
    }
}