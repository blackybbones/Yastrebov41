using System;
using System.Collections.Generic;
using System.Data;
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

namespace Yastrebov41
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        List<Product> CurrentPageList = new List<Product>();
        List<Product> TableList;

        int CountPage;
        int CurrentCountPage;
        private User _user;
        public ProductPage(User user)
        {

            string login;
            string role;
            InitializeComponent();
            var currentProducts = Yastrebov41Entities.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentProducts;

            if (user != null)
            {
                login = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;
                shw.Text = login;
                switch (user.UserRole)
                {
                    case 1:
                        role = "Клиент";
                        break;
                    case 2:
                        role = "Менеджер";
                        break;
                    case 3:
                        role = "Администратор";
                        break;
                    default:
                        role = "Гость";
                        break;

                }
            }
            else
            {
                role = "Гость";
            }

            rolee.Text = role;


            ComboBoxFilter.SelectedIndex = 0;
            CurrentCountPage = TableList.Count;
            EveryPages.Text = CurrentCountPage.ToString();
            UpdateProduct();

        }

        private void UpdateProduct()
        {
            var currentProducts = Yastrebov41Entities.GetContext().Product.ToList();

            if (ComboBoxFilter.SelectedIndex == 1)
            {
                currentProducts = currentProducts.Where(p => p.ProductDiscountAmount <= 9.99).ToList();
            }
            if (ComboBoxFilter.SelectedIndex == 2)
            {
                currentProducts = currentProducts.Where(p => p.ProductDiscountAmount > 10 && p.ProductDiscountAmount < 14.99).ToList();
            }
            if (ComboBoxFilter.SelectedIndex == 3)
            {
                currentProducts = currentProducts.Where(p => p.ProductDiscountAmount >= 15).ToList();
            }



            if (RButtonBiggist.IsChecked.Value)
            {
                currentProducts = currentProducts.OrderByDescending(p => p.ProductCost).ToList();
            }

            if (RbutttonSmallist.IsChecked.Value)
            {
                currentProducts = currentProducts.OrderBy(p => p.ProductCost).ToList();
            }

            currentProducts = currentProducts.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();


            ProductListView.ItemsSource = currentProducts.ToList();
            ProductListView.ItemsSource = currentProducts;
            TableList = currentProducts;
            ChangeText();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProduct();
        }

        private void RbutttonSmallist_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }

        private void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProduct();
        }

        private void ChangeText()
        {
            CurrentPageList.Clear();
            CountPage = TableList.Count;
            currentPages.Text = CountPage.ToString();
        }

        private void RButtonBiggist_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var currentChoiceProduct = ProductListView.SelectedItems.Cast<Product>().ToList();
            for (int i = 0; i < currentChoiceProduct.Count; i++)
            {
                CurrentPageList.Add(currentChoiceProduct[i]);
            }
            BasketButton.Visibility = Visibility.Visible;

        }

        private void BasketButton_Click(object sender, RoutedEventArgs e)
        {
            OrderWindow orderWindow = new OrderWindow(_user, CurrentPageList);

            orderWindow.Show();
            CurrentPageList = new List<Product>();
            BasketButton.Visibility = Visibility.Hidden;
        }
    }
}
