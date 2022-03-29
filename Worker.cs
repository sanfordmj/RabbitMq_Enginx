using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System;
using NLog;
using System.Security.Authentication;

namespace RabbitMQWorker
{
    public class Worker : BackgroundService
    {
        private IModel? channel = null;
        private IConnection? connection = null;
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        public Worker(IConfiguration configuration, ILogger<Worker> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }




        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        { 
            _logger.LogInformation("ExecuteAsync: {time}", DateTimeOffset.Now);
            await this.Run();           
        }

        public override async Task StopAsync(CancellationToken stoppingToken){
            _logger.LogInformation("StopAsync: {time}", DateTimeOffset.Now);
            if (channel != null)
            channel.Dispose();
            if(connection != null)
            connection.Dispose();

            await Task.Delay(50);
        }

        protected async Task Run()
        {
            try{

                var factory = new ConnectionFactory()
                    {
                        HostName = _configuration["RabbitMQServers:HostName"],                        
                        VirtualHost = _configuration["RabbitMQServers:VirtualHost"],
                        UserName = _configuration["RabbitMQServers:UserName"],
                        Password = _configuration["RabbitMQServers:Password"],
                        Port = Convert.ToInt32(_configuration["RabbitMQServers:Port"]),
                        AmqpUriSslProtocols = (SslProtocols)Enum.Parse(typeof(SslProtocols), _configuration["RabbitMQServers:AmqpUriSslProtocols"]),
                };
                
                connection = factory.CreateConnection();
                channel = this.connection.CreateModel();

                _logger.LogInformation("CreateConnection: {time}", DateTimeOffset.Now);

                var consumer = new EventingBasicConsumer(this.channel);
                consumer.Received += OnMessageRecieved;
            
                channel.BasicConsume(queue: "error", autoAck: true, consumer: consumer);
            }
            catch(Exception ex){
                     _logger.LogInformation("Error received : {0}", ex.Message);
            }

            await Task.Delay(50);
        } 

        private void OnMessageRecieved(object? model, BasicDeliverEventArgs args)
        {
            var body = args.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());
            _logger.LogInformation("Worker received : {0}", message);

            Logger logger = LogManager.GetCurrentClassLogger();

            logger.Error(message);

        }


    }
}