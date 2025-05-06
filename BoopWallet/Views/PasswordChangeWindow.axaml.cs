using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Projektanker.Icons.Avalonia;

namespace BoopWallet.Views;

public partial class PasswordChangeWindow : Window
{
    public PasswordChangeWindow()
    {
        InitializeComponent();
        CancelButton.Click += CancelButton_Click;

        // Hook up show/hide logic for each field
        HookPasswordToggle(ShowOldPasswordButton, OldPasswordBoxHidden, OldPasswordBoxVisible);
        HookPasswordToggle(ShowPasswordButton, PasswordBoxHidden, PasswordBoxVisible);
        HookPasswordToggle(ShowRepeatPasswordButton, RepeatPasswordBoxHidden, RepeatPasswordBoxVisible);
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

    private async void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }
}