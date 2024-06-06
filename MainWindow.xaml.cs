using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Serilog;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;

namespace Prog_Lang_Score
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

       

        public MainWindow()
        {
            InitializeComponent();

            LoadDataSQL();

            LoadDataInCombobox();


            Log.Logger = new LoggerConfiguration().WriteTo.File("E:\\C# uroki\\Prog_Lang_Score\\Logs\\log.txt").CreateLogger();

        }


        // block code check input ----
        //PreviewKeyDown
        private void TextUser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // prevent space from being entered
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        // PreviewTextInput 
        private void NumberValidationTextUser(object sender, TextCompositionEventArgs e)
        {
            // prevents the user from entering characters
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // PreviewExecuted check copypaste through CTRL + V 
        private void TextUser_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        // method to check if the user is entering a valid range and also Null input
        private bool CheckInputRange()
        {
            bool BoolAsnwChek = false;  

            if (string.IsNullOrWhiteSpace(TextUser.Text))
            {
                MessageBox.Show("Please enter a value.", "Warning");
                return false;
            }

            uint checkRange = uint.Parse(TextUser.Text);    

            if (checkRange >= 0 && checkRange <= 100)
            {
                BoolAsnwChek = true;
            }
            else
            {
                MessageBox.Show("I'm so sorry but I won't work like this anymore,Pls enter the correct range", "Warning");
            }

            return BoolAsnwChek;
        }

        // block code check input ---


  





            private void Submit_Button(object sender, RoutedEventArgs e)
        {
            if (CheckInputRange())
            {
                uint scoreLanguage = uint.Parse(TextUser.Text);
                int comboboxSelectUser ;
                if (ChooseLanguage.SelectedItem.ToString() != null)
                {
                    comboboxSelectUser = int.Parse(ChooseLanguage.SelectedIndex.ToString()) + 1;
                    ReadTableColumn(scoreLanguage, comboboxSelectUser);
                }
                else
                {
                    MessageBox.Show("Please choose language","Warning");
                }



            }

        }


        private void Calculation(string language, int minScore,int maxScore, int avgScore,int numbersOFVotes,int comboboxSelectUser, uint scoreLanguage, int score) 
        {
            if (scoreLanguage <= minScore)
            {
                minScore = (int)scoreLanguage;
            }
            else if (scoreLanguage >= maxScore)
            {
                maxScore = (int)scoreLanguage;
            }

            //2
            avgScore = (int)((avgScore * numbersOFVotes)+scoreLanguage+score) / (numbersOFVotes+1);
            score += (int)scoreLanguage;
            //1
            //avgScore = (int)((avgScore * numbersOFVotes)+scoreLanguage) / (numbersOFVotes+1);

            numbersOFVotes += 1;

           UpdateTable(language, minScore, maxScore, avgScore, numbersOFVotes, comboboxSelectUser, scoreLanguage,score);//score
        }


        private async void UpdateTable (string language, int minScore, int maxScore, int avgScore, int numbersOFVotes, int comboboxSelectUser, uint scoreLanguage, int score)
        {
            string query = "UPDATE [Top_LangProgg] SET [Prog_Lang] = @language,[MinScore] = @minScore,[MaxScore] = @maxScore,[AvgScore] = @avgScore,[NumberOfVotes] = @numbersOFVotes,[Score] = @score WHERE [ID]=@id"; //-g

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-63SSU11\SQLEXPRESS;Initial Catalog=Top_Lang_Prog;Integrated Security=True;Trust Server Certificate=True"))
                {
                    await sqlConnection.OpenAsync();
                    if (sqlConnection.State == System.Data.ConnectionState.Open)
                    {
                        try
                        {
                            using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                            {
                                sqlCommand.Parameters.AddWithValue("@id", comboboxSelectUser);
                                sqlCommand.Parameters.AddWithValue("@language", language);
                                sqlCommand.Parameters.AddWithValue("@minScore", minScore);
                                sqlCommand.Parameters.AddWithValue("@maxScore", maxScore);
                                sqlCommand.Parameters.AddWithValue("@avgScore", avgScore);
                                sqlCommand.Parameters.AddWithValue("@numbersOFVotes", numbersOFVotes);
                                sqlCommand.Parameters.AddWithValue("@score",score);
                                //sqlCommand.Parameters.AddWithValue("@id", comboboxSelectUser);

                               await sqlCommand.ExecuteNonQueryAsync();
                            }
                        }
                        catch (SqlException sqlex)
                        {
                            Log.Error(sqlex, "Error add to database(method UpdateTable)");
                            Log.CloseAndFlush();
                        }
                        finally
                        {
                            await sqlConnection.CloseAsync();
                        }
                    }
                }
            }
            catch (SqlException sqlex)
            {
                Log.Error(sqlex, "Connection to Data Base failed(method UpdateTable)");
                Log.CloseAndFlush();
            }
            LoadDataSQL();
        }

        private async void ReadTableColumn(uint scoreLanguage, int comboboxSelectUser)
        {
            string query = "SELECT * FROM Top_LangProgg"; // -g 

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-63SSU11\SQLEXPRESS;Initial Catalog=Top_Lang_Prog;Integrated Security=True;Trust Server Certificate=True"))
                {
                    await sqlConnection.OpenAsync();
                    if (sqlConnection.State == System.Data.ConnectionState.Open)
                    {
                        try 
                        {
                            using (SqlCommand sqlCommand = new SqlCommand(query,sqlConnection))
                            {
                                try
                                {
                                    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                                    {
                                        DataTable dataTable = new DataTable("Top_LangProgg"); // -g 
                                        adapter.Fill(dataTable);
                                        string language;
                                        int minScore;
                                        int maxScore;
                                        int avgScore;
                                        int numbersOFVotes;
                                        int score;
                                        var row = dataTable.AsEnumerable().FirstOrDefault(r => r.Field<int>("ID") == comboboxSelectUser);

                                        if (row != null)
                                        {
                                            language = row.Field<string>("Prog_Lang");
                                            minScore = row.Field<int>("MinScore");
                                            maxScore = row.Field<int>("MaxScore");
                                            avgScore = row.Field<int>("AvgScore");
                                            numbersOFVotes = row.Field<int>("NumberOfVotes");
                                            score = row.Field<int>("Score");

                                            Calculation(language, minScore, maxScore, avgScore, numbersOFVotes, comboboxSelectUser, scoreLanguage,score);  
                                        }

                                    }

                                }
                                catch (SqlException sqlex)
                                {
                                    Log.Error(sqlex, "Error Load table(method ReadTableColumn)");
                                    Log.CloseAndFlush();
                                }
                            }
                            
                        }
                        catch (SqlException sqlex) 
                        {
                            Log.Error(sqlex, "Error Load database(method ReadTableColumn)");
                            Log.CloseAndFlush();
                        }
                        finally
                        {
                           await sqlConnection.CloseAsync();
                        }
                    }
                }

            }
            catch (SqlException sqlex) 
            {
                Log.Error(sqlex, "Connection to Data Base failed(method ReadTableColumn)");
                Log.CloseAndFlush();
            }

        }


        private async void LoadDataSQL()
        {
            string query = "SELECT Prog_Lang,MinScore,MaxScore,AvgScore,NumberOfVotes FROM Top_LangProgg"; // -g

            try 
            {
                using (SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-63SSU11\SQLEXPRESS;Initial Catalog=Top_Lang_Prog;Integrated Security=True;Trust Server Certificate=True"))
                {
                    
                    await sqlConnection.OpenAsync();
                    if (sqlConnection.State == System.Data.ConnectionState.Open)
                    {
                        try 
                        {
                            using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                            {
                                try
                                {
                                    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                                    {
                                        DataTable dataTable = new DataTable("Top_LangProgg"); //-g 
                                        adapter.Fill(dataTable);
                                      
                                        DataGridView.ItemsSource = dataTable.DefaultView;

                                        
                                        //DataGridView.Columns["Score"].Visibility = Visibility.Collapsed; // zak

                                        /*List<string> excludedColumns = new List<string> {"Score"};

                                        // Создайте столбцы в DataGrid только для тех, которые не входят в список исключенных
                                        foreach (DataColumn column in dataTable.Columns)
                                        {
                                             if (!excludedColumns.Contains(column.ColumnName))
                                             {

                                             DataGridView.Columns.Add(new DataGridTextColumn
                                             {
                                              Header = column.ColumnName,
                                              Binding = new Binding(column.ColumnName)
                                             });

                                             }
                                        }
                                         */
                                    }
                                }
                                catch (SqlException sqlex)
                                {
                                    Log.Error(sqlex, "Error Load table(method LoadDataSQL)");
                                    Log.CloseAndFlush();
                                }
                               
                            }
                        }
                        catch (SqlException sqlex)
                        {
                            Log.Error(sqlex, "Error Load database(method LoadDataSQL)");
                            Log.CloseAndFlush();
                        }
                        finally
                        {
                           await sqlConnection.CloseAsync();
                        }
                    }
                }
            }
            catch(SqlException sqlex)
            {
                Log.Error(sqlex, "Connection to Data Base failed(method LoadDataSQL)");
                Log.CloseAndFlush();
            }

        }

        private async void LoadDataInCombobox()
        {
            string query = "SELECT Prog_Lang FROM Top_LangProgg";//-g
            try {
                using (SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-63SSU11\SQLEXPRESS;Initial Catalog=Top_Lang_Prog;Integrated Security=True;Trust Server Certificate=True"))
                {
                    await sqlConnection.OpenAsync();
                    if (sqlConnection.State == System.Data.ConnectionState.Open)
                    {
                        try 
                        {
                            using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                            {
                                try 
                                {
                                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            ChooseLanguage.Items.Add(reader["Prog_Lang"].ToString());
                                        }
                                    }
                                }
                                catch (SqlException sqlexld)
                                {
                                    Log.Error(sqlexld, "Error read table(method LoadDataInCombobox)");
                                    Log.CloseAndFlush();
                                }
                                
                            }
                        }
                        catch (SqlException sqlexld)
                        {
                            Log.Error(sqlexld, "Error Load database(method LoadDataInCombobox)");
                            Log.CloseAndFlush();
                        }
                        finally 
                        {
                            await sqlConnection.CloseAsync(); 
                        }

                    }
                }
            }
            catch (SqlException sqlexld)
            {
                Log.Error(sqlexld, "Connection to Data Base failed(method LoadDataInCombobox)");
                Log.CloseAndFlush();
            }

        }
    }
}