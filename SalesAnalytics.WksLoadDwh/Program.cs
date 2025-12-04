using Microsoft.EntityFrameworkCore;
using SalesAnalytics.Application.Interfaces;
using SalesAnalytics.Application.Repositories;
using SalesAnalytics.Application.Services;
using SalesAnalytics.Domain.Repository;
using SalesAnalytics.Persistence.Repositories; // Aseg?rate de tener este using
using SalesAnalytics.Persistence.Repositories.Db;
using SalesAnalytics.Persistence.Repositories.Db.Context;
using SalesAnalytics.Persistence.Repositories.Dwh;
using SalesAnalytics.Persistence.Repositories.Dwh.Context; // Nuevo using
using SalesAnalytics.WksLoadDwh;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;

        services.AddDbContext<SalesContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SavDbConnection")));

        services.AddDbContext<SalesDwhContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DwhConnection")));


        services.AddTransient(typeof(IFileReaderRepository<>), typeof(CsvSalesFileReaderRepository<>));
        services.AddTransient<ISaleRepository, SaleRepository>();

 
        services.AddTransient<IDwhRepository, DwhRepository>();

        services.AddTransient<ISalesDataHandlerService, SalesDataHandlerService>();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();