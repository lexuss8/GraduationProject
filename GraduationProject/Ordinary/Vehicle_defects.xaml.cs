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

namespace GraduationProject.Ordinary
{
    /// <summary>
    /// Логика взаимодействия для Vehicle_defects.xaml
    /// </summary>
    public partial class Vehicle_defects : Window
    {
        Npgsql.NpgsqlConnection con = new NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        Npgsql.NpgsqlCommand cmd = new NpgsqlCommand();
        public Vehicle_defects()
        {
            InitializeComponent();
            LoadData();
            try
            {
                cmd.Connection = con;
            }
            catch (Npgsql.NpgsqlException ex)
            {
                MessageBox.Show("Ошибка подключения PostgreSQL" + ex);
            }
        }
        private async void LoadData()
        {
            List<Vehicle_defects_View> List_vehicle_defects = new List<Vehicle_defects_View>();
            try
            {
                await con.OpenAsync();
                string sql = @"SELECT defect_name, description, legal_basis, photo FROM Vehicle_defects;";
                using (var cmd = new NpgsqlCommand(sql, con))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string photoPath = reader["photo"].ToString();
                        BitmapImage image = null;
                        if (!string.IsNullOrEmpty(photoPath))
                        {
                            string fullPath = $"pack://application:,,,/Images/Vehicle_defects/{photoPath}";
                            image = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                        }
                        List_vehicle_defects.Add(new Vehicle_defects_View
                        {
                            Название = reader["defect_name"].ToString(),
                            Описание = reader["description"].ToString(),
                            Правовая_база = reader["legal_basis"].ToString(),
                            Фото = image
                        });
                    }
                }
                data_grid_vehicle_defects.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_vehicle_defects.Columns[0].Header = new TextBlock { Text = "Название", FontWeight = FontWeights.Bold };

                data_grid_vehicle_defects.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_vehicle_defects.Columns[1].Header = new TextBlock { Text = "Описание", FontWeight = FontWeights.Bold };

                data_grid_vehicle_defects.Columns[2].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_vehicle_defects.Columns[2].Header = new TextBlock { Text = "Правовая база", FontWeight = FontWeights.Bold };

                data_grid_vehicle_defects.Columns[3].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_vehicle_defects.Columns[3].Header = new TextBlock { Text = "Фото", FontWeight = FontWeights.Bold };
                data_grid_vehicle_defects.ItemsSource = List_vehicle_defects;
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
            Ordinary_menu ordinary_menu = new Ordinary_menu();
            ordinary_menu.Show();
            this.Close();
        }
    }
}