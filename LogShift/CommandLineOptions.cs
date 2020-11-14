using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace LogShift
{
    public class Options
    {
        [Option('f', "file", Required = true, HelpText = "Log Filename to process. Can also omit the -f and just put the filename as the only argument.", Default = null)]
        public string File { get; set; }
    }

}
