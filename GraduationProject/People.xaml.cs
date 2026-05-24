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

namespace GraduationProject
{
    /// <summary>
    /// Логика взаимодействия для People.xaml
    /// </summary>
    public partial class People : Window
    {
        Npgsql.NpgsqlConnection con = new NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        Npgsql.NpgsqlCommand cmd = new NpgsqlCommand();
        string tb_search_standart_text = "🔍 Поиск";
        public People()
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
            List<PeopleView> List_people = new List<PeopleView>();
            try
            {
                await con.OpenAsync();
                string sql = @"SELECT last_name, first_name, middle_name, birthday, photo, inn, passport, phone, residence FROM People";
                string[] search_parts = Search.Search_text.Split(
                    new[] { ' ' });
                if (Search.Search_text != tb_search_standart_text && !string.IsNullOrWhiteSpace(Search.Search_text)) {
                    if (search_parts.Length == 1)
                    {
                        sql += @" WHERE last_name ILIKE @p0 OR first_name ILIKE @p0 OR
                            middle_name ILIKE @p0 OR
                            inn ILIKE @p0 OR passport ILIKE @p0 OR
                            phone ILIKE @p0 OR residence ILIKE @p0;";
                    }
                    if (search_parts.Length == 2) {
                        sql += @" WHERE (last_name ILIKE @p0 AND first_name ILIKE @p1)
                             OR (first_name ILIKE @p0 AND last_name ILIKE @p1);";
                    }
                    if (search_parts.Length == 3)
                    {
                        sql += @" WHERE (last_name ILIKE @p0 AND first_name ILIKE @p1 AND middle_name ILIKE @p2)
                             OR (first_name ILIKE @p0 AND last_name ILIKE @p1 AND middle_name ILIKE @p2);";
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
                                string photoPath = reader["photo"].ToString();
                                BitmapImage image = null;
                                if (!string.IsNullOrEmpty(photoPath))
                                {
                                    string fullPath = $"pack://application:,,,/Images/People/{photoPath}";
                                    image = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                                }
                                List_people.Add(new PeopleView
                                {
                                    Фамилия = reader["last_name"].ToString(),
                                    Имя = reader["first_name"].ToString(),
                                    Отчество = reader["middle_name"].ToString(),
                                    Дата_рождения = DateTime.Parse(reader["birthday"].ToString()).ToString("dd.MM.yyyy"),
                                    Фото = image,
                                    ИНН = reader["inn"].ToString(),
                                    Паспорт = reader["passport"].ToString(),
                                    Телефон = reader["phone"].ToString(),
                                    Прописка = reader["residence"].ToString()
                                });
                            }
                        }
                    }
                data_grid_people.Columns[0].Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                data_grid_people.Columns[0].Header = new TextBlock { Text = "Фамилия", FontWeight = FontWeights.Bold };

                data_grid_people.Columns[1].Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                data_grid_people.Columns[1].Header = new TextBlock { Text = "Имя", FontWeight = FontWeights.Bold };

                data_grid_people.Columns[2].Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                data_grid_people.Columns[2].Header = new TextBlock { Text = "Отчество", FontWeight = FontWeights.Bold };

                data_grid_people.Columns[3].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_people.Columns[3].Header = new TextBlock { Text = "Дата рождения", FontWeight = FontWeights.Bold };

                data_grid_people.Columns[4].Width = new DataGridLength(0.5, DataGridLengthUnitType.Star);
                data_grid_people.Columns[4].Header = new TextBlock { Text = "Фото", FontWeight = FontWeights.Bold };

                data_grid_people.Columns[5].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_people.Columns[5].Header = new TextBlock { Text = "ИНН", FontWeight = FontWeights.Bold };

                data_grid_people.Columns[6].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_people.Columns[6].Header = new TextBlock { Text = "Паспорт", FontWeight = FontWeights.Bold };

                data_grid_people.Columns[7].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_people.Columns[7].Header = new TextBlock { Text = "Телефон", FontWeight = FontWeights.Bold };

                data_grid_people.Columns[8].Width = new DataGridLength(2, DataGridLengthUnitType.Star);
                data_grid_people.Columns[8].Header = new TextBlock { Text = "Прописка", FontWeight = FontWeights.Bold };
                data_grid_people.ItemsSource = List_people;
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
            if (Search.Search_text != tb_search_standart_text)
            {
                string search_text = tb_search.Text.Trim();
                LoadData();
            }
        }
        private void tb_search_lost_focus (object sender, EventArgs e)
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
