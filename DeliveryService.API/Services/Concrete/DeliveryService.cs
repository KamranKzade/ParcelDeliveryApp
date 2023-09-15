using Serilog;
using Newtonsoft.Json;
using SharedLibrary.Dtos;
using SharedLibrary.Models;
using DeliveryServer.API.Dtos;
using SharedLibrary.Models.Enum;
using DeliveryServer.API.Models;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using DeliveryServer.API.Services.Abstract;
using SharedLibrary.Services.RabbitMqCustom;

namespace DeliveryServer.API.Services.Concrete;

public class DeliveryService : IDeliveryService
{
	private readonly AppDbContext _dbContext;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IConfiguration _configuration;
	private readonly ILogger<DeliveryService> _logger;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly RabbitMQPublisher<OrderDelivery> _rabbitMQPublisher;
	private readonly IGenericRepository<OrderDelivery> _genericRepository;

	public DeliveryService(AppDbContext dbContext, IUnitOfWork unitOfWork, IGenericRepository<OrderDelivery> genericRepository, IConfiguration configuration, IHttpClientFactory httpClientFactory, RabbitMQPublisher<OrderDelivery> rabbitMQPublisher, ILogger<DeliveryService> logger)
	{
		_dbContext = dbContext;
		_unitOfWork = unitOfWork;
		_genericRepository = genericRepository;
		_configuration = configuration;
		_httpClientFactory = httpClientFactory;
		_rabbitMQPublisher = rabbitMQPublisher;
		_logger = logger;
	}

	public async Task<Response<NoDataDto>> ChangeOrderStatus(ChangeOrderStatusDto dto, string courierId)
	{
		try
		{
			var orders = await GetOrders();

			if (orders.Count == 0)
			{
				_logger.LogWarning("Database-da order yoxdur");
				return Response<NoDataDto>.Fail("Database-da order yoxdur", StatusCodes.Status404NotFound, true);
			}

			foreach (var order in orders)
			{
				if (order.Id.ToString().ToLower() == dto.OrderId.ToLower() && order.CourierId == courierId)
				{
					var isCheck = await _dbContext.OrderDeliveries.FirstOrDefaultAsync(d => d.Id == order.Id);

					if (order.Status == OrderStatus.Delivered)
					{
						_logger.LogWarning("Bu Order Artiq catdirilib ve siz bunu deyise bilmezsiniz");
						return Response<NoDataDto>.Fail("Bu Order Artiq catdirilib ve siz bunu deyise bilmezsiniz", StatusCodes.Status404NotFound, true);
					}

					if (isCheck != null)
					{
						isCheck.Status = dto.OrderStatus;
						isCheck.DeliveryDate = DateTime.Now;
						_genericRepository.UpdateAsync(isCheck);
						await _unitOfWork.CommitAsync();

						// RabbitMq ile elaqe
						_rabbitMQPublisher.Publish(isCheck);
						_logger.LogInformation("Order statusu deyisdirildi.");
						return Response<NoDataDto>.Success(StatusCodes.Status200OK);
					}
					else
					{
						order.Status = dto.OrderStatus;
						order.DeliveryDate = DateTime.Now;
						await _genericRepository.AddAsync(order);
						await _unitOfWork.CommitAsync();

						// RabbitMq ile elaqe
						_rabbitMQPublisher.Publish(order);
						return Response<NoDataDto>.Success(StatusCodes.Status200OK);
					}
				}
			}

			_logger.LogWarning("Order tapilmadi");
			return Response<NoDataDto>.Fail("Order tapilmadi", StatusCodes.Status404NotFound, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Order statusunu deyisdirken xeta bas verdi.");
			throw;
		}
	}


	public async Task<List<OrderDelivery>> GetOrders()
	{
		try
		{
			string orderServiceBaseUrl = _configuration["OrderServiceBaseUrl"];
			string getOrderEndpoint = $"{orderServiceBaseUrl}/api/Order/GetOrder";

			// IHttpClientFactory'den HttpClient örneği alın
			var client = _httpClientFactory.CreateClient();

			// Identity Service ile iletişim kuracak HttpClient oluşturun
			using (client)
			{
				// Identity Service'den kullanıcı bilgilerini alın
				var response = await client.GetAsync(getOrderEndpoint);
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
						return null;
					}
				}
				else
				{
					_logger.LogWarning($"HTTP isteği sırasında bir hata oluştu. StatusCode: {response.StatusCode}");
					return null;
				}
			}
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "HTTP isteği sırasında bir hata oluştu: {Message}", ex.Message);
			return null;
		}
	}

}
