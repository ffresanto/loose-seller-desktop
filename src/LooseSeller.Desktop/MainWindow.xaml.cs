using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
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

    public MainWindow()
    {
        InitializeComponent();
        var settings = new JsonSettingsProvider().Load(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));
        ApplicationVersionTextBlock.Text = $"v{settings.Application.Version}";
        ImagesListBox.ItemsSource = _selectedImages;
        var productsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LooseArt 3D");
        _productPreparationService = new ProductPreparationService(new FileProductStorage(productsPath));
    }

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) { MaximizeButton_Click(sender, e); return; }
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void SelectModelButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Selecione o arquivo de impressão 3D", Filter = "Arquivos 3D|*.stl;*.3mf;*.obj;*.fbx|Todos os arquivos|*.*" };
        if (dialog.ShowDialog() == true) ModelPathTextBox.Text = dialog.FileName;
    }

    private void SelectImagesButton_Click(object sender, RoutedEventArgs e)
    {
        var remaining = 5 - _selectedImages.Count;
        if (remaining <= 0) { MessageBox.Show("Você já selecionou cinco imagens.", "Limite atingido", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        var dialog = new OpenFileDialog { Title = "Selecione as imagens do produto", Filter = "Imagens|*.jpg;*.jpeg;*.png;*.webp|Todos os arquivos|*.*", Multiselect = true };
        if (dialog.ShowDialog() != true) return;
        foreach (var file in dialog.FileNames.Take(remaining)) _selectedImages.Add(new SelectedImage(_selectedImages.Count + 1, file));
        StatusTextBlock.Text = $"{_selectedImages.Count} de 5 imagens selecionadas.";
    }

    private void ClearImagesButton_Click(object sender, RoutedEventArgs e) { _selectedImages.Clear(); StatusTextBlock.Text = "Imagens removidas."; }

    private async void PrepareProductButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text)) throw new InvalidOperationException("Informe o título do produto.");
            if (string.IsNullOrWhiteSpace(ModelPathTextBox.Text) || !File.Exists(ModelPathTextBox.Text)) throw new InvalidOperationException("Selecione um arquivo de impressão 3D válido.");
            var product = new Product(TitleTextBox.Text, ParseDecimal(WidthTextBox.Text, "largura"), ParseDecimal(HeightTextBox.Text, "altura"), ParseDecimal(DepthTextBox.Text, "profundidade"), MaterialTextBox.Text, ColorTextBox.Text, ModelPathTextBox.Text, _selectedImages.Select(image => image.FullPath));
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
        if (string.IsNullOrWhiteSpace(value)) return 0;
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var result) && result >= 0) return result;
        throw new InvalidOperationException($"Informe uma medida válida para {fieldName}.");
    }

    private void SetControlsEnabled(bool enabled)
    {
        TitleTextBox.IsEnabled = enabled; MaterialTextBox.IsEnabled = enabled; ColorTextBox.IsEnabled = enabled; WidthTextBox.IsEnabled = enabled; HeightTextBox.IsEnabled = enabled; DepthTextBox.IsEnabled = enabled; ModelPathTextBox.IsEnabled = enabled; ImagesListBox.IsEnabled = enabled;
    }

    private sealed record SelectedImage(int Position, string FullPath) { public string FileName => Path.GetFileName(FullPath); }
}
