using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VLC_playlist_creator;

internal class Program
{
    static readonly string[] possibleSwitches = ["/D", "/S", "/T"];
    const string preList = """<?xml version="1.0" encoding="UTF-8"?>""" + "\r\n" + 
        """<playlist xmlns="http://xspf.org/ns/0/" xmlns:vlc="http://www.videolan.org/vlc/playlist/ns/0/" version="1">""" + "\r\n" +
        "\t<title>Playlist</title>" + "\r\n" +
        "\t<trackList>\r\n";
    const string postList = "\t</trackList>" + "\r\n" +
        "</playlist>";

    static void Main(string[] args)
    {
        /*
         * /S C:\Users\Administrator\Music\
         * /D Y:\Safe\Start Menu\VLC\
         * /T 90
         */
        if(ArgsInvalid(out string message, out Dictionary<string, string> convertedArgs, args))
        {
            Console.WriteLine(message);
            return;
        }

        if(!Directory.Exists(convertedArgs["/S"]))
        {
            Console.WriteLine("Folder not found " + convertedArgs["/S"]);
            return;
        }

        List<string> recentSongs = [];
        List<string> sourceDirSongsPaths = new(Directory.GetFiles(convertedArgs["/S"]));
        string[] subDirsPaths = Directory.GetDirectories(convertedArgs["/S"]);


        if(!ushort.TryParse(convertedArgs["/T"], out ushort days))
        {
            Console.WriteLine($"Failed to parse {convertedArgs["/T"]} to ushort.");
            return;
        }

        DateTime recentThreshold = DateTime.UtcNow.Subtract(TimeSpan.FromDays(days));

        foreach(string rootSong in sourceDirSongsPaths)
        {
            FileInfo rootSongFileInfo = new(rootSong);

            if(DateTime.Compare(recentThreshold, rootSongFileInfo.CreationTimeUtc) <= 0 ||
                DateTime.Compare(recentThreshold, rootSongFileInfo.LastWriteTimeUtc) <= 0)
                recentSongs.Add(rootSong);
        }

        foreach(string subDirPath in subDirsPaths)
        {
            string folderName = subDirPath[(subDirPath.LastIndexOf('\\')+1)..];
            string[] filesInAsubDir = Directory.GetFiles(subDirPath);
            sourceDirSongsPaths.AddRange(filesInAsubDir);
            GeneratePlaylist(folderName, convertedArgs["/D"], filesInAsubDir);

            foreach(string fileInAsubDir in filesInAsubDir)
            {
                FileInfo subSongFileInfo = new(fileInAsubDir);

                if(DateTime.Compare(recentThreshold, subSongFileInfo.CreationTimeUtc) <= 0 ||
                DateTime.Compare(recentThreshold, subSongFileInfo.LastWriteTimeUtc) <= 0)
                    recentSongs.Add(fileInAsubDir);
            }
        }

        GeneratePlaylist("recent", convertedArgs["/D"], recentSongs);
        GeneratePlaylist("all", convertedArgs["/D"], sourceDirSongsPaths);

#if DEBUG
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Press any key to continue...");
        Console.ReadKey();
#endif
    }

    static void GeneratePlaylist(string playlistName, string outputPath, IEnumerable<string> songsPaths)
    {
        using StreamWriter playlistFileOutput = new(Path.Join(outputPath, playlistName + ".xspf"), false);
        playlistFileOutput.Write(preList);
        ushort killCheck = ushort.MinValue;
        StringBuilder locationBuilder = new();
        foreach(string songPath in songsPaths)
        {
            if(songPath.Contains("desktop.ini", StringComparison.OrdinalIgnoreCase))
                continue;

            playlistFileOutput.Write("\t\t<track>\r\n\t\t\t<location>file:///");

            foreach(char songPathLetter in songPath)
            {
                string add = songPathLetter switch
                {
                    ' ' => "%20",
                    '!' => "%21",
                    '"' => "%22",
                    '#' => "%23",
                    '$' => "%24",
                    '%' => "%25",
                    '&' => "%26",
                    '\'' => "%27",
                    '(' => "%28",
                    ')' => "%29",
                    '*' => "%2A",
                    '+' => "%2B",
                    ',' => "%2C",
                    '-' => "%2D",
                    // '.' => "%2E", not needed
                    '/' => "%2F",
                    // ':' => "%3A", not needed
                    ';' => "%3B",
                    '<' => "%3C",
                    '=' => "%3D",
                    '>' => "%3E",
                    '?' => "%3F",
                    '@' => "%40",
                    '[' => "%5B",
                    '\\' => "%2F", // instead of "%5C",
                    ']' => "%5D",
                    '^' => "%5E",
                    '_' => "%5F",
                    '`' => "%60",
                    '{' => "%7B",
                    '|' => "%7C",
                    '}' => "%7D",
                    '~' => "%7E",
                    '£' => "%C2%A3",
                    '€' => "%E2%82%AC",
                    _ => songPathLetter.ToString()
                };

                locationBuilder.Append(add);
            }

            playlistFileOutput.Write(locationBuilder);
            playlistFileOutput.WriteLine("</location>\r\n\t\t</track>");
            locationBuilder.Clear();
            if(++killCheck > 9000)
            {
                Console.WriteLine("Do you have more than 9000 songs? Shutting down for safety.");
                return;
            }
        }

        playlistFileOutput.Write(postList);
        playlistFileOutput.Close();
    }

    /// <summary>
    /// Checks if provided arguments are invalid
    /// </summary>
    /// <param name="message">if it returns</param>
    /// <param name="dict">sdf</param>
    /// <param name="args">dfgdfg</param>
    /// <returns><see langword="true"/> if invalid, <see langword="false"/> if valid</returns>
    static bool ArgsInvalid(out string message, out Dictionary<string, string> dict, params string[] args)
    {
        dict = [];
        if(args.Length == 0)
        {
            message = "No arguments have been specified. Use /? for help.";
            return true;
        }

        if(args[0] == "/?")
        {
            message = "This tool generates a couple of VLC playlists:\r\n";
            message += "\t1. recent.xspf playlist - creates a playlist from last X days where you specify X by using /T parameter\r\n";
            message += "\t2. all.xspf playlist - creates a playlist containing all songs in the directory you specify by /S parameter and in its all subdirectories (1 step only)\r\n";
            message += "\t3. for each folder (or let's say \"sub folder\") you have it will generate a playlist as well\r\n\r\n";
            message += "Switches you must use:";
            message += "\r\n   /S\tsource location where all your music is located, root directory e.g. " + @"C:\Users\Administrator\Music\" + Environment.NewLine +
                "   /D\tdestination location where you want .xspf playlist files to appear" + Environment.NewLine +
                "   /T\ttime in days, how many last days to check for to generate recent.xspf playlist";

            return true;
        }

        if(args.Length % 2 != 0)
        {
            message = "Parameters number must be even.";
            return true;
        }

        if(args.Length > possibleSwitches.Length * 2)
        {
            message = $"Too many arguments provided: {args.Length} but the limit is {possibleSwitches.Length * 2}.";
            return true;
        }

        for(byte x = 0 ; x < args.Length ; x += 2)
        {
            string param = args[x].ToUpper();

            if(!possibleSwitches.Any(R => R == param))
            {
                message = "Unrecognized switch: " + args[x];
                return true;
            }

            if(!dict.TryAdd(param, args[x + 1]))
            {
                message = "Duplicate parameters or parameters specified incorrectly.";
                return true;
            }
        }

        message = string.Empty;
        return false;
    }
}