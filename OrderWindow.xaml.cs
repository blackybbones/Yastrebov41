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
        List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        List<Product> selectedProducts = new List<Product>();
        private Order currentOrder = new Order();
        private OrderProduct currentOrderProduct = new OrderProduct();
        private User _currentUser;

        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, User user)
        {
            InitializeComponent();
            _currentUser = user;
            if (user != null)
                ClientTB.Text = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;
            else
                ClientTB.Text = "Гость";

            var currentPickups = Yastrebov41Entities.GetContext().PickUpPoint.ToList().Select(p => p.PickUpPointFull).ToList();
            PickPointComboBox.ItemsSource = currentPickups;

            int currentID = selectedOrderProducts.First().OrderID; //определение номера текущего заказа
            currentOrder.OrderID = currentID;
            TBOrderID.Text = currentID.ToString();

            int nextOrderID = GetNextOrderID();
            TBOrderID.Text = nextOrderID.ToString();


            foreach (Product product in selectedProducts)
            {
                var orderProduct = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);
                if (orderProduct != null)
                {
                    product.Quantity = orderProduct.ProductCount;
                }
                else
                {
                    product.Quantity = 1; // Если не найден, устанавливаем 1 (опционально)
                }
            }

            ProductListView.ItemsSource = selectedProducts;

            this.selectedOrderProducts = selectedOrderProducts;
            this.selectedProducts = selectedProducts;
            OrderDP.Text = DateTime.Now.ToString();
            SetDeliveryDate();
            CalculateTotalAndDiscount();
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;
            var selectedOP = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);

            if (selectedOP != null)
            {
                selectedOP.ProductCount++;
                prod.Quantity = selectedOP.ProductCount;
                SetDeliveryDate();
                CalculateTotalAndDiscount();
                ProductListView.Items.Refresh();
            }
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e) //удаление есои накликал ноль, невозм кликать в минус
        {
            var prod = (sender as Button).DataContext as Product;
            var selectedOP = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);

            if (selectedOP != null)
            {
                if (selectedOP.ProductCount > 1)
                {
                    selectedOP.ProductCount--;
                    prod.Quantity = selectedOP.ProductCount; // Синхронизируем Quantity
                    SetDeliveryDate();
                    CalculateTotalAndDiscount();
                    ProductListView.Items.Refresh();
                }
                else
                {
                    // Удаляем OrderProduct из списка
                    selectedOrderProducts.Remove(selectedOP);

                    // Находим Product в selectedProducts по артикулу (чтобы избежать проблем с ссылками)
                    var productToRemove = selectedProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);
                    if (productToRemove != null)
                    {
                        selectedProducts.Remove(productToRemove);
                    }

                    // Обновляем интерфейс
                    ProductListView.Items.Refresh();
                    // Перепривязываем данные, чтобы обновить интерфейс
                    ProductListView.ItemsSource = null;
                    ProductListView.ItemsSource = selectedProducts;
                    SetDeliveryDate();
                    CalculateTotalAndDiscount();
                    ProductListView.Items.Refresh();

                    if (selectedProducts.Count == 0)
                    {
                        Manager.OrderBtn.Visibility = Visibility.Hidden;
                        this.Close();
                    }
                }
            }

            //MessageBox.Show(prod.ProductArticleNumber.ToString() + " " + prod.Quantity);
        }

        //private void DateFormOrder_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    SetDeliveryDate();
        //}

        private int GenerateUniqueOrderCode()
        {
            var context = Yastrebov41Entities.GetContext();
            Random random = new Random();
            int code;
            do
            {
                code = random.Next(100, 1000);
            } while (context.Order.Any(o => o.OrderCode == code));

            return code;
        }

        private int GetNextOrderID()
        {
            var sqlCommand = "SELECT IDENT_CURRENT('Order')";
            var nextID = Yastrebov41Entities.GetContext().Database.SqlQuery<decimal>(sqlCommand).FirstOrDefault() + 1;
            return (int)nextID;
        }

        private void SetDeliveryDate()
        {
            // Проверяем, достаточно ли товара на складе: (количество в заказе + 3) <= количество на складе?
            // Если на складе меньше, чем (количество в заказе + 3), то срок 6 дней.

            bool DeliveryStatus = false;
            foreach (var p in selectedProducts)
            {
                if (p.ProductQuantityInStock < (p.Quantity + 3))
                {
                    DeliveryStatus = true;
                }
            }

            DateTime deliveryDate = OrderDP.SelectedDate.Value;
            deliveryDate = DeliveryStatus
                ? deliveryDate.AddDays(6) // Если есть товары <3 шт. → +6 дней
                : deliveryDate.AddDays(3); // В противном случае → +3 дня
            OrderDD.SelectedDate = deliveryDate;

        }


        private void CalculateTotalAndDiscount()
        {
            decimal total = 0;
            decimal discount = 0;

            foreach (var orderProduct in selectedOrderProducts)
            {
                var product = selectedProducts.FirstOrDefault(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber);
                if (product == null) continue;

                decimal price = product.ProductCost;
                decimal discountPercent = product.ProductDiscountAmount ?? 0;

                // Сумма без скидки
                total += orderProduct.ProductCount * price;

                // Сумма скидки для текущего товара
                discount += orderProduct.ProductCount * price * (discountPercent / 100);
            }

            // Итоговая сумма с учётом скидки
            decimal discountedTotal = total - discount;

            // Обновление отображения
            TotalAmountTB.Text = total.ToString("N0") + " ₽ ";
            TotalDiscountAmountTB.Text = discount.ToString("N0") + " ₽ ";
            DiscountAmountTB.Text = discountedTotal.ToString("N0") + " ₽";
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            if (PickPointComboBox.SelectedIndex == -1)
                errors.AppendLine("Выберите пункт выдачи!");
            if (OrderDP.SelectedDate.Value == null)
                errors.AppendLine("Введите дату формирования заказа!");
            if (OrderDD.SelectedDate.Value == null)
                errors.AppendLine("Введите дату доставки заказа!");
            if (selectedOrderProducts.Count == 0) // Новая проверка
                errors.AppendLine("Добавьте хотя бы один товар в заказ!");
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }
            if (_currentUser == null)
            {
                currentOrder.OrderClient = null;
            }
            else
            {
                currentOrder.OrderClient = _currentUser.UserID;
            }

            currentOrder.OrderPickupPoint = PickPointComboBox.SelectedIndex + 1;
            currentOrder.OrderDate = OrderDP.SelectedDate.Value;
            currentOrder.OrderDeliveryDate = OrderDD.SelectedDate.Value;
            currentOrder.OrderStatus = "Новый";

            currentOrder.OrderCode = GenerateUniqueOrderCode();

            Yastrebov41Entities.GetContext().Order.Add(currentOrder);

            Yastrebov41Entities.GetContext().SaveChanges();
            TBOrderID.Text = currentOrder.OrderID.ToString();

            foreach (var op in selectedOrderProducts)
            {
                op.OrderID = currentOrder.OrderID;
                Yastrebov41Entities.GetContext().OrderProduct.Add(op);
            }
            Yastrebov41Entities.GetContext().SaveChanges();
            MessageBox.Show($"Заказ №{currentOrder.OrderID} сохранен! Код: {currentOrder.OrderCode}");
            this.DialogResult = true;

            
            Close();
        }
    }
}
