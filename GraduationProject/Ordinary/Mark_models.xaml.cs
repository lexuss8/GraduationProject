using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace GraduationProject.Ordinary
{
    /// <summary>
    /// Логика взаимодействия для Mark_models.xaml
    /// </summary>
    public partial class Mark_models : Window
    {
        Npgsql.NpgsqlConnection con = new NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        Npgsql.NpgsqlCommand cmd = new NpgsqlCommand();
        public Mark_models()
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
            List<CarModelView> cars = new List<CarModelView>();
            try
            {
                await con.OpenAsync();
                string sql = @"SELECT m.mark AS mark, mm.model, mm.category, mm.body, mm.photo FROM Mark_models mm
                            JOIN Marks m ON m.id = mm.mark_id;";
                using (var cmd = new NpgsqlCommand(sql, con))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string photoPath = reader["photo"].ToString();
                        BitmapImage image = null;
                        if (!string.IsNullOrEmpty(photoPath))
                        {
                            string fullPath = $"pack://application:,,,/Images/Cars/{photoPath}";
                            image = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                        }
                        cars.Add(new CarModelView
                        {
                            Марка = reader["mark"].ToString(),
                            Модель = reader["model"].ToString(),
                            Категория = reader["category"].ToString(),
                            Кузов = reader["body"].ToString(),
                            Фото = image
                        });
                    }
                }
                data_grid_mark_models.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_mark_models.Columns[0].Header = new TextBlock { Text = "Марка", FontWeight = FontWeights.Bold };

                data_grid_mark_models.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_mark_models.Columns[1].Header = new TextBlock { Text = "Модель", FontWeight = FontWeights.Bold };

                data_grid_mark_models.Columns[2].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_mark_models.Columns[2].Header = new TextBlock { Text = "Категория", FontWeight = FontWeights.Bold };

                data_grid_mark_models.Columns[3].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_mark_models.Columns[3].Header = new TextBlock { Text = "Кузов", FontWeight = FontWeights.Bold };

                data_grid_mark_models.Columns[4].Width = new DataGridLength(2, DataGridLengthUnitType.Star);
                data_grid_mark_models.Columns[4].Header = new TextBlock { Text = "Фото", FontWeight = FontWeights.Bold };
                data_grid_mark_models.ItemsSource = cars;
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
        private void Image_click (object sender, EventArgs e)
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