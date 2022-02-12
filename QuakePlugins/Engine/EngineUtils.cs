using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Engine
{
    internal static class EngineUtils
    {
        public static int QCGetReturnStatementIndex()
        {
            unsafe
            {
                int runaway = 100000;
                int index = 0;
                while (runaway-- > 0)
                {
                    var statement = QEngine.QCGetStatement(index++);
                    if (statement->op == 43) // OP_RETURN
                        return index;
                }
            }

            return 0;
        }
    }
}
