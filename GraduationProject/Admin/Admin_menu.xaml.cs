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
using GraduationProject.Admin;
using Npgsql;

namespace GraduationProject
{
    /// <summary>
    /// Логика взаимодействия для Admin_menu.xaml
    /// </summary>
    public partial class Admin_menu : Window
    {
        NpgsqlConnection con = new Npgsql.NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        NpgsqlCommand cmd = new NpgsqlCommand();
        public Admin_menu()
        {
            InitializeComponent();
            try
            {
                con = new NpgsqlConnection();
            }
            catch (PostgresException ex)
            {
                MessageBox.Show(ex.Message);
            }
            tb_info.Text = "Администратор: " + Session.Login;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main_window = new MainWindow();
            main_window.Show();
            this.Close();
        }
        private void Button_Click_Users(object sender, RoutedEventArgs e)
        {
            Management_users management_users = new Management_users();
            management_users.Show();
            this.Close();
        }
        private void Button_Click_Employees(object sender, RoutedEventArgs e)
        {
            Employees employees = new Employees();
            employees.Show();
            this.Close();
        }
        private void Button_Click_Sessions(object sender, RoutedEventArgs e)
        {
            User_sessions user_sessions = new User_sessions();
            user_sessions.Show();
            this.Close();
        }
    }
}