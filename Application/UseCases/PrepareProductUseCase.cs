using LooseSeller.Application.Abstractions;
using LooseSeller.Business.Entities;
namespace LooseSeller.Application.UseCases;
public sealed class PrepareProductUseCase(IProductPreparationService preparationService)
{
    public Task<string> ExecuteAsync(Product product, CancellationToken cancellationToken = default) => preparationService.PrepareAsync(product, cancellationToken);
}
