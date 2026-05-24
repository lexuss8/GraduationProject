using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using Npgsql;
using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
using System.Windows.Shapes;
using System.Xml.Linq;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace GraduationProject.Admin
{
    /// <summary>
    /// Логика взаимодействия для User_sessions.xaml
    /// </summary>
    public partial class User_sessions : Window
    {
        NpgsqlConnection con = new Npgsql.NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        NpgsqlCommand cmd = new NpgsqlCommand();
        private DataTable data_table_sessions;
        private DataRowView editable_row = null;
        private int selected_id;
        private DataRow added_row = null;
        public User_sessions()
        {
            InitializeComponent();
            try
            {
                cmd.Connection = con;
            }
            catch (Npgsql.PostgresException ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message.ToString());
            }
            LoadData();
        }
        private void Button_Click_back(object sender, RoutedEventArgs e)
        {
            Admin_menu admin_menu = new Admin_menu();
            admin_menu.Show();
            this.Close();
        }
        private async void LoadData()
        {
            try
            {
                await con.OpenAsync();
                cmd.CommandText = @"
                    SELECT 
                        s.id,
                        u.login AS login,
                        s.token AS token,
                        to_char(s.created_at, 'HH24:MI.SS DD.MM.YYYY') AS created_at,
                        to_char(s.last_login_at, 'HH24:MI.SS DD.MM.YYYY') AS last_login_at,
                        to_char(s.expires_at, 'HH24:MI.SS DD.MM.YYYY') AS expires_at
                    FROM Sessions s
                    JOIN Users u ON s.user_id = u.id
                    ORDER BY s.expires_at DESC;";
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

                data_table_sessions = new DataTable();
                data_table_sessions.Load(reader);
                data_table_sessions.AcceptChanges(); //Помечает все загруженные строки как Unchanged, Unchanged - строка из базы - не изменена
                //Перебор каждого столбца и разрешение редактирования
                foreach (DataColumn col in data_table_sessions.Columns)
                {
                    col.ReadOnly = false;
                }
                data_grid_table_sessions.ItemsSource = data_table_sessions.DefaultView;
                data_grid_table_sessions.Columns[0].Visibility = Visibility.Collapsed;
                data_grid_table_sessions.RowHeight = 35;
                reader.Close();
                data_grid_table_sessions.Columns[1].Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                data_grid_table_sessions.Columns[1].Header = new TextBlock { Text = "Логин", FontWeight = FontWeights.Bold};
                data_grid_table_sessions.Columns[2].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_table_sessions.Columns[2].Header = new TextBlock { Text = "Токен", FontWeight = FontWeights.Bold };
                data_grid_table_sessions.Columns[3].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_table_sessions.Columns[3].Header = new TextBlock { Text = "Время создания", FontWeight = FontWeights.Bold };
                data_grid_table_sessions.Columns[4].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_table_sessions.Columns[4].Header = new TextBlock { Text = "Время последней авторизации", FontWeight = FontWeights.Bold};
                data_grid_table_sessions.Columns[5].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_table_sessions.Columns[5].Header = new TextBlock { Text = "Время истечения", FontWeight = FontWeights.Bold };
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
        private void data_grid_table_sessions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Button_end_session.IsEnabled = false;

            if (data_grid_table_sessions.SelectedItem is DataRowView row_view)
            {
                string expiresText = row_view["expires_at"]?.ToString();

                DateTime expiresAt;

                bool parsed = DateTime.TryParseExact(
                    expiresText,
                    "HH:mm.ss dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out expiresAt);

                if (parsed && expiresAt > DateTime.Now)
                {
                    Button_end_session.IsEnabled = true;
                }
            }
        }
        private void data_grid_table_sessions_beginning_edit(object sender, DataGridBeginningEditEventArgs e)
        {
            //e.Row.Item - объект строки, по которой пользователь начал редактирование
            if (e.Row.Item is DataRowView rowView)
            {
                // Разрешаем:
                // 1) новые строки
                // 2) строку, разрешённую кнопкой "Редактировать"
                if (rowView.Row.RowState == DataRowState.Added)
                    return;

                if (editable_row != null && rowView == editable_row)
                    return;

                // Всё остальное — запрещаем
                e.Cancel = true; //запрет редактирования
            }
        }
        private async void Button_Click_add(object sender, RoutedEventArgs e)
        {
            DataRow new_row = data_table_sessions.NewRow();

            new_row["id"] = DBNull.Value;
            new_row["login"] = DBNull.Value;
            new_row["token"] = DBNull.Value;

            new_row["created_at"] = DateTime.Now.ToString("HH:mm.ss dd.MM.yyyy");
            new_row["last_login_at"] = DateTime.Now.ToString("HH:mm.ss dd.MM.yyyy");
            new_row["expires_at"] = DateTime.Now.AddDays(7).ToString("HH:mm.ss dd.MM.yyyy");

            data_table_sessions.Rows.Add(new_row);
            added_row = new_row;

            Button_cancel.Visibility = Visibility.Visible;

            DataRowView row_view =
                data_table_sessions.DefaultView[data_table_sessions.Rows.Count - 1];

            data_grid_table_sessions.UpdateLayout();
            data_grid_table_sessions.SelectedItem = row_view;

            data_grid_table_sessions.CurrentCell =
                new DataGridCellInfo(row_view, data_grid_table_sessions.Columns[1]); // login

            data_grid_table_sessions.ScrollIntoView(row_view);

            await Task.Delay(50);

            data_grid_table_sessions.Focus();
            data_grid_table_sessions.BeginEdit();

            Button_save.IsEnabled = true;
            Button_edit.IsEnabled = false;
            Button_cancel.Visibility = Visibility.Visible;
        }
        private void Button_Click_cancel(object sender, RoutedEventArgs e)
        {
            if (added_row != null)
            {
                data_table_sessions.Rows.Remove(added_row);

                added_row = null;
            }

            Button_cancel.Visibility = Visibility.Collapsed;

            Button_save.IsEnabled = false;
        }
        private void Button_Click_edit(object sender, RoutedEventArgs e)
        {
            Button_save.IsEnabled = true;
            if (data_grid_table_sessions.SelectedItem is DataRowView row_view)
            {
                editable_row = row_view;
                data_grid_table_sessions.BeginEdit();
                data_grid_table_sessions.Focus();
                data_grid_table_sessions.SelectedItem = editable_row;
            }
            Button_edit.IsEnabled = false;
        }
        private async void Button_Click_save(object sender, RoutedEventArgs e)
        {
            DataRow[] array_new_rows =
                data_table_sessions.Select(null, null, DataViewRowState.Added);

            DataRow[] array_edit_rows =
                data_table_sessions.Select(null, null, DataViewRowState.ModifiedCurrent);

            if (array_new_rows.Length == 0 && array_edit_rows.Length == 0)
            {
                MessageBox.Show("Нет новых записей для сохранения.");
                return;
            }

            try
            {
                await con.OpenAsync();

                int row_saved = 0;

                // ДОБАВЛЕНИЕ
                foreach (DataRow row in array_new_rows)
                {
                    string login = row["login"]?.ToString();

                    string token = row["token"]?.ToString();
                    DateTime created_at = DateTime.ParseExact(
                        row["created_at"].ToString(),
                        "HH:mm.ss dd.MM.yyyy",
                        CultureInfo.InvariantCulture);

                    DateTime last_login_at = DateTime.ParseExact(
                        row["last_login_at"].ToString(),
                        "HH:mm.ss dd.MM.yyyy",
                        CultureInfo.InvariantCulture);

                    DateTime expires_at = DateTime.ParseExact(
                        row["expires_at"].ToString(),
                        "HH:mm.ss dd.MM.yyyy",
                        CultureInfo.InvariantCulture);

                    // Валидация
                    if (string.IsNullOrWhiteSpace(login) ||
                        string.IsNullOrWhiteSpace(token))
                    {
                        MessageBox.Show("Логин или токен не могут быть пустыми.");
                        continue;
                    }

                    // Получение user_id по login
                    int user_id;

                    string sql_get_user =
                        "SELECT id FROM Users WHERE login = @login;";

                    using (var cmd_user = new NpgsqlCommand(sql_get_user, con))
                    {
                        cmd_user.Parameters.AddWithValue("login", login);

                        object result = await cmd_user.ExecuteScalarAsync();

                        if (result == null)
                        {
                            MessageBox.Show($"Пользователь '{login}' не найден.");
                            continue;
                        }

                        user_id = Convert.ToInt32(result);
                    }

                    // INSERT
                    string sql =
                        @"INSERT INTO Sessions
                (user_id, token, created_at, last_login_at, expires_at)
                VALUES
                (@user_id, @token, @created_at, @last_login_at, @expires_at);";

                    using (var cmd_insert = new NpgsqlCommand(sql, con))
                    {
                        cmd_insert.Parameters.AddWithValue("user_id", user_id);
                        cmd_insert.Parameters.AddWithValue("token", token);
                        cmd_insert.Parameters.AddWithValue("created_at", created_at);
                        cmd_insert.Parameters.AddWithValue("last_login_at", last_login_at);
                        cmd_insert.Parameters.AddWithValue("expires_at", expires_at);

                        row_saved += await cmd_insert.ExecuteNonQueryAsync();
                    }
                }
                // РЕДАКТИРОВАНИЕ
                foreach (DataRow row in array_edit_rows)
                {
                    int session_id = Convert.ToInt32(row["id"]);
                    string login = row["login"]?.ToString();

                    string token = row["token"]?.ToString();

                    DateTime created_at = DateTime.ParseExact(
                        row["created_at"].ToString(),
                        "HH:mm.ss dd.MM.yyyy",
                        CultureInfo.InvariantCulture);

                    DateTime last_login_at = DateTime.ParseExact(
                        row["last_login_at"].ToString(),
                        "HH:mm.ss dd.MM.yyyy",
                        CultureInfo.InvariantCulture);

                    DateTime expires_at = DateTime.ParseExact(
                        row["expires_at"].ToString(),
                        "HH:mm.ss dd.MM.yyyy",
                        CultureInfo.InvariantCulture);

                    // Получение user_id по login
                    int user_id;

                    string sql_get_user =
                        "SELECT id FROM Users WHERE login = @login;";

                    using (var cmd_user = new NpgsqlCommand(sql_get_user, con))
                    {
                        cmd_user.Parameters.AddWithValue("login", login);

                        object result = await cmd_user.ExecuteScalarAsync();

                        if (result == null)
                        {
                            MessageBox.Show($"Пользователь '{login}' не найден.");
                            continue;
                        }

                        user_id = Convert.ToInt32(result);
                    }

                    // UPDATE
                    string sql_update =
                        @"UPDATE Sessions
                          SET token = @token,
                              created_at = @created_at,
                              last_login_at = @last_login_at,
                              expires_at = @expires_at
                          WHERE id = @id;";

                    using (var cmd_update = new NpgsqlCommand(sql_update, con))
                    {
                        cmd_update.Parameters.AddWithValue("id", session_id);
                        cmd_update.Parameters.AddWithValue("token", token);
                        cmd_update.Parameters.AddWithValue("created_at", created_at);
                        cmd_update.Parameters.AddWithValue("last_login_at", last_login_at);
                        cmd_update.Parameters.AddWithValue("expires_at", expires_at);
                        cmd_update.Parameters.AddWithValue("user_id", user_id);

                        row_saved += await cmd_update.ExecuteNonQueryAsync();
                    }
                }
                if (row_saved > 0)
                {
                    MessageBox.Show("Изменения сохранены");

                    Button_save.IsEnabled = false;

                    Button_cancel.Visibility = Visibility.Hidden;

                    added_row = null;

                    data_table_sessions.AcceptChanges();
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
        private async void Button_Click_end_session(object sender, RoutedEventArgs e)
        {
            if (!(data_grid_table_sessions.SelectedItem is DataRowView row_view))
            {
                MessageBox.Show("Выберите сессию.");
                return;
            }

            int session_id = Convert.ToInt32(row_view["id"]);

            try
            {
                await con.OpenAsync();

                string sql = @"UPDATE Sessions
                       SET expires_at = NOW()
                       WHERE id = @id;";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("id", session_id);

                    int result = await cmd.ExecuteNonQueryAsync();

                    if (result > 0)
                    {
                        MessageBox.Show("Сессия завершена.");
                        Button_end_session.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении сессии: {ex.Message}");
            }
            finally
            {
                await con.CloseAsync();
            }

            LoadData();
        }
        private void Button_Click_export_pdf(object sender, RoutedEventArgs e)
        {
            if (data_table_sessions == null || data_table_sessions.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF файл (*.pdf)|*.pdf";
            saveDialog.FileName = "Сессии.pdf";

            if (saveDialog.ShowDialog() != true)
                return;

            ExportSessionsToPdf(saveDialog.FileName);

            MessageBox.Show("PDF-файл успешно создан.");
        }
        private void ComboBox_export_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_export.SelectedIndex == 0)
                return;

            if (ComboBox_export.SelectedIndex == 1)
            {
                Button_Click_export_pdf(sender, e);
            }
            else if (ComboBox_export.SelectedIndex == 2)
            {
                Button_Click_export_excel(sender, e);
            }

            ComboBox_export.SelectedIndex = 0;
        }
        private void ExportSessionsToPdf(string filePath)
        {
            Document document = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);

            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            document.Open();

            string fontPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
            "arial.ttf");

            BaseFont baseFont = BaseFont.CreateFont(
                fontPath,
                BaseFont.IDENTITY_H,
                BaseFont.EMBEDDED);

            Font titleFont = new Font(baseFont, 16, Font.BOLD);
            Font cellFont = new Font(baseFont, 10, Font.NORMAL);

            iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("Список сессий пользователей", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 15;
            document.Add(title);

            PdfPTable table = new PdfPTable(data_table_sessions.Columns.Count);
            table.WidthPercentage = 100;

            foreach (DataColumn column in data_table_sessions.Columns)
            {
                PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName, cellFont));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
            }

            foreach (DataRow row in data_table_sessions.Rows)
            {
                foreach (object item in row.ItemArray)
                {
                    table.AddCell(new Phrase(item?.ToString(), cellFont));
                }
            }

            document.Add(table);

            document.Close();
        }
        private void Button_Click_export_excel(object sender, RoutedEventArgs e)
        {
            if (data_table_sessions == null || data_table_sessions.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel файл (*.xlsx)|*.xlsx";
            saveDialog.FileName = "Сессии.xlsx";

            if (saveDialog.ShowDialog() != true)
                return;

            ExportSessionsToExcel(saveDialog.FileName);

            MessageBox.Show("Excel-файл успешно создан.");
        }
        private void ExportSessionsToExcel(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Сессии");

                // Заголовки
                worksheet.Cell(1, 1).Value = "Логин";
                worksheet.Cell(1, 2).Value = "Токен";
                worksheet.Cell(1, 3).Value = "Время создания";
                worksheet.Cell(1, 4).Value = "Время последней авторизации";
                worksheet.Cell(1, 5).Value = "Время истечения";

                int rowIndex = 2;

                foreach (DataRow row in data_table_sessions.Rows)
                {
                    worksheet.Cell(rowIndex, 1).Value = row["login"]?.ToString();
                    worksheet.Cell(rowIndex, 2).Value = row["token"]?.ToString();
                    worksheet.Cell(rowIndex, 3).Value = row["created_at"]?.ToString();
                    worksheet.Cell(rowIndex, 4).Value = row["last_login_at"]?.ToString();
                    worksheet.Cell(rowIndex, 5).Value = row["expires_at"]?.ToString();

                    rowIndex++;
                }

                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }
        }
    }
}