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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yastrebov41
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    /// loginDEftn2018
    /// gPq+a}
    public partial class AuthPage : Page
    {
        public int AuthFailscount = 0;
        public string Capchacharacters = "";
        public AuthPage()
        {
            InitializeComponent();
        }
        private void EnterAsGuestBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProductPage(null));
            Login.Text = "";
            Password.Text = "";
            CapchaStackPanel.Visibility = Visibility.Hidden;
            CapchaTextBox.Visibility = Visibility.Hidden;
        }
        private async void EnterAsUser_Click(object sender, RoutedEventArgs e)
        {
            if (Login.Text == "" || Password.Text == "")
            {
                MessageBox.Show("Логин или пароль не введены");
            }
            else
            {
                if (AuthFailscount > 0 && CapchaTextBox.Text == "")
                {
                    MessageBox.Show("Введите капчу");
                    CapchaStackPanel.Visibility = Visibility.Visible;
                    CapchaTextBox.Visibility = Visibility.Visible;
                }
                else
                {
                    User currentUser = Yastrebov41Entities.GetContext().User.ToList().Find(p => p.UserLogin == Login.Text && p.UserPassword == Password.Text);
                    if (currentUser != null && (AuthFailscount == 0 || Capchacharacters == CapchaTextBox.Text))
                    {

                        Manager.MainFrame.Navigate(new ProductPage(currentUser));
                        AuthFailscount = 0;
                        Login.Text = "";
                        Password.Text = "";
                        Capchacharacters = "";
                        CapchaCharOne.Text = "";
                        CapchaCharTwo.Text = "";
                        CapchaCharTree.Text = "";
                        CapchaCharFour.Text = "";
                        CapchaTextBox.Text = "";
                        CapchaStackPanel.Visibility = Visibility.Hidden;
                        CapchaTextBox.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        if (AuthFailscount > 0)
                            MessageBox.Show("Логин или пароль или капча введены неверно");
                        else
                            MessageBox.Show("Логин или пароль введены неверно");
                        CapchaStackPanel.Visibility = Visibility.Visible;
                        CapchaTextBox.Visibility = Visibility.Visible;
                        Random random = new Random();
                        string symbols = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
                        Capchacharacters = (symbols[random.Next(symbols.Length)].ToString() + symbols[random.Next(symbols.Length)].ToString() + symbols[random.Next(symbols.Length)].ToString() + symbols[random.Next(symbols.Length)].ToString());
                        CapchaCharOne.Text = Capchacharacters[0].ToString();
                        CapchaCharTwo.Text = Capchacharacters[1].ToString();
                        CapchaCharTree.Text = Capchacharacters[2].ToString();
                        CapchaCharFour.Text = Capchacharacters[3].ToString();
                        CapchaCharOne.Margin = new Thickness(random.Next(0, 20), random.Next(0, 20), 0, 0);
                        CapchaCharTwo.Margin = new Thickness(random.Next(0, 20), random.Next(0, 20), 0, 0);
                        CapchaCharTree.Margin = new Thickness(random.Next(0, 20), random.Next(0, 20), 0, 0);
                        CapchaCharFour.Margin = new Thickness(random.Next(0, 20), random.Next(0, 20), 0, 0);
                        AuthFailscount++;
                        if (AuthFailscount >= 2)
                        {
                            LogBtn.IsEnabled = false;
                            MessageBox.Show("Неверные данные, подождите...");
                            await Task.Delay(10000);
                            LogBtn.IsEnabled = true;
                        }
                    }
                }
            }
        }
    }
}
