using System;
using System.Data;
using System.Data.SQLite;

public class dBHelper
{
    private string connectionString;
    private SQLiteConnection connection;
    private SQLiteDataAdapter dataAdapter;
    public DataSet DataSet { get; private set; }

    public dBHelper(string dbPath)
    {
        connectionString = $"Data Source={dbPath};Version=3;";
        connection = new SQLiteConnection(connectionString);
        DataSet = new DataSet();
    }

    public bool Load(string commandText, string tableName)
    {
        try
        {
            connection.Open();
            dataAdapter = new SQLiteDataAdapter(commandText, connection);
            DataSet.Clear();
            dataAdapter.Fill(DataSet, tableName);
            connection.Close();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (connection.State == ConnectionState.Open)
                connection.Close();
            return false;
        }
    }

    public bool Save()
    {
        try
        {
            connection.Open();
            SQLiteCommandBuilder commandBuilder = new SQLiteCommandBuilder(dataAdapter);
            dataAdapter.Update(DataSet.Tables[0]);
            connection.Close();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (connection.State == ConnectionState.Open)
                connection.Close();
            return false;
        }
    }
}
