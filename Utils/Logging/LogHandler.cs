/*
 * Imported code from older Flogger logging:https://github.com/sorrynofocus/flogger
  
             //Declare the Flogger object
            Logger logger = new Logger();

            //Set the logfile and disable it (we don't want logging to start yet)
            logger.LogFile = "c:\\temp\\mylogfile.txt";
            logger.EnableLogger = false;

            //Enable Date/Time stamps - true
            logger.EnableDTStamps = true;

            // To log, you can send a List<string> or a <string> to it.

            // List<string> example:
             List<string> LogMesg = new List<string>
                                    (
                                        new string[]
                                            {
                                                "Example 1",
                                                "Example 2",
                                                "Examepl 3"
                                            }
                                    );
            
             logger.LogInfo (LogMesg);
            
            //
            // OR send a simple string:
            //            
            logger.LogInfo ("Logging this to the logger);
 */


using System;

namespace com.nobodynoze.flogger
{
    public delegate void LogEventHandler(object source, LogEventArgs e);

    public class LogEventArgs : EventArgs
    {
        public string LogMessage { get; }

        public LogEventArgs(string message)
        {
            LogMessage = message;
        }
    }

    public class Logger
    {
        public event LogEventHandler LogWritten;

        //Private string to define log file
        private static string sLogFile = string.Empty;

        //Enable data time stamp during printing of log.
        private bool bEnableDTStamp = false;

        /// <summary>
        /// Logging levels DEBUG, INFO, WARN, ERROR, FATAL, TRACE, and ALL will provide logging info based off difficulty level.
        /// DEBUG (0): Additional information about application behavior for cases when that information is necessary to diagnose problems
        /// 
        /// INFO (1): Application events for general purposes
        /// 
        /// WARN (2): Application events that may be an indication of a problem
        /// 
        /// ERROR (3): Typically logged in the catch block a try/catch block, includes the exception and contextual data
        /// 
        /// FATAL (4): A critical error that results in the termination of an application
        /// 
        /// TRACE (5): Used to mark the entry and exit of functions, for purposes of performance profiling
        /// 
        /// ALL (6): All log info
        /// </summary>
        public enum LogDifficultyLvl
        {
            DEBUG,
            INFO,
            WARN,
            ERROR,
            FATAL,
            TRACE,
            ALL
        }

        /// <summary>
        /// Property logfile get/set
        /// </summary>
        public string LogFile
        {
            //Expression-bodied member 
            get => sLogFile;
            set { sLogFile = value; }
        }

        /// <summary>
        /// Property enable date/time stamps
        /// </summary>
        public bool EnableDTStamps
        {
            get => bEnableDTStamp;
            set { bEnableDTStamp = value; }
        }

        /// <summary>
        /// Formnat log data to make it readable.
        /// </summary>
        /// <param name="LogMsg"></param>
        /// <param name="LogDiffLvl"></param>
        /// <returns></returns>
        /// Log data looks like this
        /// You can see if we enable timestamps, then any other messages between our log will try to group it together. 
        ///[ 4/3/2021 7:25:04 PM  DEBUG ] The quick brown fox jumps over the lazy dog.
        ///                       [DEBUG] FUNC: SendTextToLog
        ///                       [DEBUG] SRC : C:\Users\<USER>\source\repos\FloggerTest\FloggerTest\MainForm.cs
        ///[4 / 3 / 2021 7:25:04 PM  DEBUG] LINE: 102

        ///This is formatted without timestamps.
        ///[DEBUG] Test1
        ///[DEBUG] This is example text to dump to logging
        ///[DEBUG] Another LOGGING TEXT test
        ///[DEBUG]     <- spaces added! This space in list!
        ///[DEBUG] This     is   also s P a c e    d  !
        ///[DEBUG] The quick brown fox jumped over the lazy dog
        ///[DEBUG][WARNING]! Help!
        ///[DEBUG] FUNC: DumpTextToLog
        ///[DEBUG] SRC : C:\Users\<USER>\source\repos\FloggerTest\FloggerTest\MainForm.cs

        private IEnumerable<string> FormatLogData(List<string> LogMsg, LogDifficultyLvl LogDiffLvl)
        {
            //            int iListCount = LogMsg.Count();
            //            int remove = Math.Min(LogMsg.Count, 1);

            string MessageFormat = "";
            int DTcount = DateTime.Now.ToString().Length;

            foreach (string curItem in LogMsg)
            {
                //Could not get string.format or PadLEft, right to work, so did it hard way.

                //Should we come across first item in the log message list, format it
                //with date/time stamp and everything between, don't (if date/time stamp is enabled)
                if (curItem == LogMsg.First())
                {
                    //Do something with the first item
                    if (EnableDTStamps)
                        MessageFormat = "[ " + DateTime.Now.ToString() + "  " + LogDiffLvl.ToString() + " ] " + curItem;
                    else
                        MessageFormat = "[ " + LogDiffLvl.ToString() + " ] " + curItem;
                }
                else if (curItem == LogMsg.Last())
                {
                    //Do something with the last item
                    if (EnableDTStamps)
                        MessageFormat = "[ " + DateTime.Now.ToString() + "  " + LogDiffLvl.ToString() + " ] " + curItem;
                    else
                        MessageFormat = "[ " + LogDiffLvl.ToString() + " ] " + curItem;
                }
                else
                {
                    //if time stamps enabled , format it to fit within the group of timestamped messages.
                    if (EnableDTStamps)
                        MessageFormat = "[ " + LogDiffLvl.ToString().PadLeft((DTcount * 2) + 3, (char)32) + " ] " + curItem;
                    else
                        //without time stamps, try to format to fit plain messages.
                        MessageFormat = "    [ " + LogDiffLvl.ToString() + " ] " + curItem;
                }
                yield return (MessageFormat);
            }

        } //End of FormatLogData() func


        /// <summary>
        /// General logger (list mode).
        /// </summary>
        /// <param name="Message"></param>
        private void Log(List<string> Message, LogDifficultyLvl LogDiffLvl)
        {
            foreach (string pItem in FormatLogData(Message, LogDiffLvl))
            {
                if (pItem != "")
                    WriteLog(pItem, LogDiffLvl);
            }
        }

        //Log specifics, debug, warn, info, etc.
        public void LogInfo(string sMessage)
        {

            List<string> lMessage = new List<string>
                                        (
                                            new string[]
                                                {
                                                    sMessage
                                                }
                                        );
            Log(lMessage, LogDifficultyLvl.INFO);
        }


        public void LogInfo(List<string> lMessage)
        {
            Log(lMessage, LogDifficultyLvl.INFO);
        }


        public void WriteLog(string sLogMsg, LogDifficultyLvl LogDiffLvl)
        {
            ///Let's do the file logging...

            //Check if logfile is null, if it is, return
            if (string.IsNullOrEmpty(LogFile))
                return;

            using (System.IO.FileStream pFile = new System.IO.FileStream(LogFile,
                                                                         System.IO.FileMode.Append,
                                                                         System.IO.FileAccess.Write,
                                                                         System.IO.FileShare.ReadWrite))
            {
                System.IO.StreamWriter pStream = new System.IO.StreamWriter(pFile);

                //if (EnableDTStamps)
                //    pStream.Write(DateTime.Now.ToString() + ": " + sLogMsg + "\r");
                //else
                //    pStream.Write(sLogMsg + "\r");
                pStream.Write(sLogMsg + "\r");

                pFile.Flush();
                pStream.Flush();
                pStream.Close();
                pFile.Close();
            }

            // Trigger the event if a handler is present
            LogWritten?.Invoke(this, new LogEventArgs(sLogMsg));
        }
    }

}

