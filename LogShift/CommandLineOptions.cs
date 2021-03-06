﻿using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace LogShift
{
    public class Options
    {
        [Value(0, 
            HelpText = "The Filename(s) to process. Specify either one or more filenames or use the -f option."
        )]
        public IEnumerable<string> Files { get; set; }


        [Option('f', "file",
            HelpText = "Log Filename to process. Can also omit the -f and just put the filename as the only argument.",
            Default = null
        )]
        public string File { get; set; }


        [Option('d', "durationtag",
            HelpText = "String used to preface duration values inserted into shifted log file",
            Default = "(DURATION)"
        )]
        public string DurationTag { get; set; }


        [Option('s', "outputsuffix",
            HelpText = "The Suffix to append to file name when writing the output file",
            Default = "shifted"
        )]
        public string OutputSuffix { get; set; }


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
