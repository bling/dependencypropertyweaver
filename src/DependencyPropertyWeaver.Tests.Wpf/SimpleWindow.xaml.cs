using System.Windows;
using DependencyPropertyWeaver.Tests.Models;

namespace DependencyPropertyWeaver.Tests.Wpf
{
    /// <summary>
    /// Interaction logic for SimpleWindow.xaml
    /// </summary>
    public partial class SimpleWindow : Window
    {
        public Simple ViewModel { get; private set; }

        public SimpleWindow()
        {
            InitializeComponent();
            ViewModel = new Simple();
            DataContext = ViewModel;
        }
    }
}