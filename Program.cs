using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VLC_playlist_creator;

internal class Program
{
    static readonly string[] possibleSwitches = ["/D", "/S", "/T"];

    static void Main(string[] args)
    {
       /*
        * /S C:\Users\Administrator\Music\
        * /D Y:\Safe\Start Menu\VLC\
        * /N Custom generated playlist
        * /T 90
        * 
        */
        if(ArgsInvalid(out string message, out Dictionary<string, string> convertedArgs, args))
        {
            Console.WriteLine(message);
            return;
        }

        if(!Directory.Exists(convertedArgs["/S"]))
        {
            Console.WriteLine("Folder not found " + convertedArgs["/S"]);
        }
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
            message = "/S\tsource location where all your music is located, root directory e.g. " + @"C:\Users\Administrator\Music\" + Environment.NewLine +
                "/D\tdestination location where you want .xspf playlist files to appear" + Environment.NewLine +
                "/T\ttime in days, how many last days to check for to generate recent.xspf playlist";

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