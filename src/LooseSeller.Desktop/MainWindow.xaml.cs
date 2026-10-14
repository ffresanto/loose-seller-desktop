using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;
using Microsoft.Win32;
using LooseSeller.Application.Services;
using LooseSeller.Infrastructure.Configuration;
using LooseSeller.Domain.Entities;
using LooseSeller.Infrastructure.Storage;

namespace LooseSeller.Desktop;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<SelectedImage> _selectedImages = [];
    private readonly ObservableCollection<SizeInput> _sizes = [];
    private readonly ProductPreparationService _productPreparationService;
    private readonly JsonSettingsProvider _settingsProvider = new();
    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Loose Seller",
        "appsettings.json");
    private Rect _restoreBounds;
    private bool _isWorkAreaMaximized;

    public MainWindow()
    {
        InitializeComponent();
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        if (!File.Exists(_settingsPath))
        {
            File.Copy(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), _settingsPath);
        }
        var settings = _settingsProvider.Load(_settingsPath);
        ApplicationVersionTextBlock.Text = $"v{settings.Application.Version}";
        ProductsDirectoryTextBox.Text = settings.Application.ProductsDirectory;
        ImagesListBox.ItemsSource = _selectedImages;
        SizesListBox.ItemsSource = _sizes;
        var productsPath = string.IsNullOrWhiteSpace(settings.Application.ProductsDirectory) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LooseArt 3D") : settings.Application.ProductsDirectory;
        var templatePath = Path.Combine(AppContext.BaseDirectory, settings.Application.DescriptionTemplateFile);
        _productPreparationService = new ProductPreparationService(new FileProductStorage(productsPath, templatePath));
    }

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) { MaximizeButton_Click(sender, e); return; }
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isWorkAreaMaximized)
        {
            Left = _restoreBounds.Left;
            Top = _restoreBounds.Top;
            Width = _restoreBounds.Width;
            Height = _restoreBounds.Height;
            _isWorkAreaMaximized = false;
            return;
        }

        _restoreBounds = new Rect(Left, Top, Width, Height);
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left;
        Top = workArea.Top;
        Width = workArea.Width;
        Height = workArea.Height;
        _isWorkAreaMaximized = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void DeveloperLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void SelectModelButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Selecione o arquivo de impressão 3D", Filter = "Arquivos 3D|*.stl;*.3mf;*.obj;*.fbx|Todos os arquivos|*.*" };
        if (dialog.ShowDialog() == true) ModelPathTextBox.Text = dialog.FileName;
    }

    private void SelectProductsDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Selecione onde os produtos serão salvos",
            InitialDirectory = Directory.Exists(ProductsDirectoryTextBox.Text)
                ? ProductsDirectoryTextBox.Text
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog() != true) return;

        ProductsDirectoryTextBox.Text = dialog.FolderName;
        var settings = _settingsProvider.Load(_settingsPath);
        settings.Application.ProductsDirectory = dialog.FolderName;
        _settingsProvider.Save(_settingsPath, settings);
        StatusTextBlock.Text = "Diretório de saída salvo.";
    }

    private void ReferenceTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var text = new string(ReferenceTextBox.Text.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        if (text.Length > 0 && text[0] != 'L') text = "L" + text;
        if (text.Length > 1 && text[1] != 'A') text = text[0] + "A" + text[1..];
        text = new string(text.Take(2).Concat(text.Skip(2).Where(char.IsDigit).Take(4)).ToArray());
        if (ReferenceTextBox.Text != text)
        {
            ReferenceTextBox.Text = text;
            ReferenceTextBox.CaretIndex = text.Length;
        }
    }

    private void ReferenceTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        => e.Handled = e.Text.Any(character => !char.IsDigit(character));

    private void MeasureTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && (textBox.Text == "Largura" || textBox.Text == "Altura" || textBox.Text == "Comprimento"))
        {
            textBox.Clear();
            textBox.Foreground = System.Windows.Media.Brushes.White;
        }
    }

    private void MeasureTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
        {
            textBox.Text = textBox.Name switch
            {
                "WidthTextBox" => "Largura",
                "HeightTextBox" => "Altura",
                _ => "Comprimento"
            };
            textBox.Foreground = (System.Windows.Media.Brush)FindResource("MutedBrush");
        }
    }

    private void AddSizeButton_Click(object sender, RoutedEventArgs e)
    {
        var size = new SizeInput(SizeNameTextBox.Text, ParseDecimal(HeightTextBox.Text, "altura"), ParseDecimal(WidthTextBox.Text, "largura"), ParseDecimal(DepthTextBox.Text, "comprimento"));
        _sizes.Add(size);
        SizeNameTextBox.Clear();
        HeightTextBox.Text = "Altura";
        WidthTextBox.Text = "Largura";
        DepthTextBox.Text = "Comprimento";
        StatusTextBlock.Text = $"{_sizes.Count} tamanho(s) adicionado(s).";
    }

    private void RemoveSizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: SizeInput size }) _sizes.Remove(size);
    }

    private void MeasureTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        => e.Handled = e.Text.Any(character => !char.IsDigit(character) && character != ',' && character != '.');

    private void SelectImagesButton_Click(object sender, RoutedEventArgs e)
    {
        var remaining = 9 - _selectedImages.Count;
        if (remaining <= 0) { MessageBox.Show("Você já selecionou nove imagens.", "Limite atingido", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        var dialog = new OpenFileDialog { Title = "Selecione as imagens do produto", Filter = "Imagens|*.jpg;*.jpeg;*.png;*.webp|Todos os arquivos|*.*", Multiselect = true };
        if (dialog.ShowDialog() != true) return;
        foreach (var file in dialog.FileNames.Take(remaining)) _selectedImages.Add(new SelectedImage(_selectedImages.Count + 1, file));
        StatusTextBlock.Text = $"{_selectedImages.Count} de 9 imagens selecionadas.";
    }

    private void ClearImagesButton_Click(object sender, RoutedEventArgs e) { _selectedImages.Clear(); StatusTextBlock.Text = "Imagens removidas."; }

    private void ClearFormButton_Click(object sender, RoutedEventArgs e)
    {
        TitleTextBox.Clear();
        MaterialTextBox.Text = "PLA";
        MadeToOrderDaysTextBox.Text = "4";
        ReferenceTextBox.Text = "LA";
        HeightTextBox.Text = "Altura";
        WidthTextBox.Text = "Largura";
        DepthTextBox.Text = "Comprimento";
        HeightTextBox.Foreground = (System.Windows.Media.Brush)FindResource("MutedBrush");
        WidthTextBox.Foreground = (System.Windows.Media.Brush)FindResource("MutedBrush");
        DepthTextBox.Foreground = (System.Windows.Media.Brush)FindResource("MutedBrush");
        ModelPathTextBox.Clear();
        _selectedImages.Clear();
        _sizes.Clear();
        SizeNameTextBox.Text = "Único";
        StatusTextBlock.Text = "Formulário limpo.";
    }

    private async void PrepareProductButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ClearValidationMarks();
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text)) throw new InvalidOperationException("Informe o título do produto.");
            if (string.IsNullOrWhiteSpace(ModelPathTextBox.Text) || !File.Exists(ModelPathTextBox.Text)) throw new InvalidOperationException("Selecione um arquivo de impressão 3D válido.");
            var sizes = _sizes.Count > 0
                ? _sizes.Select(size => new ProductSize(size.Name, size.Height, size.Width, size.Length))
                : [new ProductSize(SizeNameTextBox.Text, ParseDecimal(HeightTextBox.Text, "altura"), ParseDecimal(WidthTextBox.Text, "largura"), ParseDecimal(DepthTextBox.Text, "comprimento"))];
            var product = new Product(TitleTextBox.Text, MaterialTextBox.Text, ParseInteger(MadeToOrderDaysTextBox.Text, "dias sob encomenda"), ReferenceTextBox.Text, ModelPathTextBox.Text, sizes, _selectedImages.Select(image => image.FullPath));
            StatusTextBlock.Text = "Preparando arquivos..."; SetControlsEnabled(false);
            var folder = await _productPreparationService.PrepareAsync(product);
            StatusTextBlock.Text = $"Produto preparado em: {folder}";
            new OperationResultWindow("Produto preparado", $"O produto foi criado com sucesso.\n\nPasta:\n{folder}", true)
            {
                Owner = this
            }.ShowDialog();
        }
        catch (Exception exception)
        {
            HighlightInvalidFields(exception.Message);
            StatusTextBlock.Text = "Não foi possível preparar o produto.";
            new OperationResultWindow("Não foi possível preparar", exception.Message, false)
            {
                Owner = this
            }.ShowDialog();
        }
        finally { SetControlsEnabled(true); }
    }

    private void ClearValidationMarks()
    {
        foreach (var textBox in new[]
        {
            TitleTextBox, MaterialTextBox, MadeToOrderDaysTextBox, ReferenceTextBox,
            HeightTextBox, WidthTextBox, DepthTextBox, ModelPathTextBox, SizeNameTextBox
        })
        {
            textBox.ClearValue(BorderBrushProperty);
        }
    }

    private void HighlightInvalidFields(string message)
    {
        var invalidBrush = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF8FAD"));
        TextBox? firstInvalid = null;

        void Mark(TextBox textBox)
        {
            textBox.BorderBrush = invalidBrush;
            firstInvalid ??= textBox;
        }

        if (message.Contains("título", StringComparison.OrdinalIgnoreCase)) Mark(TitleTextBox);
        if (message.Contains("arquivo 3D", StringComparison.OrdinalIgnoreCase)) Mark(ModelPathTextBox);
        if (message.Contains("referência", StringComparison.OrdinalIgnoreCase)) Mark(ReferenceTextBox);
        if (message.Contains("altura", StringComparison.OrdinalIgnoreCase)) Mark(HeightTextBox);
        if (message.Contains("largura", StringComparison.OrdinalIgnoreCase)) Mark(WidthTextBox);
        if (message.Contains("comprimento", StringComparison.OrdinalIgnoreCase)) Mark(DepthTextBox);
        if (message.Contains("tamanho", StringComparison.OrdinalIgnoreCase)) Mark(SizeNameTextBox);

        firstInvalid?.Focus();
    }

    private static decimal ParseDecimal(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || value is "Largura" or "Altura" or "Comprimento") return 0;
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var result) && result >= 0) return result;
        throw new InvalidOperationException($"Informe uma medida válida para {fieldName}.");
    }

    private void SetControlsEnabled(bool enabled)
    {
        TitleTextBox.IsEnabled = enabled; MaterialTextBox.IsEnabled = enabled; MadeToOrderDaysTextBox.IsEnabled = enabled; ReferenceTextBox.IsEnabled = enabled; WidthTextBox.IsEnabled = enabled; HeightTextBox.IsEnabled = enabled; DepthTextBox.IsEnabled = enabled; ModelPathTextBox.IsEnabled = enabled; ImagesListBox.IsEnabled = enabled;
    }

    private static int ParseInteger(string value, string fieldName)
    {
        if (int.TryParse(value, out var result) && result >= 0) return result;
        throw new InvalidOperationException($"Informe um valor válido para {fieldName}.");
    }

    private sealed record SelectedImage(int Position, string FullPath)
    {
        public string FileName => Path.GetFileName(FullPath);
        public string FileSize => FormatFileSize(new FileInfo(FullPath).Length);

        private static string FormatFileSize(long bytes)
            => bytes < 1024 ? $"{bytes} B" : $"{bytes / 1024d:0.0} KB";
    }

    private sealed record SizeInput(string Name, decimal Height, decimal Width, decimal Length)
    {
        public string Summary => $"{Name} - Altura {Height}cm x Largura {Width}cm x Comprimento {Length}cm";
    }
}
