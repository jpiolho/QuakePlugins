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
        public static Edict Spawn()
        {
            QEngine.QCRegistersBackup();
            QEngine.BuiltinCall(14);

            var edict = QC.GetEdict(QC.ValueLocation.Return);

            QEngine.QCRegistersRestore();

            return edict;
        }

        public static void BPrint(string text)
        {
            QEngine.QCRegistersBackup();

            QEngine.QCSetArgumentCount(1);
            QEngine.QCSetStringValue(QEngine.QCValueOffset.Parameter0, text);
            QEngine.BuiltinCall(23);

            QEngine.QCRegistersRestore();
        }

        public static void SPrint(Edict edict, string text)
        {
            unsafe
            {
                QEngine.QCRegistersBackup();

                QEngine.QCSetArgumentCount(2);
                QEngine.QCSetEdictValue(QEngine.QCValueOffset.Parameter0, edict.EngineEdict);
                QEngine.QCSetStringValue(QEngine.QCValueOffset.Parameter1, text);
                QEngine.BuiltinCall(24);

                QEngine.QCRegistersRestore();
            }
        }

        public static void Localcmd(string command)
        {
            QEngine.QCRegistersBackup();

            QEngine.QCSetArgumentCount(1);
            QEngine.QCSetStringValue(QEngine.QCValueOffset.Parameter0, command);
            QEngine.BuiltinCall(46);

            QEngine.QCRegistersRestore();
        }

        public static void Stuffcmd(Edict entity, string command)
        {
            unsafe
            {
                QEngine.QCRegistersBackup();

                QEngine.QCSetArgumentCount(2);
                QEngine.QCSetEdictValue(QEngine.QCValueOffset.Parameter0, entity.EngineEdict);
                QEngine.QCSetStringValue(QEngine.QCValueOffset.Parameter1, command);
                QEngine.BuiltinCall(21);

                QEngine.QCRegistersRestore();
            }
        }
    }
}
