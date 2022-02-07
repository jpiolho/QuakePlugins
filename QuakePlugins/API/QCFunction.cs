using QuakePlugins.Engine;
using QuakePlugins.Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    /// <apiglobal />
    public class QCFunction
    {
        private unsafe EngineQCFunction* _pointer;

        /// <summary>
        /// Index of the QC function
        /// </summary>
        public int Index
        {
            get { unsafe { return QEngine.QCFunctionPointerToIndex(_pointer); } }
        }

        internal unsafe QCFunction(EngineQCFunction* engineFunctionPointer)
        {
            _pointer = engineFunctionPointer;
        }

        /// <summary>
        /// Invoke the QC function, with any parameters.
        /// </summary>
        public void Call(params object[] parameters)
        {
            unsafe
            {
                QEngine.QCRegistersBackup();

                Utils.SetQCGenericParameters(parameters);

                QEngine.QCCallFunction(_pointer);

                QEngine.QCRegistersRestore();
            }
        }
    }
}
