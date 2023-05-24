﻿using Npgsql;
using System.Data;

namespace AGC_Management.Services.DatabaseHandler;

public static class DatabaseService
{
    public static NpgsqlConnection? dbConnection;

    public static void OpenConnection()
    {
        try
        {
            var dbConfigSection = GlobalProperties.DebugMode ? "DatabaseCfgDBG" : "DatabaseCfg";
            var DbHost = GlobalProperties.ConfigIni[dbConfigSection]["Database_Host"];
            var DbUser = GlobalProperties.ConfigIni[dbConfigSection]["Database_User"];
            var DbPass = GlobalProperties.ConfigIni[dbConfigSection]["Database_Password"];
            var DbName = GlobalProperties.ConfigIni[dbConfigSection]["Database"];

            dbConnection = new NpgsqlConnection($"Host={DbHost};Username={DbUser};Password={DbPass};Database={DbName}");
            try
            {
                if (dbConnection.State != ConnectionState.Open)
                    dbConnection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while opening the database connection: " + ex.Message +
                                  "\nFunctionality will be restricted and the Program can be Unstable. Continue at your own risk!\nPress any key to continue");
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while opening the database connection: " + ex.Message);
            throw;
        }
    }


    public static void CloseConnection()
    {
        try
        {
            dbConnection.Close();
            dbConnection.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while closing the database connection: " + ex.Message);
            throw;
        }
    }

    public static bool IsConnected()
    {
        try
        {
            if (dbConnection.State == ConnectionState.Open)
                return true;
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Change DBContent
    public static void ExecuteCommand(string sql)
    {
        try
        {
            using var cmd = new NpgsqlCommand(sql, dbConnection);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while executing the database command: " + ex.Message);
            throw;
        }
    }

    public static string GetConnectionString()
    {
        var dbConfigSection = GlobalProperties.DebugMode ? "DatabaseCfgDBG" : "DatabaseCfg";
        var DbHost = GlobalProperties.ConfigIni[dbConfigSection]["Database_Host"];
        var DbUser = GlobalProperties.ConfigIni[dbConfigSection]["Database_User"];
        var DbPass = GlobalProperties.ConfigIni[dbConfigSection]["Database_Password"];
        var DbName = GlobalProperties.ConfigIni[dbConfigSection]["Database"];
        return $"Host={DbHost};Username={DbUser};Password={DbPass};Database={DbName}";
    }

    // Read DBContent
    public static NpgsqlDataReader ExecuteQuery(string sql)
    {
        try
        {
            using (var cmd = new NpgsqlCommand(sql, dbConnection))
            {
                return cmd.ExecuteReader();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while executing the database query: " + ex.Message);
            throw;
        }
    }
}