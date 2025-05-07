using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Projektanker.Icons.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using static WalletCreation_BTC;

namespace BoopWallet.Views;

public partial class WalletCreationWindow : Window
{
    public WalletCreationWindow()
    {
        InitializeComponent();
        HookPasswordToggle(ShowKeyButton, PrivateKeyBoxHidden, PrivateKeyBoxVisible);

        CryptoChoiceCombobox?.Items.Clear();
        foreach (var item in new[] { "BTC", "ETH", "XMR", "XRP" })
        {
            CryptoChoiceCombobox?.Items.Add(item);
        }

        if (CryptoChoiceCombobox != null)
        {
            CryptoChoiceCombobox.SelectionChanged += CryptoChoiceCombobox_SelectionChanged;
        }
        CancelButton.Click += CancelButton_Click;
        CreateButton.Click += CreateButton_Click;
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
    private void CryptoChoiceCombobox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var comboBox = sender as ComboBox;
        var selected = comboBox?.SelectedItem;
        if (selected?.ToString() == "BTC")
        {
            HideEveryField();
            ShowBasicFields();
        }
        else if (selected?.ToString() == "ETH")
        {
            HideEveryField();
            ShowBasicFields();
        }
        else if (selected?.ToString() == "XMR")
        {
            HideEveryField();
            // Handle XMR selection
        }
        else if (selected?.ToString() == "XRP")
        {
            HideEveryField();
            // Handle XRP selection
        }
    }
    //show public key field, private key field, recovery phrase field
    private void ShowBasicFields()
    {
        PublicKeyField.IsVisible = true;
        PrivateKeyField.IsVisible = true;
        RecoveryPhraseField.IsVisible = true;
    }
    private void HideEveryField()
    {
        PublicKeyField.IsVisible = false;
        PrivateKeyField.IsVisible = false;
        RecoveryPhraseField.IsVisible = false;
    }
    private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }
    private void CreateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string ?SelectedCrypto = CryptoChoiceCombobox?.SelectedItem?.ToString();
        if (SelectedCrypto == "BTC")
        {

        }
    }
}