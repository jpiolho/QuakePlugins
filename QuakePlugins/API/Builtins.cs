using QuakePlugins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    public static class Builtins
    {
        public static void BPrint(string text)
        {
            QEngine.QCRegistersBackup();

            QEngine.QCSetArgumentCount(1);
            QEngine.QCSetStringValue(QEngine.QCValueOffset.Parameter0, text);
            QEngine.BuiltinCall(23);

            QEngine.QCRegistersRestore();
        }


        public static void Localcmd(string command)
        {
            QEngine.QCRegistersBackup();

            QEngine.QCSetArgumentCount(1);
            QEngine.QCSetStringValue(QEngine.QCValueOffset.Parameter0, command);
            QEngine.BuiltinCall(46);

            QEngine.QCRegistersRestore();
        }

        public static void Stuffcmd(int entity,string command)
        {
            QEngine.QCRegistersBackup();

            QEngine.QCSetArgumentCount(2);
            QEngine.QCSetIntValue(QEngine.QCValueOffset.Parameter0, entity);
            QEngine.QCSetStringValue(QEngine.QCValueOffset.Parameter1, command);
            QEngine.BuiltinCall(21);

            QEngine.QCRegistersRestore();
        }
    }
}
