namespace LooseSeller.Business.Entities;

public sealed class Product
{
    public string Title { get; }
    public decimal Width { get; }
    public decimal Height { get; }
    public decimal Depth { get; }
    public string Material { get; }
    public string Color { get; }
    public string ModelFilePath { get; }
    public IReadOnlyCollection<string> ImageFilePaths { get; }

    public Product(string title, decimal width, decimal height, decimal depth, string material, string color, string modelFilePath, IEnumerable<string> imageFilePaths)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("O título é obrigatório.", nameof(title));
        if (string.IsNullOrWhiteSpace(modelFilePath)) throw new ArgumentException("O arquivo 3D é obrigatório.", nameof(modelFilePath));
        var images = imageFilePaths.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        if (images.Count > 5) throw new ArgumentException("O produto pode ter no máximo cinco imagens.", nameof(imageFilePaths));
        Title = title.Trim(); Width = width; Height = height; Depth = depth;
        Material = material?.Trim() ?? string.Empty; Color = color?.Trim() ?? string.Empty;
        ModelFilePath = modelFilePath; ImageFilePaths = images.AsReadOnly();
    }
}
