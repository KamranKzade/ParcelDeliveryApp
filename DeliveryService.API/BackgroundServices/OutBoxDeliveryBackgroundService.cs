using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using SharedLibrary.Models;
using RabbitMQ.Client.Events;
using DeliveryServer.API.Models;
using SharedLibrary.ResourceFiles;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using SharedLibrary.Services.RabbitMqCustom;

namespace DeliveryServer.API.BackgroundServices;

public class OutBoxDeliveryBackgroundService : BackgroundService
{
	private IModel _channel;
	private readonly IServiceProvider _serviceProvider;
	private readonly RabbitMQClientService _rabbitMqClientService;
	private readonly ILogger<OutBoxDeliveryBackgroundService> _logger;

	public OutBoxDeliveryBackgroundService(IServiceProvider serviceProvider, RabbitMQClientService rabbitMqClientService, ILogger<OutBoxDeliveryBackgroundService> logger)
	{
		_serviceProvider = serviceProvider;
		_rabbitMqClientService = rabbitMqClientService;
		_logger = logger;
	}


	public override Task StartAsync(CancellationToken cancellationToken)
	{
		_channel = _rabbitMqClientService.Connect(OutBoxDirect.ExchangeName, OutBoxDirect.QueueName, OutBoxDirect.RoutingWaterMark);

		_channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

		_logger.LogInformation("OutBoxDeliveryBackgroundService started.");
		return base.StartAsync(cancellationToken);
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Stopping OutBoxDeliveryBackgroundService...");
		return base.StopAsync(cancellationToken);
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var consumer = new AsyncEventingBasicConsumer(_channel);

		_channel.BasicConsume(OutBoxDirect.QueueName, false, consumer);

		consumer.Received += Consumer_Received; ; ;

		_logger.LogInformation("Message consumption has started.");
		return Task.CompletedTask;
	}

	private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
	{
		try
		{
			var outboxInOrderService = JsonSerializer.Deserialize<OutBox>(Encoding.UTF8.GetString(@event.Body.ToArray()));

			using var scope = _serviceProvider.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			var genericRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<AppDbContext, OrderDelivery>>();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var orderInDeliveryService = dbContext.OrderDeliveries.FirstOrDefault(x => x.Id == outboxInOrderService!.Id);

			var order = new OrderDelivery
			{
				Id = outboxInOrderService!.Id,
				CourierId = outboxInOrderService.CourierId,
				CourierName = outboxInOrderService.CourierName,
				CreatedDate = outboxInOrderService.CreatedDate,
				DestinationAddress = outboxInOrderService.DestinationAddress,
				DeliveryDate = outboxInOrderService.DeliveryDate,
				Name = outboxInOrderService.Name,
				Status = outboxInOrderService.Status,
				TotalAmount = outboxInOrderService.TotalAmount,
				UserId = outboxInOrderService.UserId,
				UserName = outboxInOrderService.UserName,
			};

			if (orderInDeliveryService is null)
			{
				await genericRepo.AddAsync(order!);
				_logger.LogInformation($"Order added successfully. OrderId: {outboxInOrderService!.Id}");
			}
			else
			{
				if (outboxInOrderService.IsDelete)
				{
					genericRepo.Remove(order);
					_logger.LogInformation($"Order remove successfully. OrderId: {outboxInOrderService!.Id}");
				}
				genericRepo.UpdateAsync(order);
				_logger.LogInformation($"Order update successfully. OrderId: {outboxInOrderService!.Id}");
			}

			await unitOfWork.CommitAsync();
			_channel.BasicAck(@event.DeliveryTag, false);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while processing the order update: {ex.Message}");
			_logger.LogError(ex.Message);
		}
	}

}
