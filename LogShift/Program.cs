using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;


[assembly: InternalsVisibleTo("UnitTests")]

namespace LogShift
{
    public class Program
    {
        public static string DurationTag = "(DURATION)";
        public static bool MonthFirst = true;
        public static bool YearFirst = true;

        private static bool _console = false;


        static void Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<Options>(args);

            parseResult.WithParsed<Options>(o =>
                {
                    if (o.File == null)
                    {
                        //render missing file argument error
                        Console.Error.Write(GetHelp<Options>(parseResult));
                        Console.Error.WriteLine("\r\nMust specify either a single filename as argument or provide the -f argument.");
                        return;
                    }

                    Program.DurationTag = o.DurationTag;
                    Program.MonthFirst = o.MonthFirst;
                    Program.YearFirst = o.YearFirst;

                    _console = true;
                    WriteLine("LogShift");
                    WriteLine("(c) 2020 drVenture");
                    WriteLine("Translate all datetime stamps in log file back to 0 based on first detected datetime stamp.");
                    WriteLine("");


                    var reader = OpenLog(o.File);
                    var writer = new System.IO.StreamWriter(o.File + ".Shifted");
                    ShiftLog(reader, writer);
                    writer.Close();
                    reader.Close();

                    WriteLine("Done");
                });
        }

        //Generate Help text
        internal static string GetHelp<T>(ParserResult<T> result)
        {
            // use default configuration
            // you can customize HelpText and pass different configuratins
            //see wiki
            // https://github.com/commandlineparser/commandline/wiki/How-To#q1
            // https://github.com/commandlineparser/commandline/wiki/HelpText-Configuration
            return CommandLine.Text.HelpText.AutoBuild(result, h => h, e => e);
        }


        internal static void WriteLine(string msg = "")
        {
            if (_console) Console.WriteLine(msg);
        }

        internal static void Write(string msg = "")
        {
            if (_console) Console.Write(msg);
        }

        internal static void ResetLeft()
        {
            if (_console) Console.CursorLeft = 0;
        }


        internal static TextReader OpenLog(string filename)
        {
            WriteLine($"Loading {filename}...");
            return new StreamReader(System.IO.File.OpenRead(filename));
        }


        internal static DateTime? BaseTimeStamp { get; set; }

        internal static DateTime? LastTimeStamp { get; set; }


        internal static void ShiftLog(TextReader textReader, TextWriter textWriter)
        {
            var c = 0;

            do
            {
                var b = textReader.ReadLine();
                if (b == null) break;

                c++;

                b = ShiftLine(b);

                Write($"Processing line: {c}");
                ResetLeft();

                textWriter.WriteLine(b);
            } while (true);

            WriteLine();
        }


        /// <summary>
        /// Parse timestamps from line and shift all stamps, rewriting the line
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        internal static string ShiftLine(string buf)
        {
            TimeSpan span = TimeSpan.Zero;
            var o = new StringBuilder();

            var stamps = ParseLine(buf);
            var s = 0;

            for (int i = 0; i < stamps.Count; i++)
            {
                var stamp = stamps[i];

                //put a space unless the stamp is at the front of the line
                o.Append(buf.Substring(s, stamp.Start - s));
                o.Append(Program.DurationTag);
                o.Append(FormattedTimeSpan(stamp.Offset));
                s = stamp.Start + stamp.Length;
            }
            o.Append(buf.Substring(s, buf.Length - s));

            return o.ToString();
        }


        /// <summary>
        /// Parse all timestamps from a line
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        internal static List<Stamp> ParseLine(string buf)
        {
            var r = new List<Stamp>();

            //regex to find a date stamp and break it down into
            //hh mm ss frac of time
            //and 3 date parts
            var matches = new Regex(@"(?<date>(?<d1>\d+)[\/\-\\_](?<d2>\d+)[\/\-\\_](?<d3>\d+) (?<hour>\d+):(?<minute>\d+):(?<second>\d+)([:\.\,]?(?<frac>\d+))(\s(?<ampm>[AaPp][Mm]))?)").Matches(buf);
            foreach (Match match in matches)
            {
                if (match.Groups.Cast<Group>().FirstOrDefault(g => g.Name == "date") == null) break;

                //get the date parts
                var year = 0;
                var month = 0;
                var day = 0;
                var d1 = int.Parse(match.Groups["d1"].Value);
                var d2 = int.Parse(match.Groups["d2"].Value);
                var d3 = int.Parse(match.Groups["d3"].Value);
                //year will never be in the middle so check if it's clearly the first or last grouping
                if (d1 > 59)
                {
                    year = d1; d1 = 0;
                    if (Program.MonthFirst)
                    { month = d2; d2 = 0; day = d3; d3 = 0; }
                    else
                    { month = d3; d3 = 0; day = d2; d2 = 0; }
                }
                //if it's last, we might have dd/mm or mm/dd
                else if (d3 > 59)
                {
                    year = d3; d3 = 0;
                    if (Program.MonthFirst)
                    { month = d1; d1 = 0; day = d2; d2 = 0; }
                    else
                    { month = d2; d2 = 0; day = d1; d1 = 0; }
                }
                else if (Program.YearFirst)
                {
                    year = d1; d1 = 0;
                    if (Program.MonthFirst)
                    { month = d2; d2 = 0; day = d3; d3 = 0; }
                    else
                    { month = d3; d3 = 0; day = d2; d2 = 0; }
                }
                else
                {
                    year = d3; d3 = 0;
                    if (Program.MonthFirst)
                    { month = d2; d2 = 0; day = d3; d3 = 0; }
                    else
                    { month = d3; d3 = 0; day = d2; d2 = 0; }
                }

                //get time parts
                var hour = int.Parse(match.Groups["hour"].Value ?? "0");
                var minute = int.Parse(match.Groups["minute"].Value ?? "0");
                var second = int.Parse(match.Groups["second"].Value ?? "0");
                //convert fractional part to milliseconds
                var frac = (int)decimal.Floor((decimal.Parse("0." + match.Groups["frac"].Value ?? "0")) * 1000);
                var pm = (match.Groups["ampm"].Value ?? "am").ToLower().Contains("p");
                if (pm == true) hour += 12;
                hour = hour % 24;

                DateTime dt = new DateTime(year, month, day, hour, minute, second, frac);

                if (BaseTimeStamp == null && r.Count == 0)
                {
                    //snag the first timestamp on the line as the baseline
                    BaseTimeStamp = dt;
                    LastTimeStamp = dt;
                }

                if (dt < LastTimeStamp)
                {
                    //this date is a backwards progression, so we'll ignore it
                    continue;
                }

                r.Add(new Stamp()
                {
                    Start = match.Groups["date"].Index,
                    Length = match.Groups["date"].Length,
                    TimeStamp = dt,
                    Offset = dt.Subtract(LastTimeStamp.Value)
                });

                LastTimeStamp = dt;
            }

            return r;
        }


    internal static string FormattedTimeSpan(TimeSpan span)
    {
        const string shortfmt = "hh\\:mm\\:ss";
        const string longfmt = "hh\\:mm\\:ss\\.FFF";

        if (span.Milliseconds > 0)
            return span.ToString(longfmt);

        return span.ToString(shortfmt);
    }
}


internal class Stamp
{
    public int Start { get; set; }
    public int Length { get; set; }
    public DateTime TimeStamp { get; set; }
    public TimeSpan Offset { get; set; }
}
}
