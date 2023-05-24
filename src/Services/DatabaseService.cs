﻿using System.Data;
using Npgsql;

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

    public static async Task InsertDataIntoTable(string tableName, Dictionary<string, object> columnValuePairs)
    {
        string insertQuery = $"INSERT INTO {tableName} ({string.Join(", ", columnValuePairs.Keys)}) " +
                             $"VALUES ({string.Join(", ", columnValuePairs.Keys.Select(k => $"@{k}"))})";

        await using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            await connection.OpenAsync();

            await using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
            {
                foreach (var kvp in columnValuePairs)
                {
                    NpgsqlParameter parameter = new NpgsqlParameter($"@{kvp.Key}", kvp.Value);
                    command.Parameters.Add(parameter);
                }

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public static async Task<List<Dictionary<string, object>>> SelectDataFromTable(string tableName, List<string> columns, Dictionary<string, object> whereConditions)
    {
        string selectQuery;
        if (columns.Contains("*"))
        {
            selectQuery = $"SELECT * FROM \"{tableName}\"";
        }
        else
        {
            string columnNames = string.Join(", ", columns.Select(c => $"\"{c}\""));
            selectQuery = $"SELECT {columnNames} FROM \"{tableName}\"";
        }

        if (whereConditions != null && whereConditions.Count > 0)
        {
            string whereClause = string.Join(" AND ", whereConditions.Select(c => $"\"{c.Key}\" = @{c.Key}"));
            selectQuery += $" WHERE {whereClause}";
        }

        List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

        await using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            await connection.OpenAsync();

            await using (NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection))
            {
                if (whereConditions != null && whereConditions.Count > 0)
                {
                    foreach (var condition in whereConditions)
                    {
                        NpgsqlParameter parameter = new NpgsqlParameter($"@{condition.Key}", condition.Value);
                        command.Parameters.Add(parameter);
                    }
                }

                await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            object columnValue = reader.GetValue(i);

                            row[columnName] = columnValue;
                        }

                        results.Add(row);
                    }
                }
            }
        }

        return results;
    }




}