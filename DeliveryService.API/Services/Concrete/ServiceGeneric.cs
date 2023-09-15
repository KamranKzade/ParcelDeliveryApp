using SharedLibrary.Dtos;
using System.Linq.Expressions;
using DeliveryServer.API.Mapper;
using DeliveryServer.API.Models;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;

namespace DeliveryServer.API.Services.Concrete;

public class ServiceGeneric<TEntity, TDto> : IServiceGeneric<TEntity, TDto> where TDto : class where TEntity : class
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IGenericRepository<AppDbContext, TEntity> _genericRepo;
	private readonly ILogger<ServiceGeneric<TEntity, TDto>> _logger;

	public ServiceGeneric(IUnitOfWork unitOfWork, IGenericRepository<AppDbContext, TEntity> genericRepo, ILogger<ServiceGeneric<TEntity, TDto>> logger)
	{
		_unitOfWork = unitOfWork;
		_genericRepo = genericRepo;
		_logger = logger;
	}

	public async Task<Response<TDto>> AddAsync(TDto entity)
	{

		try
		{
			var newEntity = ObjectMapper.Mapper.Map<TEntity>(entity);

			await _genericRepo.AddAsync(newEntity);
			await _unitOfWork.CommitAsync();

			var newDto = ObjectMapper.Mapper.Map<TDto>(newEntity);

			_logger.LogInformation($"Entity added successfully: {entity}");
			return Response<TDto>.Success(newDto, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while adding the entity: {Entity}");
			var error = new ErrorDto("An error occurred while adding the entity", true);
			// You can customize the error message and status code as needed.
			return Response<TDto>.Fail(error, StatusCodes.Status500InternalServerError);
		}
	}

	public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
	{
		try
		{
			var product = ObjectMapper.Mapper.Map<List<TDto>>(await _genericRepo.GetAllAsync());
			_logger.LogInformation("All entities fetched successfully");
			return Response<IEnumerable<TDto>>.Success(product, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while fetching all entities");
			var error = new ErrorDto("Error while fetching all entities", true);
			return Response<IEnumerable<TDto>>.Fail(error, StatusCodes.Status500InternalServerError);
		}
	}

	public async Task<Response<TDto>> GetByIdAsync(string id)
	{
		try
		{
			var product = await _genericRepo.GetByIdAsync(id);

			if (product == null)
			{
				_logger.LogWarning($"Entity with ID {id} not found");
				return Response<TDto>.Fail("Id not found", StatusCodes.Status404NotFound, true);
			}

			_logger.LogInformation($"Entity with ID {id} retrieved successfully"); ;
			return Response<TDto>.Success(ObjectMapper.Mapper.Map<TDto>(product), StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while retrieving entity by ID: {id}");
			var error = new ErrorDto("An error occurred while retrieving the entity", true);

			return Response<TDto>.Fail(error, StatusCodes.Status500InternalServerError);
		}
	}

	public async Task<Response<NoDataDto>> RemoveAsync(string id)
	{
		try
		{
			var isExistEntity = await _genericRepo.GetByIdAsync(id);

			if (isExistEntity == null)
			{
				_logger.LogWarning($"Entity with ID {id} not found for removal");
				return Response<NoDataDto>.Fail("Id not found", StatusCodes.Status404NotFound, true);
			}

			_genericRepo.Remove(isExistEntity);
			await _unitOfWork.CommitAsync();

			_logger.LogInformation($"Entity with ID {id} removed successfully");
			return Response<NoDataDto>.Success(StatusCodes.Status204NoContent);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while removing entity by ID: {id}");
			var error = new ErrorDto("An error occurred while removing the entity", true);

			return Response<NoDataDto>.Fail(error, StatusCodes.Status500InternalServerError);
		}
	}

	public async Task<Response<NoDataDto>> UpdateAsync(TDto entity, string id)
	{
		try
		{
			var isExistEntity = await _genericRepo.GetByIdAsync(id);

			if (isExistEntity == null)
			{
				_logger.LogWarning($"Entity with ID {id} not found for update");
				return Response<NoDataDto>.Fail("Id not found", StatusCodes.Status404NotFound, true);
			}

			var updateEntity = ObjectMapper.Mapper.Map<TEntity>(entity);

			_genericRepo.UpdateAsync(updateEntity);
			await _unitOfWork.CommitAsync();

			_logger.LogInformation($"Entity with ID {id} updated successfully");
			return Response<NoDataDto>.Success(StatusCodes.Status204NoContent);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while updating entity by ID: {id}");
			var error = new ErrorDto("An error occurred while updating the entity", true);

			return Response<NoDataDto>.Fail(error, StatusCodes.Status500InternalServerError);
		}
	}

	public async Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
	{
		try
		{
			var list = _genericRepo.Where(predicate);

			_logger.LogInformation("Querying entities using a predicate");
			return Response<IEnumerable<TDto>>.Success(ObjectMapper.Mapper.Map<IEnumerable<TDto>>(await list.ToListAsync()), StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while querying entities using a predicate");
			var error = new ErrorDto("An error occurred while querying entities", true);

			return Response<IEnumerable<TDto>>.Fail(error, StatusCodes.Status500InternalServerError);
		}
	}

}
