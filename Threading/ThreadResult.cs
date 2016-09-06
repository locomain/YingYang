using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.locomain.thread.Threading
{
    public interface ThreadResult
    {
        bool onEndResult(BalanceThread thread);
        void release(BalanceThread thread);
    }
}
