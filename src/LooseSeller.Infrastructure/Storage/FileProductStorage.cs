using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using LooseSeller.Domain.Entities;
using LooseSeller.Domain.Interfaces;

namespace LooseSeller.Infrastructure.Storage;

public sealed class FileProductStorage(string productsRootPath, string descriptionTemplatePath) : IProductFileStorage
{
    public async Task<string> PrepareAsync(Product product, CancellationToken cancellationToken = default)
    {
        var folderName = Sanitize($"{product.Reference} - {product.Title}");
        var folder = Path.Combine(productsRootPath, folderName);
        Directory.CreateDirectory(folder);

        File.Copy(product.ModelFilePath, Path.Combine(folder, Path.GetFileName(product.ModelFilePath)), true);
        var index = 1;
        foreach (var image in product.ImageFilePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CompressImage(image, Path.Combine(folder, $"imagem-{index:00}.jpg"));
            index++;
        }

        var description = await BuildDescriptionAsync(product, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(folder, "descricao.txt"), description, Encoding.UTF8, cancellationToken);
        return folder;
    }

    private static void CompressImage(string sourcePath, string destinationPath)
    {
        using var source = Image.FromFile(sourcePath);
        using var bitmap = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.White);
            graphics.DrawImage(source, 0, 0, source.Width, source.Height);
        }

        var encoder = ImageCodecInfo.GetImageEncoders().First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
        using var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 92L);
        bitmap.Save(destinationPath, encoder, parameters);
    }

    private async Task<string> BuildDescriptionAsync(Product product, CancellationToken cancellationToken)
    {
        var template = await File.ReadAllTextAsync(descriptionTemplatePath, cancellationToken);
        return template
            .Replace("[TÍTULO DO PRODUTO]", product.Title)
            .Replace("[Material]", product.Material)
            .Replace("[DIAS SOB-ENCOMENDA]", product.MadeToOrderDays.ToString())
            .Replace("[REFERÊNCIA]", product.Reference)
            .Replace("[DIMENSÕES DO PRODUTO]", $"Altura {product.Height}cm x Largura {product.Width}cm x Comprimento {product.Length}cm");
    }

    private static string Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Select(character => invalid.Contains(character) ? '-' : character).ToArray()).Trim().TrimEnd('.');
    }
}
