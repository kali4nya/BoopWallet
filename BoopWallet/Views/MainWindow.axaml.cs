using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BoopWallet.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SettingsButton.Click += SettingsButton_Click;
    }
    private async void SettingsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        await settingsWindow.ShowDialog(this);
    }
}
