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

                    var reader = OpenLog(o.File);
                    var writer = new System.IO.StreamWriter(o.File + ".Shifted");
                    ShiftLog(reader, writer);
                    writer.Close();
                    reader.Close();

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


        internal static TextReader OpenLog(string filename)
        {
            WriteLine($"Loading {filename}...");
            return new StreamReader(System.IO.File.OpenRead(filename));
        }


        internal static DateTime? BaseTimeStamp { get; set; }


        internal static void ShiftLog(TextReader textReader, TextWriter textWriter)
        {
            var r = new StringBuilder();

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

            var stamps = ParseLine(buf);
            if (BaseTimeStamp == null && stamps.Count > 0)
            {
                //snag the first timestamp on the line as the baseline
                BaseTimeStamp = stamps[0].TimeStamp;
            }

            for (int i = stamps.Count - 1; i >= 0; i--)
            {
                var stamp = stamps[i];

                span = stamp.TimeStamp.Subtract(BaseTimeStamp.Value);
                if (span.CompareTo(TimeSpan.Zero) >= 0)
                {
                    //must be positive, if it's negative, we'll assume this datetime is NOT actually a stamp but just data of some sort

                    //put a space unless the stamp is at the front of the line
                    buf = buf.Substring(0, stamp.Start) + (stamp.Start != 0 ? " " : "") + "(TIME)" +
                        FormattedTimeSpan(span) +
                        " " +
                        buf.Substring(stamp.Start + stamp.Length);
                }
            }

            return buf;
        }


        /// <summary>
        /// Parse all timestamps from a line
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
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
    }


    internal class Stamp
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
