using LoadDWVentas.Data.Context;
using LoadDWVentas.Data.Interfaces;
using LoadDWVentas.Data.Services;
using LoadDWVentas.WorkerService;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
       CreateHostBuilder(args).Build().Run();
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) => {

            services.AddDbContextPool<NorwindContext>(options => 
                                                      options.UseSqlServer(hostContext.Configuration.GetConnectionString("DbNorwindContext")));

            services.AddDbContextPool<DbSalesContext>(options => 
                                                      options.UseSqlServer(hostContext.Configuration.GetConnectionString("DbSalesContext")));
 

            services.AddScoped<IDataServiceDwVentas, DataServiceDwVentas>();

            services.AddHostedService<Worker>();
        });
}