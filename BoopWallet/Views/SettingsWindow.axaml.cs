using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BoopWallet.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        CreateWalletButton.Click += CreateWalletButton_Click;
        ChangePasswordButton.Click += ChangePasswordButton_Click;
    }
    private async void CreateWalletButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var WalletCreationWindow = new WalletCreationWindow();
        await WalletCreationWindow.ShowDialog(this);
    }
    private async void ChangePasswordButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var PasswordChangeWindow = new PasswordChangeWindow();
        await PasswordChangeWindow.ShowDialog(this);
    }
}