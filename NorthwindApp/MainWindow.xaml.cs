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
        CollectionViewSource categoriesViewSource;
        CollectionViewSource supplierViewSource;
        public MainWindow()
        {
            InitializeComponent();
            productViewSource3 = (CollectionViewSource)(FindResource("productViewSource3"));
            categoriesViewSource = (CollectionViewSource)(FindResource("categoriesViewSource"));
            supplierViewSource = (CollectionViewSource)(FindResource("supplierViewSource"));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            context.Products.Load();
            context.Categories.Load();
            context.Suppliers.Load();
            productViewSource3.Source = context.Products.Local;
            categoriesViewSource.Source = context.Categories.Local;
            supplierViewSource.Source= context.Suppliers.Local;
            this.SizeToContent = SizeToContent.Width;
        }

        private void Item_Edit(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                IDEdit.Text = (row.Item as Product).ProductID.ToString();
                NameEdit.Text = (row.Item as Product).ProductName.ToString();
                QtyUnitEdit.Text = (row.Item as Product).QuantityPerUnit.ToString();
                CategoriesComboEdit.SelectedItem = (row.Item as Product).Category;
                StockEdit.Text = (row.Item as Product).UnitsInStock.ToString();
                PriceEdit.Text = (row.Item as Product).UnitPrice.ToString();
                DiscontinuedEdit.IsChecked = (row.Item as Product).Discontinued;
                SupplierComboEdit.SelectedItem= (row.Item as Product).Supplier;
                ReorderLevelEdit.Text = (row.Item as Product).ReorderLevel.ToString();
            }
        }
        private void Run_Product_Query(object sender, RoutedEventArgs e)
        {
            Product product = new Product()
            {
                ProductID = int.Parse(IDEdit.Text),
                ProductName = NameEdit.Text,
                QuantityPerUnit = QtyUnitEdit.Text,
                Category = CategoriesComboEdit.SelectedItem as Category,
                UnitsInStock = short.Parse(StockEdit.Text),
                UnitPrice = decimal.Parse(PriceEdit.Text),
                Discontinued = (bool)DiscontinuedEdit.IsChecked,
                Supplier = SupplierComboEdit.SelectedItem as Supplier,
                ReorderLevel = short.Parse(ReorderLevelEdit.Text)
            };
            if(context.Products.Any(x => x.ProductID == product.ProductID))
            {
                var toUpdate = context.Products.Find(product.ProductID);
                toUpdate.ProductName = product.ProductName;
                toUpdate.QuantityPerUnit = product.QuantityPerUnit;
                toUpdate.Category = product.Category;
                toUpdate.UnitsInStock = product.UnitsInStock;
                toUpdate.UnitPrice = product.UnitPrice;
                toUpdate.Discontinued = product.Discontinued;
                toUpdate.Supplier = product.Supplier;
                toUpdate.ReorderLevel = product.ReorderLevel;
                context.SaveChanges();
                productViewSource3.View.Refresh();
            }
            else
            {
                context.Products.Add(product);
                context.SaveChanges();
                productViewSource3.View.Refresh();
            }
        }

        private void Delete_Product_Query(object sender, RoutedEventArgs e)
        {
            Product product = new Product()
            {
                ProductID = int.Parse(IDEdit.Text),
                ProductName = NameEdit.Text,
                QuantityPerUnit = QtyUnitEdit.Text,
                Category = CategoriesComboEdit.SelectedItem as Category,
                UnitsInStock = short.Parse(StockEdit.Text),
                UnitPrice = decimal.Parse(PriceEdit.Text),
                Discontinued = (bool)DiscontinuedEdit.IsChecked,
                Supplier = SupplierComboEdit.SelectedItem as Supplier,
                ReorderLevel = short.Parse(ReorderLevelEdit.Text)
            };
            if (context.Products.Any(x => x.ProductID == product.ProductID))
            {
                var toRemove = context.Products.Find(product.ProductID);
                if (toRemove != null) 
                { 
                    context.Products.Remove(toRemove);
                    context.SaveChanges();
                    productViewSource3.View.Refresh();
                }
            }
        }
    }
}
