namespace KogaseEngine.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
} 