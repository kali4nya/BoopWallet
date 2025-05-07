using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BoopWallet.ViewModels;
using BoopWallet.Views;
using System.IO;
using System;
using static PasswordHasher;

namespace BoopWallet;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        //generating password file //default password is "123"
        try
        {
            string filePath = "password.txt";

            if (!File.Exists(filePath))
            {
                string password = "123";
                var hashedPassword = HashPassword(password);
                File.WriteAllText(filePath, $"{hashedPassword.Item1}\n{hashedPassword.Item2}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating password file: {ex.Message}");
        }

        base.OnFrameworkInitializationCompleted();
    }
}
