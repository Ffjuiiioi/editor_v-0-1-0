using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace FWGE_Editor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Проверяем аргументы командной строки
        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            string projectPath = args[1];
            if (Directory.Exists(projectPath))
            {
                // Загружаем проект автоматически
                var mainPage = this.Content as MainPage;
                if (mainPage != null)
                {
                    mainPage.LoadProject(projectPath);
                }
            }
        }
    }
}