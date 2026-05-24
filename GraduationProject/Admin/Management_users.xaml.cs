using DocumentFormat.OpenXml.Office2013.Word;
using GraduationProject.Admin;
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

namespace GraduationProject.Admin
{
    /// <summary>
    /// Логика взаимодействия для Management_users.xaml
    /// </summary>
    public partial class Management_users : Window
    {
        NpgsqlConnection con = new Npgsql.NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        NpgsqlCommand cmd = new NpgsqlCommand();
        private DataTable data_table_users;
        private string SelectedLogin;
        private List<Role> roles = new List<Role>();
        private List<TableUser> users = new List<TableUser>();
        private TableUser editable_user;
        private List<EmployeeForComboBox> employees = new List<EmployeeForComboBox>();
        public Management_users()
        {
            InitializeComponent();
            cmd.Connection = con;

            Loaded += async (s, e) =>
            {
                await con.OpenAsync();

                await LoadRoles();
                await LoadEmployees();
                await LoadData();

                await con.CloseAsync();
            };
        }
        private async Task LoadRoles()
        {
            roles.Clear();

            string sql = "SELECT id, name FROM Roles ORDER BY id;";

            using (var cmd = new NpgsqlCommand(sql, con))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    roles.Add(new Role
                    {
                        id = Convert.ToInt32(reader["id"]),
                        name = reader["name"].ToString()
                    });
                }
            }
        }
        private async Task LoadData()
        {
            users.Clear();

            string sqlUsers = @"
        SELECT 
            u.id,
            u.login,
            u.password,
            u.employee_token,
            p.last_name || ' ' || p.first_name || ' ' || p.middle_name AS fio,
            u.role_id,
            r.name AS role_name
        FROM Users u
        JOIN Roles r ON u.role_id = r.id
        LEFT JOIN Employees e ON u.employee_token = e.token
        LEFT JOIN People p ON e.people_id = p.id;";

            using (var cmd = new NpgsqlCommand(sqlUsers, con))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    users.Add(new TableUser
                    {
                        id = Convert.ToInt32(reader["id"]),
                        login = reader["login"].ToString(),
                        password = reader["password"].ToString(),

                        employee_token = reader["employee_token"] == DBNull.Value
                            ? null
                            : reader["employee_token"].ToString(),

                        fio = reader["fio"] == DBNull.Value
                            ? ""
                            : reader["fio"].ToString(),

                        role_id = Convert.ToInt32(reader["role_id"]),
                        role_name = reader["role_name"].ToString()
                    });
                }
            }

            data_grid_table_users.ItemsSource = users;

            column_employee.ItemsSource = employees;
            column_role.ItemsSource = roles;

            data_grid_table_users.Columns[1].Header = new TextBlock { Text = "Логин", FontWeight = FontWeights.Bold };
            data_grid_table_users.Columns[2].Header = new TextBlock { Text = "Пароль", FontWeight = FontWeights.Bold };
            column_employee.Header = new TextBlock { Text = "ФИО", FontWeight = FontWeights.Bold };
            column_role.Header = new TextBlock { Text = "Роль", FontWeight = FontWeights.Bold };
        }
        private async Task LoadEmployees()
        {
            employees.Clear();

            string sql = @"
        SELECT 
            e.token,
            p.last_name || ' ' || p.first_name || ' ' || p.middle_name AS fio
        FROM Employees e
        JOIN People p ON e.people_id = p.id
        ORDER BY p.last_name, p.first_name, p.middle_name;";

            using (var cmd = new NpgsqlCommand(sql, con))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    employees.Add(new EmployeeForComboBox
                    {
                        token = reader["token"].ToString(),
                        fio = reader["fio"].ToString()
                    });
                }
            }
        }
        private void data_grid_table_users_selected(object sender, SelectionChangedEventArgs e)
        {
            Button_edit.IsEnabled = false;

            TableUser user = data_grid_table_users.SelectedItem as TableUser;

            if (user == null)
                return;

            // Новая строка ещё не сохранена в БД, её редактировать через кнопку не нужно
            if (user.id == 0)
                return;

            Button_edit.IsEnabled = true;
        }

        private void Button_Click_add(object sender, RoutedEventArgs e)
        {
            TableUser new_user = new TableUser
            {
                id = 0,
                login = "",
                password = "",
                employee_token = null,
                role_id = 3
            };

            users.Add(new_user);

            data_grid_table_users.ItemsSource = null;
            data_grid_table_users.ItemsSource = users;

            data_grid_table_users.SelectedItem = new_user;
            data_grid_table_users.ScrollIntoView(new_user);

            Button_save.IsEnabled = true;
            Button_edit.IsEnabled = false;
        }
        private void Button_Click_edit(object sender, RoutedEventArgs e)
        {
            if (data_grid_table_users.SelectedItem is TableUser user)
            {
                editable_user = user;

                Button_save.IsEnabled = true;
                Button_edit.IsEnabled = false;

                data_grid_table_users.Focus();
                data_grid_table_users.BeginEdit();
            }
        }
        //Точка входа в режим редактирования строки. Без этого события и метода можно двойным ЛКМ редактировать любые строки.
        private void data_grid_table_users_beginning_edit(object sender, DataGridBeginningEditEventArgs e)
        {
            //e.Row.Item - объект строки, по которой пользователь начал редактирование
            if (e.Row.Item is TableUser user)
            {
                // Разрешаем редактирование:
                // 1) новой строки
                if (user.id == 0)
                    return;

                //Строки через кнопку "Редактировать"
                if (editable_user != null && user == editable_user)
                    return;

                // Всё остальное — запрещаем
                e.Cancel = true; //запрет редактирования
            }
        }
        private async void Button_Click_save(object sender, RoutedEventArgs e)
        {
            TableUser user = data_grid_table_users.SelectedItem as TableUser;

            if (user == null)
            {
                MessageBox.Show("Строка не выбрана.");
                return;
            }

            if (string.IsNullOrWhiteSpace(user.login) ||
                string.IsNullOrWhiteSpace(user.password))
            {
                MessageBox.Show("Логин и пароль не могут быть пустыми.");
                return;
            }

            if (string.IsNullOrWhiteSpace(user.employee_token))
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            try
            {
                await con.OpenAsync();

                if (user.id == 0)
                {
                    string sql_insert = @"
                INSERT INTO Users (login, password, employee_token, role_id)
                VALUES (@login, @password, @employee_token, @role_id);";

                    using (var cmd = new NpgsqlCommand(sql_insert, con))
                    {
                        cmd.Parameters.AddWithValue("login", user.login);
                        cmd.Parameters.AddWithValue("password", user.password);
                        cmd.Parameters.AddWithValue("employee_token", user.employee_token);
                        cmd.Parameters.AddWithValue("role_id", user.role_id);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    MessageBox.Show("Пользователь добавлен.");
                }
                else
                {
                    string sql_update = @"
                UPDATE Users
                SET login = @login,
                    password = @password,
                    employee_token = @employee_token,
                    role_id = @role_id
                WHERE id = @id;";

                    using (var cmd = new NpgsqlCommand(sql_update, con))
                    {
                        cmd.Parameters.AddWithValue("login", user.login);
                        cmd.Parameters.AddWithValue("password", user.password);
                        cmd.Parameters.AddWithValue("employee_token", user.employee_token);
                        cmd.Parameters.AddWithValue("role_id", user.role_id);
                        cmd.Parameters.AddWithValue("id", user.id);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    MessageBox.Show("Изменения сохранены.");
                }

                Button_save.IsEnabled = false;

                await LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
            finally
            {
                await con.CloseAsync();
            }
        }
        private async void Button_Click_delete(object sender, RoutedEventArgs e)
        {
            TableUser user = data_grid_table_users.SelectedItem as TableUser;

            if (user == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Удалить пользователя \"{user.login}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await con.OpenAsync();

                string sql = "DELETE FROM Users WHERE id = @id;";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("id", user.id);

                    int deleted = await cmd.ExecuteNonQueryAsync();

                    if (deleted > 0)
                    {
                        users.Remove(user);

                        data_grid_table_users.ItemsSource = null;
                        data_grid_table_users.ItemsSource = users;

                        Button_delete.IsEnabled = false;
                        Button_edit.IsEnabled = false;
                        Button_save.IsEnabled = false;

                        MessageBox.Show("Пользователь удалён.");
                    }
                }
            }
            catch (PostgresException ex)
            {
                MessageBox.Show("Ошибка PostgreSQL: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            finally
            {
                await con.CloseAsync();
            }
        }
        private void Button_Click_back(object sender, RoutedEventArgs e)
        {
            Admin_menu admin_menu = new Admin_menu();
            admin_menu.Show();
            this.Close();
        }
    }
}