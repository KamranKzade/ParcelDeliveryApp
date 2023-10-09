using Newtonsoft.Json;
using SharedLibrary.Dtos;
using SharedLibrary.Models;
using SharedLibrary.Helpers;
using System.Net.Http.Headers;
using DeliveryServer.API.Dtos;
using SharedLibrary.Models.Enum;
using DeliveryServer.API.Models;
using SharedLibrary.ResourceFiles;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using DeliveryServer.API.Services.Abstract;
using SharedLibrary.Services.RabbitMqCustom;

namespace DeliveryServer.API.Services.Concrete;

public class DeliveryService : IDeliveryService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IConfiguration _configuration;
	private readonly ILogger<DeliveryService> _logger;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly RabbitMQPublisher<OrderDelivery> _rabbitMQPublisher;
	private readonly IGenericRepository<AppDbContext, OrderDelivery> _genericRepository;

	public DeliveryService(IUnitOfWork unitOfWork, IGenericRepository<AppDbContext, OrderDelivery> genericRepository, IConfiguration configuration, IHttpClientFactory httpClientFactory, RabbitMQPublisher<OrderDelivery> rabbitMQPublisher, ILogger<DeliveryService> logger)
	{
		_unitOfWork = unitOfWork;
		_genericRepository = genericRepository;
		_configuration = configuration;
		_httpClientFactory = httpClientFactory;
		_rabbitMQPublisher = rabbitMQPublisher;
		_logger = logger;
	}

	public async Task<Response<NoDataDto>> ChangeOrderStatus(ChangeOrderStatusDto dto, string courierId, string authorizationToken)
	{
		try
		{
			// OrderService-de olan datalarimizdi
			var OrderServerDb = await GetOrders(authorizationToken);

			if (OrderServerDb.Count == 0)
			{
				_logger.LogWarning("Database-da order yoxdur");
				return Response<NoDataDto>.Fail("Database-da order yoxdur", StatusCodes.Status404NotFound, true);
			}

			foreach (var order in OrderServerDb)
			{
				if (order.Id.ToString().ToLower() == dto.OrderId!.ToLower() && order.CourierId == courierId)
				{
					// DeliveryService-e yazilib ona baxiriq
					var isLocalDb = await _genericRepository.Where(d => d.Id == order.Id).SingleOrDefaultAsync();

					if (order.Status == OrderStatus.Delivered)
					{
						_logger.LogWarning("Bu Order Artiq catdirilib ve siz bunu deyise bilmezsiniz");
						return Response<NoDataDto>.Fail("Bu Order Artiq catdirilib ve siz bunu deyise bilmezsiniz", StatusCodes.Status404NotFound, true);
					}

					if (order.Status >= dto.OrderStatus)
					{
						_logger.LogWarning("Bu Order Artiq yola cixib ve siz bunu initial vezyete bilmezsiniz");
						return Response<NoDataDto>.Fail("Bu Order Artiq yola cixib ve siz bunu initial vezyete bilmezsiniz", StatusCodes.Status404NotFound, true);
					}

					if (isLocalDb != null)
					{
						isLocalDb.Status = dto.OrderStatus;
						if (dto.OrderStatus == OrderStatus.Delivered)
						{
							isLocalDb.DeliveryDate = DateTime.Now;
						}
						_genericRepository.UpdateAsync(isLocalDb);
						await _unitOfWork.CommitAsync();

						// RabbitMq ile elaqe
						_rabbitMQPublisher.Publish(isLocalDb, DeliveryDirect.ExchangeName, DeliveryDirect.QueueName, DeliveryDirect.RoutingWaterMark);
						_logger.LogInformation("Order statusu deyisdirildi.");
						return Response<NoDataDto>.Success(StatusCodes.Status200OK);
					}
					else
					{
						_logger.LogInformation("Delivery Serverde bu order yoxdur.");
						return Response<NoDataDto>.Fail("Delivery Serverde bu order yoxdur.", StatusCodes.Status404NotFound, true);
					}
				}
			}

			_logger.LogWarning("OrderServerDb-da bu order tapilmadi");
			return Response<NoDataDto>.Fail("Order tapilmadi", StatusCodes.Status404NotFound, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Order statusunu deyisdirken xeta bas verdi.");
			throw;
		}
	}


	public async Task<Response<IEnumerable<OrderDelivery>>> GetDeliveryOrder()
	{
		var deliveyOrder = await _genericRepository.GetAllAsync();

		if (deliveyOrder.Count() == 0)
		{
			_logger.LogWarning("No orders found in the database.");
			return Response<IEnumerable<OrderDelivery>>.Fail("There are no orders in the database", StatusCodes.Status404NotFound, true);
		}

		var orderDtos = deliveyOrder.Select(o => new OrderDelivery
		{
			Id = o.Id,
			Name = o.Name,

			Status = o.Status,
			CreatedDate = o.CreatedDate,
			DestinationAddress = o.DestinationAddress,
			TotalAmount = o.TotalAmount,
			UserId = o.UserId,
			UserName = o.UserName,
			CourierId = o.CourierId,
			CourierName = o.CourierName
		}).AsQueryable();

		_logger.LogInformation("Successfully retrieved orders from the database.");
		return Response<IEnumerable<OrderDelivery>>.Success(orderDtos, StatusCodes.Status200OK);
	}

	public async Task<List<OrderDelivery>> GetOrders(string authorizationToken)
	{
		var policy = RetryPolicyHelper.GetRetryPolicy();

		try
		{
			string orderServiceBaseUrl = _configuration["MicroServices:OrderServiceBaseUrl"];
			string getOrderEndpoint = $"{orderServiceBaseUrl}/api/Order/GetOrder";

			// IHttpClientFactory'den HttpClient örneği alın
			var client = _httpClientFactory.CreateClient();

			// Identity Service ile iletişim kuracak HttpClient oluşturun
			using (client)
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken);

				// Identity Service'den kullanıcı bilgilerini alın
				var response = await policy.ExecuteAsync(async () =>
				{
					// Identity Service'den kullanıcı bilgilerini alın
					return await client.GetAsync(getOrderEndpoint);
				});

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var orders = JsonConvert.DeserializeObject<Response<IEnumerable<OrderDelivery>>>(responseContent);
					if (orders != null)
					{
						_logger.LogInformation("Order bilgileri başarıyla alındı.");
						return orders.Data.ToList();
					}
					else
					{
						_logger.LogWarning("Hiçbir sipariş bulunamadı.");
						return null!;
					}
				}
				else
				{
					_logger.LogWarning($"HTTP isteği sırasında bir hata oluştu. StatusCode: {response.StatusCode}");
					return null!;
				}
			}
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "HTTP isteği sırasında bir hata oluştu: {Message}", ex.Message);
			return null!;
		}
	}

}
