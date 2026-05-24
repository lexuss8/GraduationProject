using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
    /// Логика взаимодействия для Cars.xaml
    /// </summary>
    public partial class Cars : Window
    {
        Npgsql.NpgsqlConnection con = new NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        Npgsql.NpgsqlCommand cmd = new NpgsqlCommand();
        string tb_search_standart_text = "🔍 Поиск";
        public Cars()
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
            List<CarsView> List_cars = new List<CarsView>();
            try
            {
                await con.OpenAsync();
                string sql = @"SELECT m.mark, mm.model, c.color, s.number, c.release, c.vin, c.engine, p.last_name,
                                p.first_name, p.middle_name, i.name FROM Cars c
								JOIN Mark_models mm ON c.mark_model_id = mm.id
								JOIN Marks m ON mm.mark_id = m.id
								JOIN State_numbers s ON c.state_number_id = s.id
                                JOIN People p ON c.owner_id = p.id
                                JOIN Insurances i ON c.insurance_id = i.id";
                string[] search_parts = Search.Search_text.Split(
                    new[] { ' ' });
                if (Search.Search_text != tb_search_standart_text && !string.IsNullOrWhiteSpace(Search.Search_text))
                {
                    if (search_parts.Length == 1)
                    {
                        sql += @" WHERE 
            m.mark ILIKE @p0 OR
            mm.model ILIKE @p0 OR
            c.vin ILIKE @p0 OR
            c.color::text ILIKE @p0 OR
            s.number ILIKE @p0 OR
            p.last_name ILIKE @p0 OR
            p.first_name ILIKE @p0 OR
            p.middle_name ILIKE @p0 OR
            i.name ILIKE @p0";
                    }

                    if (search_parts.Length == 2)
                    {
                        sql += @" WHERE
            (p.last_name ILIKE @p0 AND p.first_name ILIKE @p1)
            OR (p.first_name ILIKE @p0 AND p.last_name ILIKE @p1)
            OR (m.mark ILIKE @p0 AND mm.model ILIKE @p1)";
                    }

                    if (search_parts.Length == 3)
                    {
                        sql += @" WHERE
            (p.last_name ILIKE @p0 AND p.first_name ILIKE @p1 AND p.middle_name ILIKE @p2)
            OR (p.first_name ILIKE @p0 AND p.last_name ILIKE @p1 AND p.middle_name ILIKE @p2)";
                    }
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
                    if (Search.Search_text != tb_search_standart_text && !string.IsNullOrWhiteSpace(Search.Search_text) && search_parts.Length == 3)
                    {
                        cmd.Parameters.AddWithValue("p0", $"%{search_parts[0]}%");
                        cmd.Parameters.AddWithValue("p1", $"%{search_parts[1]}%");
                        cmd.Parameters.AddWithValue("p2", $"%{search_parts[2]}%");
                    }
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var engineJson = reader["engine"]?.ToString();

                            string engineText = null;
                            if (!string.IsNullOrEmpty(engineJson))
                            {
                                var doc = JsonDocument.Parse(engineJson);
                                engineText = string.Join(", ",
                                    doc.RootElement.EnumerateObject()
                                       .Select(p => $"{p.Name}: {p.Value.GetString()}"));
                            }
                            List_cars.Add(new CarsView
                            {
                                Марка = reader["mark"].ToString(),
                                Модель = reader["model"].ToString(),
                                Цвет = reader["color"].ToString(),
                                Государственный_номер = reader["number"].ToString(),
                                Год_выпуска = reader["release"].ToString(),
                                VIN = reader["vin"].ToString(),
                                Двигатель = engineText,
                                Имя = reader["first_name"].ToString(),
                                Фамилия = reader["last_name"].ToString(),
                                Отчество = reader["middle_name"].ToString(),
                                Страховая_компания = reader["name"].ToString()
                            });
                        }
                    }
                }
                data_grid_cars.Columns[0].Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                data_grid_cars.Columns[0].Header = new TextBlock { Text = "Марка", FontWeight = FontWeights.Bold };

                data_grid_cars.Columns[1].Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                data_grid_cars.Columns[1].Header = new TextBlock { Text = "Модель", FontWeight = FontWeights.Bold };

                data_grid_cars.Columns[2].Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                data_grid_cars.Columns[2].Header = new TextBlock { Text = "Цвет", FontWeight = FontWeights.Bold };

                data_grid_cars.Columns[3].Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                data_grid_cars.Columns[3].Header = new TextBlock { Text = "Государственный номер", FontWeight = FontWeights.Bold };

                data_grid_cars.Columns[4].Width = new DataGridLength(0.7, DataGridLengthUnitType.Star);
                data_grid_cars.Columns[4].Header = new TextBlock { Text = "Год выпуска", FontWeight = FontWeights.Bold };

                data_grid_cars.Columns[5].Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                data_grid_cars.Columns[5].Header = new TextBlock { Text = "VIN", FontWeight = FontWeights.Bold };

                data_grid_cars.Columns[6].Width = new DataGridLength(2, DataGridLengthUnitType.Star);
                data_grid_cars.Columns[6].Header = new TextBlock { Text = "Двигатель", FontWeight = FontWeights.Bold };

                data_grid_cars.Columns[7].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_cars.Columns[7].Header = new TextBlock { Text = "Фамилия", FontWeight = FontWeights.Bold };

                data_grid_cars.Columns[8].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_cars.Columns[8].Header = new TextBlock { Text = "Имя", FontWeight = FontWeights.Bold };

                data_grid_cars.Columns[9].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_cars.Columns[9].Header = new TextBlock { Text = "Отчество", FontWeight = FontWeights.Bold };

                data_grid_cars.Columns[10].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_cars.Columns[10].Header = new TextBlock { Text = "ОСАГО", FontWeight = FontWeights.Bold };
                data_grid_cars.ItemsSource = List_cars;
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
            if (Search.Search_text != tb_search_standart_text)
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