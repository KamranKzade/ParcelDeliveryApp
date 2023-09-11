namespace SharedLibrary.UnitOfWork.Abstract;

public interface IUnitOfWork
{
    Task CommitAsync();
    void Commit();
}
