using SharedLibrary.Models;
using OrderServer.API.Models;
using SharedLibrary.ResourceFiles;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using SharedLibrary.Services.RabbitMqCustom;

namespace OrderServer.API.BackgroundServices;

public class OutBoxBackground : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly RabbitMQPublisher<OutBox> _rabbitMQPublisher;
	private readonly ILogger<OutBoxBackground> _logger;

	public OutBoxBackground(IServiceProvider serviceProvider, RabbitMQPublisher<OutBox> rabbitMQPublisher, ILogger<OutBoxBackground> logger)
	{
		_serviceProvider = serviceProvider;
		_rabbitMQPublisher = rabbitMQPublisher;
		_logger = logger;
	}

	public override Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Starting OutBoxBackground...");
		return base.StartAsync(cancellationToken);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			using var scope = _serviceProvider.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			var genericRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<AppDbContext, OutBox>>();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var OutboxOrders = dbContext.OutBoxes.Where(x => x.IsSend == false);

			foreach (var order in OutboxOrders)
			{
				if (order.IsDelete)
				{
					_rabbitMQPublisher.Publish(order, OutBoxDirect.ExchangeName, OutBoxDirect.QueueName, OutBoxDirect.RoutingWaterMark);
					_logger.LogInformation($"OutBox sent to RabbitMQ --> {order.Name}");

					genericRepo.Remove(order);
					_logger.LogInformation($"Outbox order successfully removed --> {order.Name}");
				}
				else
				{
					order.IsSend = true;
					genericRepo.UpdateAsync(order);
					_logger.LogInformation($"Outbox order successfully updated --> {order.Name}");
				}

				await unitOfWork.CommitAsync();

				_rabbitMQPublisher.Publish(order, OutBoxDirect.ExchangeName, OutBoxDirect.QueueName, OutBoxDirect.RoutingWaterMark);
				_logger.LogInformation($"OutBox sent to RabbitMQ --> {order.Name}");
			}

			await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
		}
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Stopping OutBoxBackground...");
		return base.StopAsync(cancellationToken);
	}
}
