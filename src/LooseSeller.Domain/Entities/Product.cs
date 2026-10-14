namespace LooseSeller.Domain.Entities;

public sealed class Product
{
    public string Title { get; }
    public decimal Width { get; }
    public decimal Height { get; }
    public decimal Length { get; }
    public string Material { get; }
    public int MadeToOrderDays { get; }
    public string Reference { get; }
    public string ModelFilePath { get; }
    public IReadOnlyCollection<string> ImageFilePaths { get; }

    public Product(string title, decimal width, decimal height, decimal length, string material, int madeToOrderDays, string reference, string modelFilePath, IEnumerable<string> imageFilePaths)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("O título é obrigatório.", nameof(title));
        if (string.IsNullOrWhiteSpace(modelFilePath)) throw new ArgumentException("O arquivo 3D é obrigatório.", nameof(modelFilePath));

        var images = imageFilePaths.Where(path => !string.IsNullOrWhiteSpace(path)).ToList();
        if (images.Count > 9) throw new ArgumentException("O produto pode ter no máximo nove imagens.", nameof(imageFilePaths));
        if (madeToOrderDays < 0) throw new ArgumentException("O prazo não pode ser negativo.", nameof(madeToOrderDays));
        var normalizedReference = reference?.Trim() ?? string.Empty;
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedReference, "^LA[0-9]{4}$")) throw new ArgumentException("A referência deve seguir o formato LA0000.", nameof(reference));

        Title = title.Trim();
        Width = width;
        Height = height;
        Length = length;
        Material = material?.Trim() ?? string.Empty;
        MadeToOrderDays = madeToOrderDays;
        Reference = normalizedReference.ToUpperInvariant();
        ModelFilePath = modelFilePath;
        ImageFilePaths = images.AsReadOnly();
    }
}
