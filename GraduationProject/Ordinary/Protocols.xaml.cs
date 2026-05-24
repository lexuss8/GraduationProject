using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для Protocols.xaml
    /// </summary>
    public partial class Protocols : Window
    {
        NpgsqlConnection con = new Npgsql.NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        NpgsqlCommand cmd = new NpgsqlCommand();
        private DataTable data_table_protocols;
        public Protocols()
        {
            InitializeComponent();
            cmd.Connection = con;
            LoadData();
        }
        private async void LoadData()
        {
            try
            {
                await con.OpenAsync();

                string sql = @"
            SELECT 
            p.id AS protocol_id,
            to_char(p.date, 'HH24:MI DD.MM.YYYY') AS protocol_date,
            p.source AS protocol_source,
            p.place AS place,

            l.article AS law_article,
            l.code AS law_code,

            c.id AS car_id,
            m.mark AS car_mark,
            mm.model AS car_model,
            c.color AS car_color,

            s.id AS state_number_id,
            s.number AS state_number,

            pm.member_role AS member_role,

            p_people.id AS people_id,
            p_people.last_name AS people_last_name,
            p_people.first_name AS people_first_name,
            p_people.middle_name AS people_middle_name,

            p.description AS description,

            e.token AS employee_token,
            p_employee.last_name AS employee_last_name,
            p_employee.first_name AS employee_first_name,
            p_employee.middle_name AS employee_middle_name

            FROM Protocols p
            LEFT JOIN Protocol_members pm ON p.id = pm.protocol_id
            LEFT JOIN Laws l ON p.law_id = l.id
            LEFT JOIN Employees e ON p.employee_token = e.token
            LEFT JOIN People p_employee ON e.people_id = p_employee.id
            LEFT JOIN People p_people ON pm.people_id = p_people.id
            LEFT JOIN Cars c ON pm.car_id = c.id
            LEFT JOIN Mark_models mm ON c.mark_model_id = mm.id
            LEFT JOIN Marks m ON mm.mark_id = m.id
            LEFT JOIN State_numbers s ON c.state_number_id = s.id
            ORDER BY p.date DESC;";

                using (var cmd = new NpgsqlCommand(sql, con))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    data_table_protocols = new DataTable();
                    data_table_protocols.Load(reader);
                    data_table_protocols.CaseSensitive = false;
                }

                data_table_protocols.AcceptChanges();

                foreach (DataColumn col in data_table_protocols.Columns)
                {
                    col.ReadOnly = false;
                }

                data_grid_protocols.ItemsSource = data_table_protocols.DefaultView;
                data_grid_protocols.MinRowHeight = 35;
                data_grid_protocols.RowHeight = double.NaN;

                // Скрытые служебные ID
                data_grid_protocols.Columns[6].Visibility = Visibility.Collapsed;  // car_id
                data_grid_protocols.Columns[10].Visibility = Visibility.Collapsed; // state_number_id
                data_grid_protocols.Columns[13].Visibility = Visibility.Collapsed; // people_id

                // Если token сотрудника нужен только для внутренней работы, тоже можно скрыть
                data_grid_protocols.Columns[18].Visibility = Visibility.Collapsed; // employee_token

                data_grid_protocols.Columns[0].Width = new DataGridLength(150, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[0].Header = new TextBlock { Text = "№ протокола", FontWeight = FontWeights.Bold };
                data_grid_protocols.Columns[1].Width = new DataGridLength(180, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[1].Header = new TextBlock { Text = "Дата протокола", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[2].Width = new DataGridLength(120, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[2].Header = new TextBlock { Text = "Источник", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[3].Width = new DataGridLength(300, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[3].Header = new TextBlock { Text = "Место", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[4].Width = new DataGridLength(100, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[4].Header = new TextBlock { Text = "Статья", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[5].Width = new DataGridLength(100, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[5].Header = new TextBlock { Text = "Код", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[7].Width = new DataGridLength(120, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[7].Header = new TextBlock { Text = "Марка", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[8].Width = new DataGridLength(120, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[8].Header = new TextBlock { Text = "Модель", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[9].Width = new DataGridLength(100, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[9].Header = new TextBlock { Text = "Цвет", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[11].Width = new DataGridLength(120, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[11].Header = new TextBlock { Text = "Гос. номер", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[12].Width = new DataGridLength(160, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[12].Header = new TextBlock { Text = "Роль участника", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[14].Width = new DataGridLength(180, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[14].Header = new TextBlock { Text = "Фамилия участника", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[15].Width = new DataGridLength(160, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[15].Header = new TextBlock { Text = "Имя участника", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[16].Width = new DataGridLength(180, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[16].Header = new TextBlock { Text = "Отчество участника", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[17].Width = new DataGridLength(350, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[17].Header = new TextBlock { Text = "Описание", FontWeight = FontWeights.Bold };

                Style descriptionStyle = new Style(typeof(TextBlock));
                descriptionStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                descriptionStyle.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Top));
                descriptionStyle.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(5)));


                data_grid_protocols.Columns[19].Width = new DataGridLength(180, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[19].Header = new TextBlock { Text = "Фамилия сотрудника", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[20].Width = new DataGridLength(160, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[20].Header = new TextBlock { Text = "Имя сотрудника", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[21].Width = new DataGridLength(180, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[21].Header = new TextBlock { Text = "Отчество сотрудника", FontWeight = FontWeights.Bold };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            finally
            {
                await con.CloseAsync();
            }
        }
        private void Button_Click_edit(object sender, RoutedEventArgs e)
        {
        }
        private void Button_Click_add(object sender, RoutedEventArgs e)
        {
        }
        private async void Button_Click_delete(object sender, RoutedEventArgs e)
        {
        }
        private async void Button_Click_save(object sender, RoutedEventArgs e)
        {
        }
        private void TextBox_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (data_table_protocols == null)
                return;

            string searchText = TextBox_search.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                data_table_protocols.DefaultView.RowFilter = "";
                return;
            }

            searchText = searchText.Replace("'", "''");

            data_table_protocols.DefaultView.RowFilter = $@"
        Convert(protocol_id, 'System.String') LIKE '%{searchText}%'
        OR protocol_date LIKE '%{searchText}%'
        OR protocol_source LIKE '%{searchText}%'
        OR place LIKE '%{searchText}%'
        OR law_article LIKE '%{searchText}%'
        OR law_code LIKE '%{searchText}%'
        OR car_mark LIKE '%{searchText}%'
        OR car_model LIKE '%{searchText}%'
        OR car_color LIKE '%{searchText}%'
        OR state_number LIKE '%{searchText}%'
        OR member_role LIKE '%{searchText}%'
        OR people_last_name LIKE '%{searchText}%'
        OR people_first_name LIKE '%{searchText}%'
        OR people_middle_name LIKE '%{searchText}%'
        OR description LIKE '%{searchText}%'
        OR employee_last_name LIKE '%{searchText}%'
        OR employee_first_name LIKE '%{searchText}%'
        OR employee_middle_name LIKE '%{searchText}%'
    ";
        }
        private void Button_Click_back(object sender, RoutedEventArgs e)
        {
            Ordinary_menu ordinary_menu = new Ordinary_menu();
            ordinary_menu.Show();
            this.Close();
        }
    }
}
