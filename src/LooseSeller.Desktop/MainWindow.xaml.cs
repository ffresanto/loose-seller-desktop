using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
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
    private readonly ProductPreparationService _productPreparationService;
    private readonly JsonSettingsProvider _settingsProvider = new();
    private readonly string _settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    private Rect _restoreBounds;
    private bool _isWorkAreaMaximized;

    public MainWindow()
    {
        InitializeComponent();
        var settings = _settingsProvider.Load(_settingsPath);
        ApplicationVersionTextBlock.Text = $"v{settings.Application.Version}";
        ProductsDirectoryTextBox.Text = settings.Application.ProductsDirectory;
        ImagesListBox.ItemsSource = _selectedImages;
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
        StatusTextBlock.Text = "Formulário limpo.";
    }

    private async void PrepareProductButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text)) throw new InvalidOperationException("Informe o título do produto.");
            if (string.IsNullOrWhiteSpace(ModelPathTextBox.Text) || !File.Exists(ModelPathTextBox.Text)) throw new InvalidOperationException("Selecione um arquivo de impressão 3D válido.");
            var product = new Product(TitleTextBox.Text, ParseDecimal(WidthTextBox.Text, "largura"), ParseDecimal(HeightTextBox.Text, "altura"), ParseDecimal(DepthTextBox.Text, "profundidade"), MaterialTextBox.Text, ParseInteger(MadeToOrderDaysTextBox.Text, "dias sob encomenda"), ReferenceTextBox.Text, ModelPathTextBox.Text, _selectedImages.Select(image => image.FullPath));
            StatusTextBlock.Text = "Preparando arquivos..."; SetControlsEnabled(false);
            var folder = await _productPreparationService.PrepareAsync(product);
            StatusTextBlock.Text = $"Produto preparado em: {folder}";
            MessageBox.Show($"Produto preparado com sucesso!\n\nPasta criada em:\n{folder}", "Concluído", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception exception) { StatusTextBlock.Text = "Não foi possível preparar o produto."; MessageBox.Show(exception.Message, "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning); }
        finally { SetControlsEnabled(true); }
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
}
