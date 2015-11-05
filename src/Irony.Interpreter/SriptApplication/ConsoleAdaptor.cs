﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Irony.Parsing;

namespace Irony.Interpreter
{
    //WARNING: Ctrl-C for aborting running script does NOT work when you run console app from Visual Studio 2010. 
    // Run executable directly from bin folder. 

    public enum ConsoleTextStyle
    {
        Normal,
        Error,
    }

    // Default implementation of IConsoleAdaptor with System Console as input/output. 
    public class ConsoleAdapter : IConsoleAdaptor
    {
        public ConsoleAdapter()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
        }

        void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; //do not kill the app yet
            Canceled = true;
        }

        public bool Canceled { get; set; }

        public void Write(string text)
        {
            Console.Write(text);
        }
        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }
        public void SetTextStyle(ConsoleTextStyle style)
        {
            switch (style)
            {
                case ConsoleTextStyle.Normal:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case ConsoleTextStyle.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
        }

        public int Read()
        {
            return Console.Read();
        }

        public string ReadLine()
        {
            var input = Console.ReadLine();
            Canceled = (input == null);  // Windows console method ReadLine returns null if Ctrl-C was pressed.
            return input;
        }
        public void SetTitle(string title)
        {
#if DNXCORE50
#else
            Console.Title = title; 
#endif
        }
    }


}