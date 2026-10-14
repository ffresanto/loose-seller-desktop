using LooseSeller.Domain.Entities;
using LooseSeller.Domain.Interfaces;

namespace LooseSeller.Application.Services;

public sealed class ProductPreparationService(IProductFileStorage fileStorage)
{
    public Task<string> PrepareAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        return fileStorage.PrepareAsync(product, cancellationToken);
    }
}
