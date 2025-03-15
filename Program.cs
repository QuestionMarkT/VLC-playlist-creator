using System;
using System.IO;

namespace VLC_playlist_creator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(ArgsInvalid(out string message, args))
            {
                Console.WriteLine(message);
                return;
            }
        }

        static bool ArgsInvalid(out string message, params string[] args)
        {
            if(args.Length == 0)
            {
                message = "No arguments have been specified. Use /? for help";
                return true;
            }

            if(args[0] == "/?")
            {

            }

            foreach(string arg in args)
            {

            }

        }
    }
}