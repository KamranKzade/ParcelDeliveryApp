using SharedLibrary.Dtos;
using OrderService.API.Mapper;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OrderService.API.UnitOfWork.Abstract;
using OrderService.API.Services.Abstract;
using OrderService.API.Repositories.Abstract;

namespace OrderService.API.Services.Concrete;

public class ServiceGeneric<TEntity, TDto> : IServiceGeneric<TEntity, TDto> where TDto : class where TEntity : class
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IGenericRepository<TEntity> _genericRepo;

	public ServiceGeneric(IUnitOfWork unitOfWork, IGenericRepository<TEntity> genericRepo)
	{
		_unitOfWork = unitOfWork;
		_genericRepo = genericRepo;
	}

	public async Task<Response<TDto>> AddAsync(TDto entity)
	{
		var newEntity = ObjectMapper.Mapper.Map<TEntity>(entity);

		await _genericRepo.AddAsync(newEntity);
		await _unitOfWork.CommitAsync();

		var newDto = ObjectMapper.Mapper.Map<TDto>(newEntity);
		return Response<TDto>.Success(newDto, StatusCodes.Status200OK);
	}

	public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
	{
		var product = ObjectMapper.Mapper.Map<List<TDto>>(await _genericRepo.GetAllAsync());
		return Response<IEnumerable<TDto>>.Success(product, StatusCodes.Status200OK);
	}

	public async Task<Response<TDto>> GetByIdAsync(int id)
	{
		var product = await _genericRepo.GetByIdAsync(id);

		if (product == null)
		{
			return Response<TDto>.Fail("Id not fount", StatusCodes.Status404NotFound, true);
		}

		return Response<TDto>.Success(ObjectMapper.Mapper.Map<TDto>(product), StatusCodes.Status200OK);
	}

	public async Task<Response<NoDataDto>> RemoveAsync(int id)
	{
		var isExistEntity = await _genericRepo.GetByIdAsync(id);

		if (isExistEntity == null)
		{
			return Response<NoDataDto>.Fail("Id not found", StatusCodes.Status404NotFound, true);
		}

		_genericRepo.Remove(isExistEntity);

		await _unitOfWork.CommitAsync();


		// 204 durum kodu => No Content => Response body'sinde hec 1 data olmayacaq
		return Response<NoDataDto>.Success(StatusCodes.Status204NoContent);
	}

	public async Task<Response<NoDataDto>> UpdateAsync(TDto entity, int id)
	{
		var isExistEntity = await _genericRepo.GetByIdAsync(id);

		if (isExistEntity == null)
		{
			return Response<NoDataDto>.Fail("Id not found", StatusCodes.Status404NotFound, true);
		}

		var updateEntity = ObjectMapper.Mapper.Map<TEntity>(entity);

		_genericRepo.UpdateAsync(updateEntity);

		await _unitOfWork.CommitAsync();

		// 204 durum kodu => No Content => Response body'sinde hec 1 data olmayacaq
		return Response<NoDataDto>.Success(StatusCodes.Status204NoContent);
	}

	public async Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
	{
		var list = _genericRepo.Where(predicate);

		return Response<IEnumerable<TDto>>.Success(ObjectMapper.Mapper.Map<IEnumerable<TDto>>(await list.ToListAsync()), StatusCodes.Status200OK);

	}
}
