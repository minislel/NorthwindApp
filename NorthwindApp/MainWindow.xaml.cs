using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NorthwindApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NorthwindEntities context = new NorthwindEntities();
        CollectionViewSource productViewSource3;
        public MainWindow()
        {
            InitializeComponent();
            productViewSource3 = (CollectionViewSource)(FindResource("productViewSource3"));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            /*            System.Windows.Data.CollectionViewSource customerViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("customerViewSource")));
                        // Load data by setting the CollectionViewSource.Source property:
                        // customerViewSource.Source = [generic data source]
                        System.Windows.Data.CollectionViewSource customerViewSource1 = ((System.Windows.Data.CollectionViewSource)(this.FindResource("customerViewSource1")));
                        // Load data by setting the CollectionViewSource.Source property:
                        // customerViewSource1.Source = [generic data source]
                        System.Windows.Data.CollectionViewSource productViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productViewSource")));
                        // Load data by setting the CollectionViewSource.Source property:
                        // productViewSource.Source = [generic data source]
                        System.Windows.Data.CollectionViewSource productViewSource1 = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productViewSource1")));
                        // Load data by setting the CollectionViewSource.Source property:
                        // productViewSource1.Source = [generic data source]
                        System.Windows.Data.CollectionViewSource productViewSource2 = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productViewSource2")));
                        // Load data by setting the CollectionViewSource.Source property:
                        // productViewSource2.Source = [generic data source]
                        System.Windows.Data.CollectionViewSource productViewSource3 = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productViewSource3")));
                        // Load data by setting the CollectionViewSource.Source property:
                        // productViewSource3.Source = [generic data source]*/
            context.Products.Load();
            productViewSource3.Source = context.Products.Local;
            this.SizeToContent = SizeToContent.Width;
        }

        private void Item_Edit(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                IDEdit.Text = (row.Item as Product).ProductID.ToString();
            }
        }
    }
}
