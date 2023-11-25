using System.Net.Http;
using System.Windows;
using ElvUIDownloader.DI;
using ElvUIDownloader.Models;
using ElvUIDownloader.Services;
using ElvUIDownloader.Utils;
using ElvUIDownloader.ViewModels;
using ElvUIDownloader.Views;
using ElvUIDownloader.Views.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ElvUIDownloader
{
    public class SimpleIoC
    {
        private static readonly SimpleIoC instance = new SimpleIoC();
 
        public ServiceProvider Provider { get; private set; }
        
        public ServiceCollection Services { get; private set; }
 
        private SimpleIoC()
        {
            Services = new ServiceCollection();
        }
 
        public static SimpleIoC GetInstance()
        {
            return instance;
        }

        public void Build()
        {
            Provider = Services.BuildServiceProvider();
        }
    }
    
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IConfiguration Configuration { get; private set; }
 

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder();
            
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name!);
            
            if(File.Exists(Path.Combine(dir, "appsettings.json")))
            {
                builder
                    .SetBasePath(dir)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            }
            else
            {
                builder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            }



            Configuration = builder.Build();
            ConfigureServices(SimpleIoC.GetInstance().Services);
 
            SimpleIoC.GetInstance().Build();

            var provider = SimpleIoC.GetInstance().Provider;
            
            var appSettings = provider.GetRequiredService<AppSettings>();
            Configuration.Bind(appSettings);

            await appSettings.LoadProfilesAsync();

            var updateService = provider.GetRequiredService<UpdateService>();
            await updateService.RunAsync();
            
            var dialogService = provider.GetRequiredService<DialogService>();

            if (appSettings.StartMinimize)
            {
                dialogService.ShowHidden<MainViewModel>();
            }
            else
            {
                dialogService.Show<MainViewModel>();
            }
        }
        
        private void ConfigureServices(IServiceCollection services)
        {
            // ...

            services.AddSingleton<AppSettings>();
            services.AddSingleton<DialogService>();
            services.AddSingleton<RegistryService>();
            services.AddTransient<UpdateService>();
            
            var viewModels = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(ViewModelBase));

            foreach (var viewModel in viewModels)
            {
                services.AddTransient(viewModel);
            }
            
            var views = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(BaseView));
            
            foreach (var view in views)
            {
                services.AddKeyedTransient(view, view.Name);
            }
        }
    }
}