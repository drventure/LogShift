using System;
using System.Collections.Generic;
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
        private static bool _console = false;

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    _console = true;
                    WriteLine("LogShift");
                    WriteLine("(c) 2020 drVenture");
                    WriteLine("Translate all datetime stamps in log file back to 0 based on first detected datetime stamp.");
                    WriteLine("");
                    var buf = LoadLog(o.File);
                    buf = ShiftLog(buf);
                    SaveLog(o.File + ".Shifted", buf);

                    WriteLine("Done");
                });
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


        internal static string LoadLog(string filename)
        {
            WriteLine($"Loading {filename}...");
            return System.IO.File.ReadAllText(filename);
        }


        internal static string ShiftLog(string buf)
        {
            var r = new StringBuilder();

            var lines = buf.Split("\r\n");


            DateTime? firstDt = null;
            var c = 0;

            foreach (var l in lines)
            {
                c++;
                var b = l;
                TimeSpan span = TimeSpan.Zero;

                var stamps = ParseLine(b);
                if (firstDt == null && stamps.Count > 0)
                {
                    //snag the first timestamp on the line as the baseline
                    firstDt = stamps[0].TimeStamp;
                }

                for (int i = stamps.Count - 1; i >= 0; i--)
                {
                    var stamp = stamps[i];

                    span = stamp.TimeStamp.Subtract(firstDt.Value);
                    if (span.CompareTo(TimeSpan.Zero) >= 0)
                    {
                        //must be positive, if it's negative, we'll assume this datetime is NOT actually a stamp but just data of some sort

                        //put a space unless the stamp is at the front of the line
                        b = b.Substring(0, stamp.Start) + (stamp.Start != 0 ? " " : "") + "(TIME)" +
                            FormattedTimeSpan(span) +
                            " " +
                            b.Substring(stamp.Start + stamp.Length);
                    }
                }

                Write($"Processing line: {c}");
                ResetLeft();

                r = r.AppendLine(b);
            }

            WriteLine();

            return r.ToString();
        }


        internal static List<Stamp> ParseLine(string buf)
        {
            var r = new List<Stamp>();

            var matches = new Regex(@"(?<date>\d?\d[\/\-\\_]\d?\d[\/\-\\_](\d\d)?\d\d \d?\d:\d\d:\d\d[:\.]?[\d]*(\s[AaPp][Mm]))").Matches(buf);
            foreach (Match match in matches)
            {
                if (match.Groups.Cast<Group>().FirstOrDefault(g => g.Name == "date") == null) break;

                DateTime dt;
                if (DateTime.TryParse(match.Groups["date"].Value, out dt))
                {
                    r.Add(new Stamp()
                    {
                        Start = match.Groups["date"].Index,
                        Length = match.Groups["date"].Length,
                        TimeStamp = dt
                    });
                }
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


        internal static void SaveLog(string filename, string buf)
        {
            WriteLine("Saving...");
            System.IO.File.WriteAllText(filename, buf);
        }
    }


    internal class Stamp
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
