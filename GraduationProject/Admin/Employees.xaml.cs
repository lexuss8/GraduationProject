using GraduationProject.Ordinary;
using Npgsql;
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

namespace GraduationProject.Admin
{
    /// <summary>
    /// Логика взаимодействия для Employees.xaml
    /// </summary>
    public partial class Employees : Window
    {
        Npgsql.NpgsqlConnection con = new NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        Npgsql.NpgsqlCommand cmd = new NpgsqlCommand();
        public Employees()
        {
            InitializeComponent();
            try
            {
                cmd.Connection = con;
            }
            catch (PostgresException ex)
            {
                MessageBox.Show(ex.Message);
            }
            LoadData();
        }
        private async void LoadData()
        {
            List<EmployeesView> List_employees = new List<EmployeesView>();
            try
            {
                await con.OpenAsync();
                string sql = @"SELECT p.last_name, p.first_name, p.middle_name, e.rank, d.name AS division_name, p.photo
                               FROM Employees e JOIN people p ON e.people_id = p.id
                               JOIN Divisions d ON e.division_id = d.id;";
                using (var cmd = new NpgsqlCommand(sql, con))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string photoPath = reader["photo"].ToString();
                            BitmapImage image = null;
                            if (!string.IsNullOrEmpty(photoPath))
                            {
                                string fullPath = $"pack://application:,,,/Images/People/{photoPath}";
                                image = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                            }
                            List_employees.Add(new EmployeesView
                            {
                                Фамилия = reader["last_name"].ToString(),
                                Имя = reader["first_name"].ToString(),
                                Отчество = reader["middle_name"].ToString(),
                                Звание = reader["rank"].ToString(),
                                Отдел = reader["division_name"].ToString(),
                                Фото = image
                            });
                        }
                    }
                data_grid_employees.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[0].Header = new TextBlock { Text = "Фамилия", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[1].Header = new TextBlock { Text = "Имя", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[2].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[2].Header = new TextBlock { Text = "Отчество", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[3].Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[3].Header = new TextBlock { Text = "Звание", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[4].Width = new DataGridLength(2, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[4].Header = new TextBlock { Text = "Отдел", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[5].Width = new DataGridLength(0.6, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[5].Header = new TextBlock { Text = "Фото", FontWeight = FontWeights.Bold };
                data_grid_employees.ItemsSource = List_employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
            finally
            {
                await con.CloseAsync();
            }
        }
            private void Image_click(object sender, EventArgs e)
            {
                if (sender is Image img && img.Source != null)
                {
                    image_big.Visibility = Visibility.Visible;
                    image_big.Source = img.Source;
                }
            }
            private void Big_image_click(object sender, EventArgs e)
            {
                image_big.Visibility = Visibility.Hidden;
            }
            private void Button_Click_back(object sender, RoutedEventArgs e)
            {
                Admin_menu admin_menu = new Admin_menu();
                admin_menu.Show();
                this.Close();
            }
        }
    }

