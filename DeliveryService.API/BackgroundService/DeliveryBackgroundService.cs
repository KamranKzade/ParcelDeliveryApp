﻿using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using SharedLibrary.Models;
using RabbitMQ.Client.Events;
using DeliveryServer.API.Models;
using SharedLibrary.ResourceFiles;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using SharedLibrary.Services.RabbitMqCustom;

namespace DeliveryServer.API.BackgroundService;

public class DeliveryBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
{
	private IModel _channel;
	private readonly IServiceProvider _serviceProvider;
	private readonly RabbitMQClientService _rabbitMqClientService;
	private readonly ILogger<DeliveryBackgroundService> _logger;

	public DeliveryBackgroundService(IModel channel, IServiceProvider serviceProvider, RabbitMQClientService rabbitMqClientService, ILogger<DeliveryBackgroundService> logger)
	{
		_channel = channel;
		_serviceProvider = serviceProvider;
		_rabbitMqClientService = rabbitMqClientService;
		_logger = logger;
	}

	public override Task StartAsync(CancellationToken cancellationToken)
	{
		_channel = _rabbitMqClientService.Connect(OrderDirect.ExchangeName, OrderDirect.QueueName, OrderDirect.RoutingWaterMark);

		_channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

		_logger.LogInformation("DeliveryBackgroundService started.");
		return base.StartAsync(cancellationToken);
	}
	
	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var consumer = new AsyncEventingBasicConsumer(_channel);

		_channel.BasicConsume(OrderDirect.QueueName, false, consumer);

		consumer.Received += Consumer_Received; ;

		_logger.LogInformation("Message consumption has started.");
		return Task.CompletedTask;
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Stopping DeliveryOrderBackgroundService...");
		return base.StopAsync(cancellationToken);
	}

	private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
	{
		try
		{
			var orderDelivery = JsonSerializer.Deserialize<OrderDelivery>(Encoding.UTF8.GetString(@event.Body.ToArray()));
			

			using var scope = _serviceProvider.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			var genericRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<AppDbContext, OrderDelivery>>();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			// var order = dbContext.OrderDeliveries.FirstOrDefault(o => o.Id == orderDelivery.Id);

			genericRepo.UpdateAsync(orderDelivery);
			unitOfWork.Commit();

			_logger.LogInformation($"Order added successfully. OrderId: {orderDelivery.Id}");
			_channel.BasicAck(@event.DeliveryTag, false);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while processing the order update: {ex.Message}");
			_logger.LogError(ex.Message);
		}

		return Task.CompletedTask;
	}

}
