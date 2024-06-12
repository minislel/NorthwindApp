using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static System.Net.Mime.MediaTypeNames;

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
        CollectionViewSource customerViewSource;
        CollectionViewSource orderViewSource;
        CollectionViewSource employeeViewSource;
        CollectionViewSource shipperViewSource;
        byte[] data = new byte[0];
        public ObservableCollection<KeyValuePair<Product, short>> cartItems = new ObservableCollection<KeyValuePair<Product, short>>();
        public MainWindow()
        {
            InitializeComponent();
            productViewSource3 = (CollectionViewSource)(FindResource("productViewSource3"));
            categoriesViewSource = (CollectionViewSource)(FindResource("categoriesViewSource"));
            supplierViewSource = (CollectionViewSource)(FindResource("supplierViewSource"));
            customerViewSource = (CollectionViewSource)(FindResource("customerViewSource"));
            orderViewSource = (CollectionViewSource)(FindResource("orderViewSource"));
            employeeViewSource = (CollectionViewSource)(FindResource("employeeViewSource"));
            shipperViewSource = (CollectionViewSource)(FindResource("shipperViewSource"));
           
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            context.Products.Load();
            context.Categories.Load();
            context.Suppliers.Load();
            context.Customers.Load();
            context.Employees.Load();
            context.Shippers.Load();
            context.Orders.Load(); 
            context.Order_Details.Load();
            
            productViewSource3.Source = context.Products.Local;
            categoriesViewSource.Source = context.Categories.Local;
            supplierViewSource.Source= context.Suppliers.Local;
            customerViewSource.Source = context.Customers.Local;
            orderViewSource.Source = context.Order_Details.Local;
            employeeViewSource.Source = context.Employees.Local;
            shipperViewSource.Source = context.Shippers.Local;
            
            this.SizeToContent = SizeToContent.Width;

            
        }
        private void NumberValidationTextBox_Double(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            // Use SelectionStart property to find the caret position.
            // Insert the previewed text into the existing text in the textbox.
            var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            double val;
            // If parsing is successful, set Handled to false
            e.Handled = !double.TryParse(fullText,
                                         NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                                         CultureInfo.InvariantCulture,
                                         out val);
        }
        private void NumberValidationTextBox_int(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            int val;
            e.Handled = !int.TryParse(fullText, out val);
        }
        #region item queries
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
            try
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
            catch (Exception ex) 
            {
                MessageBox.Show("Invalid values: "+ex.ToString());
            }
        }


        private void Delete_Product_Query(object sender, RoutedEventArgs e)
        {
            var Result = MessageBox.Show($"Are you sure? \nThis action will delete the product assigned to ID {IDEdit.Text} and all orders containing this product", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (Result == MessageBoxResult.Yes)
            {
                try
                {

                    Product product = new Product()
                    {
                        ProductID = int.Parse(IDEdit.Text)
                    };

                    if(context.Order_Details.Any(x => x.ProductID == product.ProductID))
                    {
                        List<Order> toRemoveOrders = new List<Order>();
                        var toRemoveDetails = context.Order_Details.Select(x => x).Where(x => x.ProductID == product.ProductID).ToList();
                        foreach(var item in toRemoveDetails)
                        {
                            toRemoveOrders.Add(context.Orders.Select(x => x).Where(x => x.OrderID == item.OrderID).SingleOrDefault());
                        }
                        foreach (var item in toRemoveDetails)
                        {
                            context.Order_Details.Remove(item);
                        }
                        context.SaveChanges();
                        foreach (var item in toRemoveOrders)
                        {

                            context.Orders.Remove(item);
                        }
                        context.SaveChanges();
                        productViewSource3.View.Refresh();
                    }
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
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid ID: " + ex.Message);

                }
            }
        }
        #endregion
        #region customer queries
        private void Customer_Edit(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                var customer = row.Item as Customer;
                CustIDEdit.Text = customer.CustomerID;
                CustNameEdit.Text = customer.CompanyName;
                CustContactEdit.Text = customer.ContactName;
                CustContactTitleEdit.Text = customer.ContactTitle;
                CustAddressEdit.Text = customer.Address;
                CustCityEdit.Text = customer.City;
                CustRegionEdit.Text = customer.Region;
                CustPostalEdit.Text = customer.PostalCode;
                CustCountryEdit.Text = customer.Country;
                CustPhoneEdit.Text = customer.Phone;
                CustFaxEdit.Text = customer.Fax;          
            }
        }
        private void Run_Customer_Query(object sender, RoutedEventArgs e)
        {
            try
            {
                Customer customer = new Customer()
                {
                    CustomerID = CustIDEdit.Text,
                    CompanyName = CustNameEdit.Text,
                    ContactName = CustContactEdit.Text,
                    ContactTitle = CustContactTitleEdit.Text,
                    Address = CustAddressEdit.Text,
                    City = CustCityEdit.Text,
                    Region = CustRegionEdit.Text,
                    PostalCode = CustPostalEdit.Text,
                    Country = CustCountryEdit.Text,
                    Phone = CustPhoneEdit.Text,
                    Fax = CustFaxEdit.Text
                };
                if (context.Customers.Any(x => x.CustomerID == customer.CustomerID))
                {
                    var toUpdate = context.Customers.Find(customer.CustomerID);
                    toUpdate.CustomerID = customer.CustomerID;
                    toUpdate.CompanyName = customer.CompanyName;
                    toUpdate.ContactName = customer.ContactName;
                    toUpdate.ContactTitle = customer.ContactTitle;
                    toUpdate.Address = customer.Address;
                    toUpdate.City = customer.City;
                    toUpdate.Region = customer.Region;
                    toUpdate.PostalCode = customer.PostalCode;
                    toUpdate.Country = customer.Country;
                    toUpdate.Phone = customer.Phone;
                    toUpdate.Fax = customer.Fax;
                    context.SaveChanges();
                    customerViewSource.View.Refresh();
                }
                else
                {
                    context.Customers.Add(customer);
                    context.SaveChanges();
                    customerViewSource.View.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid values: " + ex.ToString());
            }
        }
        private void Delete_Customer_Query(object sender, RoutedEventArgs e)
        {
            var Result = MessageBox.Show($"Are you sure? \nThis action will delete the customer assigned to {CustIDEdit.Text} and all orders placed by this customer", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (Result == MessageBoxResult.Yes)
            {
                try
                {

                    Customer customer = new Customer()
                    {
                        CustomerID = CustIDEdit.Text
                    };
                        var toRemove = context.Customers.Find(customer.CustomerID);

                    if (context.Orders.Any(x => x.CustomerID == customer.CustomerID))
                    {
                        var toRemoveOrders = context.Orders.Select(x => x).Where(x => x.CustomerID == customer.CustomerID).ToList();
                        List<Order_Detail> toRemoveDetails = new List<Order_Detail>();
                        if (toRemoveOrders != null)
                        {
                            foreach (var item in toRemoveOrders)
                            {
                                toRemoveDetails.Add(context.Order_Details.Select(x => x).Where(x => x.OrderID == item.OrderID).FirstOrDefault());
                            }
                            foreach (var item in toRemoveDetails)
                            {
                                if (item != null)
                                { context.Order_Details.Remove(item); }
                            }
                            context.SaveChanges();
                            foreach (var item in toRemoveOrders)
                            {
                                if (item != null)
                                { context.Orders.Remove(item); } 
                            }
                            context.SaveChanges();

                        }
                        if (toRemove != null)
                        {
                            context.Customers.Remove(toRemove);
                            context.SaveChanges();
                        }
                    }
                    customerViewSource.View.Refresh();
                    productViewSource3.View.Refresh();
                    customerViewSource.View.Refresh();
                    orderViewSource.View.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid ID: " + ex.Message);

                }
            }
        }
        #endregion
        #region order queries
        

        private void AddToCart(object sender, RoutedEventArgs e)
        {
            cartListView.DataContext = cartItems;
            if (NewOrderProduct.SelectedValue != null && NewOrderQty.Text.Length != 0)
            {
                var product = NewOrderProduct.SelectedItem as Product;
                short qty = short.Parse(NewOrderQty.Text);
                if(cartItems.Any(x => x.Key == product))
                {
                    short temp = cartItems.Where(x => x.Key == product).Select(x => x.Value).SingleOrDefault();
                    temp += qty;
                    var toRemove = cartItems.Where(x => x.Key == product).SingleOrDefault();
                    cartItems.Remove(toRemove);
                    cartItems.Add(new KeyValuePair<Product, short>(product, temp));
                }
                else if(qty !=0 )
                { 
                    cartItems.Add(new KeyValuePair<Product, short>(product, qty));
                }
            }
        }
        private void ClearCart(object sender, RoutedEventArgs e)
        { 
            cartItems.Clear();
        }
        private void RemoveFromCart(object sender, RoutedEventArgs e) 
        {
            if( cartListView.SelectedItem is KeyValuePair<Product, short> kvp && cartListView.SelectedItem != null)
            {
                cartItems.Remove(kvp);
            }

        }

        private void PlaceOrder(object sender, RoutedEventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Cart cannot be empty");
                return;
            }
            if (NewOrderCustomer.SelectedItem == null || NewOrderEmp.SelectedItem == null || NewOrderShipper.SelectedItem == null) 
            {
                MessageBox.Show("Fill in all the values first");
                return;
            }
            try
            {
                Customer customer = NewOrderCustomer.SelectedItem as Customer;
                Employee employee = NewOrderEmp.SelectedItem as Employee; 
                Shipper shipper = NewOrderShipper.SelectedItem as Shipper;
                decimal freight = 0;
                decimal.TryParse(NewOrderFreight.Text, out freight);
                float discount = 0;
                float.TryParse(NewOrderDiscount.Text, out discount);
                foreach(var prod in cartItems)
                {
                    if(context.Products.Find(prod.Key.ProductID).UnitsInStock < prod.Value)
                    {
                        throw new Exception($"Can't place an order for more items than are available in stock!\n{context.Products.Find(prod.Key.ProductID).UnitsInStock} units of {context.Products.Find(prod.Key.ProductID).ProductName} are available.");
                    }
                    context.Products.Find(prod.Key.ProductID).UnitsInStock -= prod.Value;
                    context.SaveChanges();
                    productViewSource3.View.Refresh();
                }

                Order order = new Order()
                {
                    Customer = customer,
                    CustomerID = customer.CustomerID,
                    Employee = employee,
                    EmployeeID = employee.EmployeeID,
                    Freight = freight,
                    OrderDate = DateTime.Now,
                    RequiredDate = NewOrderRequired.SelectedDate,
                    ShipAddress = NewOrderAddress.Text,
                    ShipCity = NewOrderCity.Text,
                    ShipRegion = NewOrderRegion.Text,
                    ShipCountry = NewOrderCountry.Text,
                    ShipName = NewOrderName.Text,
                    ShipPostalCode = NewOrderPostal.Text,
                    ShipVia = shipper.ShipperID
                };
                List<Order_Detail> details = new List<Order_Detail>();
                foreach (var item in cartItems) 
                {
                    var detail = new Order_Detail();
                    detail.Discount = discount;
                    detail.Order = order;
                        
                        detail.Product = item.Key;
                        detail.ProductID = item.Key.ProductID;
                        detail.Quantity = item.Value;
                        detail.UnitPrice = (decimal)item.Key.UnitPrice;
                    details.Add(detail);
                }
                context.Orders.Add(order);
                context.SaveChanges();

                foreach (var det in details)
                {
                    det.OrderID = order.OrderID;
                    context.Order_Details.Add(det);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); 
                return;

            }

        }
        private void CustomerAddressFill(object sender, SelectionChangedEventArgs e)
        {
            if(sender is  ComboBox box)
            {
                var customer = box.SelectedItem as Customer;
                if (customer != null)
                {
                    NewOrderAddress.Text = customer.Address;
                    NewOrderCity.Text = customer.City;
                    NewOrderName.Text = customer.ContactName;
                    NewOrderCountry.Text = customer.Country;
                    NewOrderPostal.Text = customer.PostalCode;
                    NewOrderRegion.Text = customer.Region;
                }

            }
        }
        #endregion
        #region employee queries
        private void EmployeeEdit(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is DataGridRow row && row.Item != null)
                {
                    var employee = row.Item as Employee;
                    var img = new ImageSourceConverter();
                    try
                    {
                        image.Source = img.ConvertFrom(employee.Photo) as ImageSource;
                    }
                    catch 
                    {
                        image.Source = null;
                    }
                    EmpEditID.Text = employee.EmployeeID.ToString();
                    EmpEditName.Text = employee.FirstName;
                    EmpEditSurname.Text = employee.LastName;
                    EmpEditTOC.Text = employee.TitleOfCourtesy;
                    EmpEditTitle.Text = employee.Title;
                    EmpEditAddress.Text = employee.Address;
                    EmpEditRegion.Text = employee.Region;
                    EmpEditCity.Text = employee.City;
                    EmpEditCountry.Text = employee.Country;
                    EmpEditPostal.Text = employee.PostalCode;
                    EmpEditPhone.Text = employee.HomePhone;
                    EmpEditExt.Text = employee.Extension;
                }
            }
            catch (Exception ex)
            { 
            
            }
        }

        

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Photo";
            dialog.DefaultExt = ".jpg";
            dialog.Filter = "Photos (.jpg)|*.jpg";
            bool? result = dialog.ShowDialog();
            if(result == true)
            {
                string filename = dialog.FileName;
                var imgfs = File.OpenRead(filename);
                int length = Convert.ToInt32(imgfs.Length);
                data = new byte[length];
                imgfs.Read(data, 0, length);
                imgfs.Close();
                var img = new ImageSourceConverter();
                image.Source = img.ConvertFrom(data) as ImageSource;
            }
        }

        private void Run_Emp_Query(object sender, RoutedEventArgs e)
        {
            try
            {
                var employee = new Employee()
                {
                    EmployeeID = int.Parse(EmpEditID.Text),
                    FirstName = EmpEditName.Text,
                    LastName = EmpEditSurname.Text,
                    TitleOfCourtesy = EmpEditTOC.Text,
                    Title = EmpEditTitle.Text,
                    Address = EmpEditAddress.Text,
                    Region = EmpEditRegion.Text,
                    City = EmpEditCity.Text,
                    Country = EmpEditCountry.Text,
                    PostalCode = EmpEditPostal.Text,
                    HomePhone = EmpEditPhone.Text,
                    Extension = EmpEditExt.Text,
                    Photo = data
                };
                if (context.Employees.Any(x => x.EmployeeID == employee.EmployeeID))
                {
                    var toUpdate = context.Employees.Find(employee.EmployeeID);
                    toUpdate.FirstName = employee.FirstName;
                    toUpdate.LastName = employee.LastName;
                    toUpdate.TitleOfCourtesy = employee.TitleOfCourtesy;
                    toUpdate.Title = employee.Title;
                    toUpdate.Address = employee.Address;
                    toUpdate.Region = employee.Region;
                    toUpdate.City = employee.City;
                    toUpdate.Country = employee.Country;
                    toUpdate.PostalCode = employee.PostalCode;
                    toUpdate.HomePhone = employee.HomePhone;
                    toUpdate.Extension = employee.Extension;
                    if(data.Length != 0) 
                    {

                        toUpdate.Photo = employee.Photo;
                    }
                    
                    context.SaveChanges();
                    
                }
                else
                {
                    context.Employees.Add(employee);
                    context.SaveChanges();

                }
                employeeViewSource.View.Refresh();
                data = new byte[0];
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Invalid values: " + ex.Message);
            }
        }
        private void Delete_Emp_Query(object sender, RoutedEventArgs e)
        {
            var Result = MessageBox.Show($"Are you sure? \nThis action will delete the employee assigned to {EmpEditID.Text} and all orders created by this employee", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (Result == MessageBoxResult.Yes)
            {
                try
                {

                        Employee employee = new Employee()
                        {
                            EmployeeID = int.Parse(EmpEditID.Text)
                        };
                    var toRemove = context.Employees.Find(employee.EmployeeID);

                    if (context.Orders.Any(x => x.EmployeeID == employee.EmployeeID))
                    {
                        var toRemoveOrders = context.Orders.Select(x => x).Where(x => x.EmployeeID == employee.EmployeeID).ToList();
                        List<Order_Detail> toRemoveDetails = new List<Order_Detail>();
                        if (toRemoveOrders != null)
                        {
                            foreach (var item in toRemoveOrders)
                            {
                                toRemoveDetails.Add(context.Order_Details.Select(x => x).Where(x => x.OrderID == item.OrderID).FirstOrDefault());
                            }
                            foreach (var item in toRemoveDetails)
                            {
                                if (item != null)
                                { context.Order_Details.Remove(item); }
                            }
                            context.SaveChanges();
                            foreach (var item in toRemoveOrders)
                            {
                                if (item != null)
                                { context.Orders.Remove(item); }
                            }
                            context.SaveChanges();

                        }
                        if (toRemove != null)
                        {
                            context.Employees.Remove(toRemove);
                            context.SaveChanges();
                        }
                    }
                    employeeViewSource.View.Refresh();
                    productViewSource3.View.Refresh();
                    orderViewSource.View.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid ID: " + ex.Message);
                }
            }
        }
        #endregion
        #region shipper queries


        #endregion
    }
}
