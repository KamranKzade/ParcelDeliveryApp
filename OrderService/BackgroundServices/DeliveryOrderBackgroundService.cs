using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using SharedLibrary.Models;
using RabbitMQ.Client.Events;
using OrderService.API.Models;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using SharedLibrary.Services.RabbitMqCustom;

namespace OrderService.API.BackgroundServices;

public class DeliveryOrderBackgroundService : BackgroundService
{
	private IModel _channel;
	private readonly IServiceProvider _serviceProvider;
	private readonly RabbitMQClientService _rabbitMqClientService;
	private readonly ILogger<DeliveryOrderBackgroundService> _logger;

	public DeliveryOrderBackgroundService(RabbitMQClientService rabbitMqClientService, ILogger<DeliveryOrderBackgroundService> logger, IServiceProvider dbContext)
	{
		_rabbitMqClientService = rabbitMqClientService;
		_logger = logger;
		_serviceProvider = dbContext;
	}

	public override Task StartAsync(CancellationToken cancellationToken)
	{
		_channel = _rabbitMqClientService.Connect();

		_channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

		return base.StartAsync(cancellationToken);
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var consumer = new AsyncEventingBasicConsumer(_channel);

		_channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);

		consumer.Received += Consumer_Received;

		return Task.CompletedTask;
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		return base.StopAsync(cancellationToken);
	}

	private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
	{
		try
		{
			var orderDelivery = JsonSerializer.Deserialize<OrderDelivery>(Encoding.UTF8.GetString(@event.Body.ToArray()));

			using var scope = _serviceProvider.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			var genericReop = scope.ServiceProvider.GetRequiredService<IGenericRepository<OrderDelivery>>();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderDelivery.Id);

			order.Status = orderDelivery.Status;
			order.DeliveryDate = orderDelivery.DeliveryDate;
			genericReop.UpdateAsync(order);
			unitOfWork.Commit();

			_channel.BasicAck(@event.DeliveryTag, false);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex.Message);
		}

		return Task.CompletedTask;
	}
}
