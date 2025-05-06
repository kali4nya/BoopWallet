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
        LoadComboBoxItems("crypto_options.json");
    }
    public class CryptoEntry
    {
        public List<string> Supported { get; set; }
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
    private void LoadComboBoxItems(string jsonFilePath)
    {
        Dictionary<string, CryptoEntry> options;

        if (!File.Exists(jsonFilePath))
        {
            options = new Dictionary<string, CryptoEntry>
            {
                ["BTC"] = new CryptoEntry
                {
                    Supported = new List<string> { "publicKey", "privateKey", "recoveryPhrase" }
                },
                ["ETH"] = new CryptoEntry
                {
                    Supported = new List<string> { "publicKey", "privateKey", "recoveryPhrase" }
                }
            };

            var json = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonFilePath, json);
        }
        else
        {
            var jsonData = File.ReadAllText(jsonFilePath);
            options = JsonSerializer.Deserialize<Dictionary<string, CryptoEntry>>(jsonData);
        }

        CryptoChoiceCombobox.Items.Clear();
        foreach (var key in options.Keys)
        {
            CryptoChoiceCombobox.Items.Add(key);
        }
    }
}