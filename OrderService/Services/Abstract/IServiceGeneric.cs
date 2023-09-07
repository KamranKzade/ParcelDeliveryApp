using SharedLibrary.Dtos;
using System.Linq.Expressions;

namespace OrderService.Services.Abstract;

public interface IServiceGeneric<TEntity, TDto> where TEntity : class where TDto : class
{

	Task<Response<TDto>> GetByIdAsync(int id);
	Task<Response<IEnumerable<TDto>>> GetAllAsync();
	Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate);
	Task<Response<TDto>> AddAsync(TDto entity);
	Task<Response<NoDataDto>> UpdateAsync(TDto entity, int id);
	Task<Response<NoDataDto>> RemoveAsync(int id);
}
