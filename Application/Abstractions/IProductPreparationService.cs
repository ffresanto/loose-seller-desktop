using LooseSeller.Business.Entities;
namespace LooseSeller.Application.Abstractions;
public interface IProductPreparationService
{
    Task<string> PrepareAsync(Product product, CancellationToken cancellationToken = default);
}
