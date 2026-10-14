namespace LooseSeller.Domain.Entities;

public sealed class ProductSize
{
    public string Name { get; }
    public decimal Height { get; }
    public decimal Width { get; }
    public decimal Length { get; }

    public ProductSize(string name, decimal height, decimal width, decimal length)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome do tamanho é obrigatório.", nameof(name));
        if (height < 0 || width < 0 || length < 0) throw new ArgumentException("As medidas não podem ser negativas.");

        Name = name.Trim();
        Height = height;
        Width = width;
        Length = length;
    }
}
