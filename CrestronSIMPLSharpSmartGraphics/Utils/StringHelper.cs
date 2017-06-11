using System;
using Crestron.SimplSharp;
using System.Text.RegularExpressions;

namespace AVPlus
{
    public enum eDebugEventType
    {
        NA,
        Info,
        Notice,
        Ok,
        Warn,
        Error
    }

    public static class StringHelper
    {
        public static void OnDebug(eDebugEventType eventType, string str, params object[] list)
        {
            CrestronConsole.PrintLine(str, list);
            switch (eventType)
            {
                case eDebugEventType.Notice: ErrorLog.Notice(str, list); break;
                case eDebugEventType.Warn  : ErrorLog.Warn  (str, list); break;
                case eDebugEventType.Error : ErrorLog.Error (str, list); break;
            }
        }

        public static int Atoi(string str)
        {
            String m = Regex.Match(str, @"\d+").Value; // get the 2 from "button 2 pressed"
            return m.Length == 0 ? (ushort)0 : Convert.ToInt32(m);
        }
    }
}