using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace FocusGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel vm = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = vm;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            vm.AutoFocusEnabled = false;
            vm.AutoExposureEnabled = false;
            vm.FocusValue = (int)Application.Current.Resources["DefaultFocus"];
            vm.ExposureValue = (int)Application.Current.Resources["DefaultExposure"];
        }
    }
}

