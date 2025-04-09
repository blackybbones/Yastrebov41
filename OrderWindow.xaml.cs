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

namespace Yastrebov41
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private List<Product> choiceProducts = null;
        private User Client = null;
        private int countDay = 0;
        public OrderWindow(User user, List<Product> selectedProduct)
        {
            InitializeComponent();
            if (user == null)
            {

            }
            else
            {
                InputName.Text = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;
                InputName.IsReadOnly = true;
                Client = user;

            }
            choiceProducts = selectedProduct;
            var orders = Yastrebov41Entities.GetContext().Order.ToList();
            InputNumberOrder.Text = (orders.Count + 1).ToString();
            InputDateOrder.Text = DateTime.Now.ToShortDateString();
            List<PickUpPoint> pickPup = Yastrebov41Entities.GetContext().PickUpPoint.ToList();
            List<string> listPickUp = new List<string>();
            foreach (var item in pickPup)
            {
                listPickUp.Add(item.PickUpPointIndex + " " + item.PickUpPointCity + " " + item.PickUpPointStreet + " " + item.PickUpPointHome);
            }



            Combotype.ItemsSource = listPickUp;

            duplicateSort();
            ProductListView.ItemsSource = choiceProducts;
            UpdateContent();
        }


        private void UpdateContent()
        {
            double generalSum = 0;
            var articles = Yastrebov41Entities.GetContext().OrderProduct.ToList();
            bool isTerm = false;
            if (choiceProducts != null)
            {
                foreach (var item in choiceProducts)
                {

                    double discount = (double)item.ProductDiscountAmount / 100;
                    double itogsum = Convert.ToInt32(item.ProductCost) - ((double)item.ProductCost * discount);
                    generalSum += itogsum * item.Count;
                    if (item.Count > item.ProductQuantityInStock - 3)
                       isTerm = true;

                }
            }

            SumOrder.Text = generalSum.ToString() + " с учетом всех скидок";
            if (choiceProducts != null)
                TermDilivery.Text = isTerm ? "6" : choiceProducts.Count == 0 ? "Выберете товар" : "3";
            else
                TermDilivery.Text = "Выберете товар";
            if (TermDilivery.Text.Length == 1)
                countDay = Convert.ToInt32(TermDilivery.Text);
            else
                countDay = 0;
            InputDateDivivery.Text = DateTime.Now.AddDays(countDay).ToShortDateString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string errors = "";
            if (choiceProducts.Count == 0)
                errors += "Выберете хотя бы один товар!\n";
            if (InputName.Text == "")
                errors += "Введите свое ФИО\n";
            if (Combotype.SelectedIndex == -1)
                errors += "Выберете пункт выдачи\n";
            if (errors.Length > 0)
            {
                MessageBox.Show(errors);
                return;
            }
            var orders = Yastrebov41Entities.GetContext().Order.ToList();

            Order newOrder = new Order();
            newOrder.OrderID = Convert.ToInt32(InputNumberOrder.Text);
            newOrder.OrderStatus = "Новый";
            newOrder.OrderDate = DateTime.Now;
            newOrder.OrderPickupPoint = Combotype.SelectedIndex + 1;
            newOrder.OrderCode = 911 + orders.Count;
            if (Client != null)
                newOrder.OrderClient = Client.UserID;
            newOrder.OrderDeliveryDate = DateTime.Now.AddDays(countDay);
            newOrder.User = null;


            List<OrderProduct> orderProductList = new List<OrderProduct>();
            foreach (Product item in ProductListView.ItemsSource)
            {
                OrderProduct orderProduct = new OrderProduct();
                orderProduct.OrderID = newOrder.OrderID;
                orderProduct.ProductCount = item.Count;
                orderProduct.ProductArticleNumber = item.ProductArticleNumber;
                orderProductList.Add(orderProduct);
            }

            foreach (var item in orderProductList)
            {
                Yastrebov41Entities.GetContext().OrderProduct.Add(item);
            }

            Yastrebov41Entities.GetContext().Order.Add(newOrder);
            Yastrebov41Entities.GetContext().SaveChanges();
            choiceProducts = null;
            UpdateContent();
            ProductListView.ItemsSource = null;
            MessageBox.Show("Заказ оформлен успешно, ваш код: " + newOrder.OrderCode);
            this.Close();
            return;



        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button.DataContext as Product;
            if (product.Count > 1)
            {
                product.Count--;
                ProductListView.ItemsSource = null;
                ProductListView.ItemsSource = choiceProducts;
            }
            else
            {
                choiceProducts.Remove(product);
                ProductListView.ItemsSource = null;
                ProductListView.ItemsSource = choiceProducts;
            }
            UpdateContent();
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button.DataContext as Product;
            product.Count++;
            ProductListView.ItemsSource = null;
            ProductListView.ItemsSource = choiceProducts;
            UpdateContent();
        }


        private void duplicateSort()
        {


            for (int i = 0; i < choiceProducts.Count; i++)
            {
                for (int j = 0; j < choiceProducts.Count; j++)
                {
                    if (i != j && choiceProducts[i].ProductArticleNumber == choiceProducts[j].ProductArticleNumber)
                    {
                        try
                        {
                            choiceProducts.Remove(choiceProducts[j]);
                            choiceProducts[i].Count += 2;
                        }
                        catch (Exception ex)
                        {
                            return;
                        }

                    }
                }
            }

        }
    }
}
