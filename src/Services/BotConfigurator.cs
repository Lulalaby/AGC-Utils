﻿using AGC_Management.Helpers;
using DisCatSharp.Entities;
using IniParser;
using IniParser.Model;

namespace AGC_Management;

public static class BotConfig
{
    public static IniData GetConfig()
    {
        IniData ConfigIni;
        FileIniDataParser parser = new();
        try
        {
            ConfigIni = parser.ReadFile("config.ini");
        }
        catch
        {
            Console.WriteLine("Die Konfigurationsdatei konnte nicht geladen werden. Bitte überprüfe die config.");
            Console.WriteLine("Drücke eine beliebige Taste um das Programm zu beenden.");
            Console.ReadKey();
            Environment.Exit(0);
            return null;
        }

        return ConfigIni;
    }

    public static DiscordColor GetEmbedColor()
    {
        string fallbackColor = "000000";
        string colorString;

        try
        {
            string colorConfig = GetConfig()["EmbedConfig"]["DefaultEmbedColor"];
            if (colorConfig.StartsWith("#")) colorConfig = colorConfig.Remove(0, 1);

            if (string.IsNullOrEmpty(colorConfig) || !HexCheck.IsHexColor(colorConfig))
            {
                colorString = fallbackColor;
                return new DiscordColor(colorString);
            }

            colorString = colorConfig;
        }
        catch
        {
            colorString = fallbackColor;
        }

        return new DiscordColor(colorString);
    }
}