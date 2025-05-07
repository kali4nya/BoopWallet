using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Projektanker.Icons.Avalonia;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BoopWallet.Views;

public partial class WalletCreationWindow : Window
{
    public WalletCreationWindow()
    {
        InitializeComponent();
        HookPasswordToggle(ShowKeyButton, PrivateKeyBoxHidden, PrivateKeyBoxVisible);
    }
    private void HookPasswordToggle(Button toggleButton, TextBox hiddenBox, TextBox visibleBox)
    {
        toggleButton.Click += (_, _) =>
        {
            if (hiddenBox.IsVisible)
            {
                hiddenBox.IsVisible = false;
                visibleBox.IsVisible = true;
                visibleBox.Text = hiddenBox.Text;
                visibleBox.Focus();
                Attached.SetIcon(toggleButton, "fa-solid fa-eye");
            }
            else
            {
                visibleBox.IsVisible = false;
                hiddenBox.IsVisible = true;
                hiddenBox.Text = visibleBox.Text;
                hiddenBox.Focus();
                Attached.SetIcon(toggleButton, "fa-solid fa-eye-slash");
            }
        };
    }
}