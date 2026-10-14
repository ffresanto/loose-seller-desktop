using System.Windows;

namespace LooseSeller.Desktop;

public partial class OperationResultWindow : Window
{
    public OperationResultWindow(string title, string message, bool success)
    {
        InitializeComponent();
        TitleTextBlock.Text = title;
        MessageTextBlock.Text = message;
        TitleTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(success ? "#D98DFF" : "#FF8FAD"));
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
