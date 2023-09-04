namespace AuthServer.API.UnitOfWork.Abstract;

public interface IUnitOfWork
{
	Task CommitAsync();
	void Commit();
}