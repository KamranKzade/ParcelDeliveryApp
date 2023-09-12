using SharedLibrary.Dtos;
using DeliveryServer.API.Dtos;
using DeliveryServer.API.Models;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using DeliveryServer.API.Services.Abstract;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace DeliveryServer.API.Services.Concrete;

public class DeliveryService : IDeliveryService
{
	private readonly AppDbContext _dbContext;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IGenericRepository<OrderDelivery> _genericRepository;
	private readonly IServiceGeneric<OrderDelivery, ChangeOrderStatusDto> _serviceGeneric;
	private readonly IConfiguration _configuration;
	private readonly IHttpClientFactory _httpClientFactory;

	public DeliveryService(AppDbContext dbContext, IUnitOfWork unitOfWork, IGenericRepository<OrderDelivery> genericRepository, IServiceGeneric<OrderDelivery, ChangeOrderStatusDto> serviceGeneric, IConfiguration configuration, IHttpClientFactory httpClientFactory)
	{
		_dbContext = dbContext;
		_unitOfWork = unitOfWork;
		_genericRepository = genericRepository;
		_serviceGeneric = serviceGeneric;
		_configuration = configuration;
		_httpClientFactory = httpClientFactory;
	}

	public async Task<Response<NoDataDto>> ChangeOrderStatus(ChangeOrderStatusDto dto, string courierId)
	{
		var orders = await GetOrders();

		if (orders.Count == 0)
			return Response<NoDataDto>.Fail("Database-da order yoxdur", StatusCodes.Status404NotFound, true);

		foreach (var order in orders)
		{
			if (order.Id.ToString().ToLower() == dto.OrderId.ToLower() && order.CourierId == courierId)
			{
				var isCheck = await _dbContext.OrderDeliveries.FirstOrDefaultAsync(d => d.CourierId == order.CourierId);

				if (isCheck != null)
				{
					isCheck.Status = dto.OrderStatus;
					isCheck.DeliveryDate = DateTime.Now;
					_genericRepository.UpdateAsync(isCheck);
					await _unitOfWork.CommitAsync();
					return Response<NoDataDto>.Success(StatusCodes.Status200OK);
				}
				else
				{
					order.Status = dto.OrderStatus;
					order.DeliveryDate = DateTime.Now;
					await _genericRepository.AddAsync(order);
					await _unitOfWork.CommitAsync();
					return Response<NoDataDto>.Success(StatusCodes.Status200OK);
				}


				// RabbitMq

			}
		}

		return Response<NoDataDto>.Fail("Order tapilmadi", StatusCodes.Status404NotFound, true);
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
						return orders.Data.ToList();
					else
						return null;
				}
				else
					return null;
			}
		}
		catch (HttpRequestException ex)
		{
			Console.WriteLine($"HTTP isteği sırasında hata oluştu: {ex.Message}");
			return null;
		}
	}

}
