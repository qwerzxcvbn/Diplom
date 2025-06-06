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

namespace StuldastolClient.MessageWindow
{
    /// <summary>
    /// Логика взаимодействия для FalsePromoWindow.xaml
    /// </summary>
    public partial class FalsePromoWindow : Window
    {
        public FalsePromoWindow()
        {
            InitializeComponent();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Устанавливаем результат диалога
            this.Close();
        }
    }
}
