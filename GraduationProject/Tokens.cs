using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Npgsql;

namespace GraduationProject
{
    internal class Tokens
    {
        NpgsqlConnection con = new NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        NpgsqlCommand cmd = new NpgsqlCommand();

        public async Task CheckSession()
        {
            await con.OpenAsync();

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(appDirectory, "token.txt");

            if (File.Exists(filePath))
            {
                await OpenFile();
            }

            await con.CloseAsync();
        }


        public void CreateFile(int user_id)
        {
            try
            {
                con.Open();
            }
            catch (Npgsql.PostgresException ex)
            {
                MessageBox.Show("Ошибка подключения к Postgres: " + ex);
            }

            string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string FilePath = Path.Combine(AppDirectory, "token.txt"); //объединяет 2 строки в одну, предоставляющую путь к файлу

            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string token = new string(Enumerable.Repeat(chars, 10)
                                             .Select(s => s[random.Next(s.Length)])
                                             .ToArray());
            using (FileStream create_token_file = new FileStream(FilePath, FileMode.Create))
            using (StreamWriter creator_token = new StreamWriter(create_token_file))
            {
                    creator_token.Write(token);
            }
            string sql = @"INSERT INTO Sessions (user_id, token, last_login_at, expires_at)" +
                         "VALUES (@user_id, @token, NOW(), NOW() + INTERVAL '5 minutes');";
            using (var cmd = new NpgsqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("user_id", user_id);
                cmd.Parameters.AddWithValue("token", token);
                cmd.ExecuteNonQuery();
            }
            con.Close();
        }

        public void UpdateFile ()
        {

        }

        public async Task OpenFile()
        {
            string token;
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(appDirectory, "token.txt");

            token = File.ReadAllText(filePath).Trim();

            bool result = false;
            int role_id = -1;
            string login = "";

            // ---------- SELECT ----------
            using (var selectCmd = new NpgsqlCommand(
                "SELECT * FROM Function_comparison_tokens(@token);", con))
            {
                selectCmd.Parameters.AddWithValue("token", token);

                using (var readerDb = await selectCmd.ExecuteReaderAsync())
                {
                    if (await readerDb.ReadAsync())
                    {
                        result = readerDb.GetBoolean(0);
                        login = readerDb.GetString(1);
                        role_id = readerDb.GetInt32(2);
                    }
                }
            }

            // ---------- UPDATE ----------
            if (result)
            {
                Session.Login = login;
                using (var updateCmd = new NpgsqlCommand(
                    @"UPDATE Sessions 
              SET last_login_at = NOW(),
                  expires_at = NOW() + INTERVAL '5 minutes'
              WHERE token = @token;", con))
                {
                    updateCmd.Parameters.AddWithValue("token", token);
                    await updateCmd.ExecuteNonQueryAsync();
                }
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.Entry_roles(role_id);
            }
        }
    }
}
