using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SQLCC.Core.Helpers
{
    public class DataScrubber
    {
        private readonly DbTraceCodeFormatter _traceCodeFormatter;
        private static Dictionary<string, string[]> _scrubFiles;
        private string _scrubFile;
        
        public DataScrubber(DbTraceCodeFormatter traceCodeFormatter, string scrubFile)
        {
            _traceCodeFormatter = traceCodeFormatter;
            _scrubFile = scrubFile;

            // Create a dictionary to hold the different scrub files, and so we only read each file once to reduce disk IO.
            if (_scrubFiles == null)
                _scrubFiles = new Dictionary<string, string[]>();
            if (!_scrubFiles.ContainsKey(scrubFile))
                _scrubFiles.Add(scrubFile, System.IO.File.ReadAllLines(scrubFile));
        }

        public string Scrub(string code, string[] scrubArray)
        {
            if (code == null)
                return null;

            var finalCode = code;

            if (scrubArray.Length > 1)
            {
                // Set a timeout for the regex so that we know when we have a problem with a specific object and regex
                var timeOut = new System.TimeSpan(0, 0, 30);

                scrubArray[1] = scrubArray[1].Replace("\\n", "\n");

                try
                {
                    finalCode = Regex.Replace(finalCode, string.Format(scrubArray[0], _traceCodeFormatter.StartHighlightMarkUp, _traceCodeFormatter.EndHighlightMarkUp), string.Format(scrubArray[1], _traceCodeFormatter.StartHighlightMarkUp, _traceCodeFormatter.EndHighlightMarkUp), RegexOptions.IgnoreCase, timeOut);
                }
                catch (RegexMatchTimeoutException ex)
                {
                    System.Console.WriteLine("Timeout occurred during regex scrubbing after {0} hr {1} min {2} secs.", timeOut.Hours, timeOut.Minutes, timeOut.Seconds);
                    System.Console.WriteLine("Error:  " + ex.Message);
                    System.Console.WriteLine("scrubArray[0] = " + scrubArray[0]);
                    System.Console.WriteLine("scrubArray[1] = " + scrubArray[1]);
                    System.Console.WriteLine("scrubArray[2] = " + scrubArray[2]);
                    throw;
                }
            }
            return finalCode;
        }

        public string Scrub(string code)
        {
            if (code == null)
                return null;

            var finalCode = code;

            foreach (var scrubLine in _scrubFiles[_scrubFile])
            {
                var scrubText = scrubLine.Split('\t');
                finalCode = Scrub(finalCode, scrubText);
            }
            return finalCode;
        }
    }
}
