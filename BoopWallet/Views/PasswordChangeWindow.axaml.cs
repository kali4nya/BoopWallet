using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Projektanker.Icons.Avalonia;
using System;
using System.Buffers.Text;
using System.IO;
using System.Text;
using static PasswordHasher;

namespace BoopWallet.Views;

public partial class PasswordChangeWindow : Window
{
    public PasswordChangeWindow()
    {
        InitializeComponent();
        CancelButton.Click += CancelButton_Click;
        SaveButton.Click += SaveButton_Click;

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

    private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }
    private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //checking if the entered old password is correct
        string[] passwordFile = File.ReadAllLines("password.txt");

        string oldPasswordHash = passwordFile[0];
        string oldPasswordSalt = passwordFile[1];

        string EnteredOldPasswordRaw = null;
        if (OldPasswordBoxHidden.IsVisible == true)
        {
            EnteredOldPasswordRaw = string.IsNullOrEmpty(OldPasswordBoxHidden.Text) ? "123" : OldPasswordBoxHidden.Text;
        }
        else
        {
            EnteredOldPasswordRaw = string.IsNullOrEmpty(OldPasswordBoxVisible.Text) ? "123" : OldPasswordBoxVisible.Text;
        }
        byte[] saltBytes = Convert.FromBase64String(oldPasswordSalt);

        string enteredOldPasswordHash = HashPassword(EnteredOldPasswordRaw, salt: saltBytes).Item1;

        if (enteredOldPasswordHash == oldPasswordHash)
        {
            //checking if the new password is the same as the repeated password
            string newPasswordRaw = null;
            if (PasswordBoxHidden.IsVisible == true)
            {
                newPasswordRaw = string.IsNullOrEmpty(PasswordBoxHidden.Text) ? "123" : PasswordBoxHidden.Text;
            }
            else
            {
                newPasswordRaw = string.IsNullOrEmpty(PasswordBoxVisible.Text) ? "123" : PasswordBoxVisible.Text;
            }
            string repeatedPasswordRaw = null;
            if (RepeatPasswordBoxHidden.IsVisible == true)
            {
                repeatedPasswordRaw = string.IsNullOrEmpty(RepeatPasswordBoxHidden.Text) ? "123" : RepeatPasswordBoxHidden.Text;
            }
            else
            {
                repeatedPasswordRaw = string.IsNullOrEmpty(RepeatPasswordBoxVisible.Text) ? "123" : RepeatPasswordBoxVisible.Text;
            }
            if (newPasswordRaw == repeatedPasswordRaw)
            {
                //hashing the new password
                var newPasswordHash = HashPassword(newPasswordRaw);
                //saving the new password to the file
                File.WriteAllText("password.txt", $"{newPasswordHash.Item1}\n{newPasswordHash.Item2}");
                this.Close();
            }
        }
    }
}