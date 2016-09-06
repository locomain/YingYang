using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.locomain.thread
{
    class Utils
    {
        public static void log(String log)
        {
            #if DEBUG
                Console.WriteLine(log);
            #endif
        }
    }
}
