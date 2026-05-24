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
    /// Логика взаимодействия для Driver_certificares.xaml
    /// </summary>
    public partial class Driver_certificates : Window
    {
        Npgsql.NpgsqlConnection con = new NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        Npgsql.NpgsqlCommand cmd = new NpgsqlCommand();
        string tb_search_standart_text = "🔍 Поиск";
        public Driver_certificates()
        {
            InitializeComponent();
            try
            {
                cmd.Connection = con;
            }
            catch (Npgsql.NpgsqlException ex)
            {
                MessageBox.Show("Ошибка подключения PostgreSQL" + ex);
            }
            LoadData();
        }
        private async void LoadData()
        {
            List<Driver_certificatesView> List_driver_certificates = new List<Driver_certificatesView>();
            try
            {
                await con.OpenAsync();
                string sql = @"SELECT d.number, p.last_name, p.first_name, p.middle_name, ds.name AS status, d.date_issue, d.date_end,
                            d.gibdd_point, d.category, d.region FROM Driver_certificates d
                            JOIN People p ON d.people_id = p.id 
                            JOIN Driver_certificates_statuses ds ON d.driver_certificate_status_id = ds.id;";
                string[] search_parts = Search.Search_text.Split(
                    new[] { ' ' });
                if (Search.Search_text != tb_search_standart_text && !string.IsNullOrWhiteSpace(Search.Search_text))
                {
                    if (search_parts.Length == 1)
                    {
                        sql += @" WHERE d.number ILIKE @p0 OR
                                p.last_name ILIKE @p0 OR
                                p.first_name ILIKE @p0 OR
                                p.middle_name ILIKE @p0 OR
                                d.gibdd_point ILIKE @p0";
                    }
                    //if (search_parts.Length == 2)
                    //{
                    //    sql += @" WHERE (last_name ILIKE @p0 AND first_name ILIKE @p1)
                    //         OR (first_name ILIKE @p0 AND last_name ILIKE @p1);";
                    //}
                }
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    if (Search.Search_text != tb_search_standart_text && !string.IsNullOrWhiteSpace(Search.Search_text) && search_parts.Length == 1)
                    {
                        cmd.Parameters.AddWithValue("p0", $"%{search_parts[0]}%");
                    }
                    if (Search.Search_text != tb_search_standart_text && !string.IsNullOrWhiteSpace(Search.Search_text) && search_parts.Length == 2)
                    {
                        cmd.Parameters.AddWithValue("p0", $"%{search_parts[0]}%");
                        cmd.Parameters.AddWithValue("p1", $"%{search_parts[1]}%");
                    }
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            List_driver_certificates.Add(new Driver_certificatesView
                            {
                                Номер = reader["number"].ToString(),
                                Фамилия = reader["last_name"].ToString(),
                                Имя = reader["first_name"].ToString(),
                                Отчество = reader["middle_name"].ToString(),
                                Статус = reader["status"].ToString(),
                                Дата_регистрации = DateTime.Parse(reader["date_issue"].ToString()).ToString("dd.MM.yyyy"),
                                Дата_окончания = DateTime.Parse(reader["date_end"].ToString()).ToString("dd.MM.yyyy"),
                                Пункт_ГИБДД = reader["gibdd_point"].ToString(),
                                Категория = reader["category"].ToString(),
                                Регион = reader["region"].ToString()
                            });
                        }
                    }
                }
                data_grid_driver_certificates.Columns[0].Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                data_grid_driver_certificates.Columns[0].Header = new TextBlock { Text = "Номер", FontWeight = FontWeights.Bold };
                data_grid_driver_certificates.Columns[1].Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                data_grid_driver_certificates.Columns[1].Header = new TextBlock { Text = "Фамилия", FontWeight = FontWeights.Bold };
                data_grid_driver_certificates.Columns[2].Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                data_grid_driver_certificates.Columns[2].Header = new TextBlock { Text = "Имя", FontWeight = FontWeights.Bold };
                data_grid_driver_certificates.Columns[3].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_driver_certificates.Columns[3].Header = new TextBlock { Text = "Отчество", FontWeight = FontWeights.Bold };
                data_grid_driver_certificates.Columns[4].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_driver_certificates.Columns[4].Header = new TextBlock { Text = "Статус", FontWeight = FontWeights.Bold };
                data_grid_driver_certificates.Columns[5].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_driver_certificates.Columns[5].Header = new TextBlock { Text = "Дата регистрации", FontWeight = FontWeights.Bold };
                data_grid_driver_certificates.Columns[6].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_driver_certificates.Columns[6].Header = new TextBlock { Text = "Дата окончания", FontWeight = FontWeights.Bold };
                data_grid_driver_certificates.Columns[7].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_driver_certificates.Columns[7].Header = new TextBlock { Text = "Пункт ГИБДД", FontWeight = FontWeights.Bold };
                data_grid_driver_certificates.Columns[8].Width = new DataGridLength(2, DataGridLengthUnitType.Star);
                data_grid_driver_certificates.Columns[8].Header = new TextBlock { Text = "Категория", FontWeight = FontWeights.Bold };
                data_grid_driver_certificates.Columns[9].Width = new DataGridLength(2, DataGridLengthUnitType.Star);
                data_grid_driver_certificates.Columns[9].Header = new TextBlock { Text = "Регион", FontWeight = FontWeights.Bold };
                data_grid_driver_certificates.ItemsSource = List_driver_certificates;
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
        private void tb_search_got_focus(object sender, EventArgs e)
        {
            if (tb_search.Text == tb_search_standart_text)
            {
                tb_search.Text = "";
                tb_search.Foreground = Brushes.Black;
            }
        }
        private void tb_search_text_changed(object sender, RoutedEventArgs e)
        {
            Search.Search_text = tb_search.Text;
            if (Search.Search_text != tb_search_standart_text && !string.IsNullOrWhiteSpace(Search.Search_text))
            {
                string search_text = tb_search.Text.Trim();
                LoadData();
            }
        }
        private void tb_search_lost_focus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_search.Text))
            {
                tb_search.Text = tb_search_standart_text;
                tb_search.Foreground = Brushes.Gray;
            }
        }
        private void Button_Click_back(object sender, RoutedEventArgs e)
        {
            Ordinary_menu ordinary_menu = new Ordinary_menu();
            ordinary_menu.Show();
            this.Close();
        }
    }
}