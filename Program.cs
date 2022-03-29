using RabbitMQWorker;
using dws.models.Config;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();        
    }).Build();




await host.RunAsync();
