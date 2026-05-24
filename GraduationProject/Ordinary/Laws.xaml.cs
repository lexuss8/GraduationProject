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
    /// Логика взаимодействия для Laws.xaml
    /// </summary>
    public partial class Laws : Window
    {
        Npgsql.NpgsqlConnection con = new NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        Npgsql.NpgsqlCommand cmd = new NpgsqlCommand();
        string tb_search_standart_text = "🔍 Поиск";
        public Laws()
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
            List<LawsView> List_laws = new List<LawsView>();
            try
            {
                await con.OpenAsync();
                string sql = @"SELECT lc.name, l.article, l.title, l.description, l.penalty
FROM Laws l JOIN law_categories lc ON l.category_id = lc.id;";
                string[] search_parts = Search.Search_text.Split(
                    new[] { ' ' });
        //        if (Search.Search_text != tb_search_standart_text && !string.IsNullOrWhiteSpace(Search.Search_text))
        //        {
        //            if (search_parts.Length == 1)
        //            {
        //                sql += @" WHERE 
        //                m.mark ILIKE @p0 OR
        //                mm.model ILIKE @p0 OR
        //                c.vin ILIKE @p0 OR
        //                c.color::text ILIKE @p0 OR
        //                p.last_name ILIKE @p0 OR
        //                p.first_name ILIKE @p0 OR
        //                p.middle_name ILIKE @p0 OR
        //                i.name ILIKE @p0";
        //            }
        //            if (search_parts.Length == 2)
        //            {
        //                sql += @" WHERE
        //            (p.last_name ILIKE @p0 AND p.first_name ILIKE @p1)
        //OR (p.first_name ILIKE @p0 AND p.last_name ILIKE @p1)
        //OR (m.mark ILIKE @p0 AND mm.model ILIKE @p1)";
        //            }
        //            if (search_parts.Length == 3)
        //            {
        //                sql += @" WHERE
        //(p.last_name ILIKE @p0 AND p.first_name ILIKE @p1 AND p.middle_name ILIKE @p2)
        //OR (p.first_name ILIKE @p0 AND p.last_name ILIKE @p1 AND p.middle_name ILIKE @p2)";
        //            }
        //        }
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
                            List_laws.Add(new LawsView
                            {
                                Категория = reader["name"].ToString(),
                                Статья = reader["article"].ToString(),
                                Название = reader["title"].ToString(),
                                Описание = reader["description"].ToString(),
                                Наказание = reader["penalty"].ToString()
                            });
                        }
                    }
                }
                data_grid_laws.Columns[0].Width = new DataGridLength(0.5, DataGridLengthUnitType.Star);
                data_grid_laws.Columns[0].Header = new TextBlock { Text = "Категория", FontWeight = FontWeights.Bold };

                data_grid_laws.Columns[1].Width = new DataGridLength(0.5, DataGridLengthUnitType.Star);
                data_grid_laws.Columns[1].Header = new TextBlock { Text = "Статья", FontWeight = FontWeights.Bold };

                data_grid_laws.Columns[2].Width = new DataGridLength(0.5, DataGridLengthUnitType.Star);
                data_grid_laws.Columns[2].Header = new TextBlock { Text = "Название", FontWeight = FontWeights.Bold };

                data_grid_laws.Columns[3].Width = new DataGridLength(1.8, DataGridLengthUnitType.Star);
                data_grid_laws.Columns[3].Header = new TextBlock { Text = "Описание", FontWeight = FontWeights.Bold };

                data_grid_laws.Columns[4].Width = new DataGridLength(2, DataGridLengthUnitType.Star);
                data_grid_laws.Columns[4].Header = new TextBlock { Text = "Наказание", FontWeight = FontWeights.Bold };
                //data_grid_laws.Columns[5].Width = new DataGridLength(1.4, DataGridLengthUnitType.Star);
                data_grid_laws.ItemsSource = List_laws;
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