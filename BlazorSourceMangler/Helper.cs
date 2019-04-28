using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BlazorSourceMangler
{
    internal static class Helper
    {

        static int start = 'a' - 1;
        static int nxt = 0;
        static int rem = 0;
        static StringBuilder str = new StringBuilder();

        internal static string GetCode(int number, MethodBase mb)
        {
            Stat.DoStat(mb);

            str = new StringBuilder();

            if (number == 0)
            {
                number=1;
            }

            if (number <= 26)
            {
                return ((char)(number + start)).ToString();
            }

            
            nxt = number;

            

            while (nxt != 0)
            {
                rem = nxt % 26;
                if (rem == 0) rem = 26;

          
                str.Append((char)(rem + start));

                nxt /= 26;

                if (rem == 26) nxt -= 1;
            }

            return str.ToString();
        }




    }
}
