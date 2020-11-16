using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace LogShift
{
    public class Options
    {
        [Value(0, 
            HelpText = "The Filename to process. Specify either a single filename or use the -f option."
        )]
        public string FileArg { get; set; }


        private string _file = null;
        [Option('f', "file", 
            HelpText = "Log Filename to process. Can also omit the -f and just put the filename as the only argument.", 
            Default = null
        )]
        public string File 
        { 
            get
            {
                if (_file == null) return this.FileArg;
                return _file;
            }
            set
            {
                _file = value;
            }
        }


        [Option('d', "durationtag",
            HelpText = "String used to preface duration values inserted into shifted log file",
            Default = "(DURATION)"
        )]
        public string DurationTag { get; set; }


        [Option('m', "monthfirst",
            Default = true,
            HelpText = "true if the month should be assumed to be before the day in date stamps"
        )]
        public bool MonthFirst{ get; set; }


        [Option('y', "yearfirst",
            Default = true,
            HelpText = "true if the year should be assumed to be before the month and day in date stamps"
        )]
        public bool YearFirst { get; set; }
    }

}
