using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;

/*
 * examepl toggle in XAML
 *  <CheckBox x:Name="cbToggle" Height="45" Width="65" IsChecked="False" Checked="cbToggle_Checked" Unchecked="cbToggle_Unchecked" />
 *  
 */

namespace Jotter
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {

        //public class CustomToggleButton : CheckBox
        //{
        //    public CustomToggleButton()
        //    {
        //        DefaultStyleKey = typeof(CustomToggleButton);
        //    }
        //}

        public Settings()
        {
            InitializeComponent();
        }



        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Set the location of the data file in UI at load up
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextUserData.Text = Jotter.MainWindow.jotNotesFilePath;
            TextLogFile.Text = Jotter.MainWindow.logger.LogFile;
        }

        private void TopBorderGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
