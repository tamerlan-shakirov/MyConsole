/* ================================================================
   ---------------------------------------------------
   Project   :    MyConsole
   Publisher :    Renowned Games
   Developer :    Tamerlan Shakirov
   ---------------------------------------------------
   Copyright 2022 Renowned Games All rights reserved.
   ================================================================ */

using System;
using UnityEngine;

namespace RenownedGames.MyConsoleEditor
{
    [Serializable]
    public struct Log : IEquatable<Log>
    {
        private readonly static ConsoleSettings Settings = ConsoleSettings.GetCurrent();

        private string condition;
        private string stackTrace;
        private LogType logType;
        private string time;

        public Log(string condition, string stackTrace, LogType logType)
        {
            this.condition = condition;
            this.stackTrace = stackTrace;
            this.logType = logType;

            string format = Settings.GetLogTimeFormat();
            time = DateTime.Now.ToString(format);
        }

        #region [IEquatable<Log> Implementation]
        public bool Equals(Log other)
        {
            return logType == other.logType
                && condition == other.condition
                && stackTrace == other.stackTrace;
        }

        public override int GetHashCode()
        {
            return (logType, condition, stackTrace).GetHashCode();
        }
        #endregion

        #region [Getter / Setter]
        public string GetCondition()
        {
            return condition;
        }

        public void SetCondition(string value)
        {
            condition = value;
        }

        public string GetStackTrace()
        {
            return stackTrace;
        }

        public void SetStackTrace(string value)
        {
            stackTrace = value;
        }

        public LogType GetLogType()
        {
            return logType;
        }

        public void SetLogType(LogType value)
        {
            logType = value;
        }

        public string GetTime()
        {
            return time;
        }

        public void SetTime(string value)
        {
            time = value;
        }
        #endregion
    }
}