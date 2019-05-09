using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BlazorSourceMangler.Helpers
{
    public static class TimeAnalyzer
    {

        static Stopwatch sw = new Stopwatch();

        private static  void Start()
        {
            sw = Stopwatch.StartNew();
        }


        public static TimeSpan Stop()
        {
            TimeSpan ts = sw.Elapsed;
            sw.Stop();
            return ts;
        }



        public static void StartTimeCounter()
        {
            Start();
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("===============process started===============");
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff"));
            Console.WriteLine(Environment.NewLine);
        }


        public static void FinishTimeCounter()
        {
            TimeSpan ts = Stop();
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("===============process Finished===============");
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff"));
            Console.WriteLine("Duration: "+ts.ToString(@"hh\:mm\:ss\.fff"));
            Console.WriteLine(Environment.NewLine);
        }

    }
}
