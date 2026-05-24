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
    /// Логика взаимодействия для ProceduralActions.xaml
    /// </summary>
    public partial class ProceduralActions : Window
    {
        Npgsql.NpgsqlConnection con = new NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        Npgsql.NpgsqlCommand cmd = new NpgsqlCommand();
        public ProceduralActions()
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
            List<Procedural_ActionsView> List_procedural_actions = new List<Procedural_ActionsView>();
            try
            {
                await con.OpenAsync();
                string sql = @"SELECT sequence_number, title, description, legal_basis FROM Procedural_actions;";
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            List_procedural_actions.Add(new Procedural_ActionsView
                            {
                                Последовательность = reader["sequence_number"].ToString(),
                                Название = reader["title"].ToString(),
                                Описание = reader["description"].ToString(),
                                Правовая_база = reader["legal_basis"].ToString()
                            });
                        }
                    }
                }
                data_grid_procedural_actions.Columns[0].Width = new DataGridLength(0.6, DataGridLengthUnitType.Star);
                data_grid_procedural_actions.Columns[0].Header = new TextBlock { Text = "Последовательность", FontWeight = FontWeights.Bold };

                data_grid_procedural_actions.Columns[1].Width = new DataGridLength(1.7, DataGridLengthUnitType.Star);
                data_grid_procedural_actions.Columns[1].Header = new TextBlock { Text = "Название", FontWeight = FontWeights.Bold };

                data_grid_procedural_actions.Columns[2].Width = new DataGridLength(2, DataGridLengthUnitType.Star);
                data_grid_procedural_actions.Columns[2].Header = new TextBlock { Text = "Описание", FontWeight = FontWeights.Bold };

                data_grid_procedural_actions.Columns[3].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_procedural_actions.Columns[3].Header = new TextBlock { Text = "Правовая база", FontWeight = FontWeights.Bold };
                data_grid_procedural_actions.ItemsSource = List_procedural_actions;
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
        private void Button_Click_back(object sender, RoutedEventArgs e)
        {
            Ordinary_menu ordinary_menu = new Ordinary_menu();
            ordinary_menu.Show();
            this.Close();
        }
    }
}