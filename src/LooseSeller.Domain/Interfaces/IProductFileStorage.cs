using LooseSeller.Domain.Entities;

namespace LooseSeller.Domain.Interfaces;

public interface IProductFileStorage
{
    Task<string> PrepareAsync(Product product, CancellationToken cancellationToken = default);
}
