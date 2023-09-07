namespace OrderService.API.UnitOfWork.Abstract;

public interface IUnitOfWork
{
	Task CommitAsync();
	void Commit();
}
