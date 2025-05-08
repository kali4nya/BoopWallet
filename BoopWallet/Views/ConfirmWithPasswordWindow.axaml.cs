using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Projektanker.Icons.Avalonia;
using System;
using System.Threading.Tasks;

namespace BoopWallet.Views;

public partial class ConfirmWithPasswordWindow : Window
{
    private TaskCompletionSource<string?> _passwordTaskSource = new();
    public ConfirmWithPasswordWindow()
    {
        InitializeComponent();

        HookPasswordToggle(ShowPasswordButton, PasswordBoxHidden, PasswordBoxVisible);
        CancelButton.Click += CancelButton_Click;
        ConfirmButton.Click += ConfirmButton_Click;
        this.Closed += OnClosed;
    }
    private void OnClosed(object? sender, EventArgs e)
    {
        _passwordTaskSource.TrySetResult(null);
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
    private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _passwordTaskSource.TrySetResult(null);
        Close();
    }
    private void ConfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        
        _passwordTaskSource.TrySetResult(PasswordBoxHidden.Text);
        Close();
    }
    public async Task<string?> ShowDialogAsync(Window parent)
    {
        await base.ShowDialog(parent);
        return await _passwordTaskSource.Task;
    }
}