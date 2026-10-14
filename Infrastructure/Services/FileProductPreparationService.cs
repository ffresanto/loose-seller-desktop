using System.Text;
using LooseSeller.Application.Abstractions;
using LooseSeller.Business.Entities;
namespace LooseSeller.Infrastructure.Services;
public sealed class FileProductPreparationService(string productsRootPath) : IProductPreparationService
{
    public async Task<string> PrepareAsync(Product product, CancellationToken cancellationToken = default)
    {
        var folder = Path.Combine(productsRootPath, Sanitize(product.Title)); Directory.CreateDirectory(folder);
        File.Copy(product.ModelFilePath, Path.Combine(folder, Path.GetFileName(product.ModelFilePath)), true);
        var index = 1;
        foreach (var image in product.ImageFilePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Copy(image, Path.Combine(folder, $"imagem-{index:00}{Path.GetExtension(image)}"), true); index++;
        }
        await File.WriteAllTextAsync(Path.Combine(folder, "descricao.txt"), BuildDescription(product), Encoding.UTF8, cancellationToken);
        return folder;
    }
    private static string BuildDescription(Product p) =>
$"""
{p.Title}

Produto produzido por impressão 3D.

Medidas aproximadas:
- Largura: {p.Width} cm
- Altura: {p.Height} cm
- Profundidade: {p.Depth} cm

Material: {p.Material}
Cor: {p.Color}

As medidas e a tonalidade podem apresentar pequenas variações devido ao processo de impressão 3D.
""";
    private static string Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Select(c => invalid.Contains(c) ? '-' : c).ToArray()).Trim().TrimEnd('.');
    }
}
