using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#nullable enable
namespace ConnieSDK
{
    public class Logger : MonoBehaviour
    {
        private static Logger? Singleton;

        [SerializeField]
        private string FileName = "ConnieSDK";
        public string OutputFile => Path.Join(".", $"{FileName}.log");

        private static Stream? FileStream;
        private static StreamWriter? Writer;
        private static bool Active = false;
        private static bool Quitting = false;

        private void Awake()
        {
            if (Singleton is Logger)
            {
                enabled = false;
                return;
            }

            Singleton = this;

            Init(OutputFile);

            DontDestroyOnLoad(this);
        }

        private void OnDestroy()
        {
            if (Quitting)
                return;

            LogWarning("Logger object has been destroyed! Closing log streams.");

            Singleton = null;
            Shutdown();
        }

        private void OnApplicationQuit()
        {
            Log("Application quitting, closing log streams.");

            Singleton = null;
            Quitting = true;
            Shutdown();
        }

        /// <summary>
        /// Initializes the static Logger behavior
        /// </summary>
        /// <param name="output">The file to output logged messages to</param>
        public static void Init(string output)
        {
            if (Active)
                LogException("Attempting to activate a second log! Call Shutdown() before calling Init() again.");

            if (!File.Exists(output))
                File.Create(output).Close();

            FileStream = File.Open(output, FileMode.Truncate, FileAccess.Write, FileShare.Read);
            Writer = new StreamWriter(FileStream);

            Writer.AutoFlush = true;

            Active = true;
            Log($"CVW5 Logger started at {Path.GetFullPath(output)}");
        }

        public static void Shutdown ()
        {
            if (!Active)
                throw new System.Exception("Attempted to shutdown a log without one active! Call Init() before calling Shutdown().");

            Log("Logging stopped (deliberate)");

            Writer?.Flush();
            Writer?.Close();

            FileStream?.Close();
            Active = false;
        }

        private static void PrintMessage (string message, LogType type = LogType.Log)
        {
            string DateTimeString = $"{System.DateTime.Now:HH:mm:ss.ffff}".PadRight(16);
            string typeString = $"{type}".PadRight(10);

            string FormattedMessage = $"{DateTimeString}{typeString}{message}";
            Writer?.WriteLineAsync(FormattedMessage);
            
        }

        public static void Write(string message, LogType Priority = LogType.Log, bool LogToUnity = false)
        {
            switch (Priority)
            {
                case LogType.Log:
                    Log(message, LogToUnity);
                    break;
                case LogType.Warning:
                    LogWarning(message, LogToUnity);
                    break;
                case LogType.Error:
                    LogError(message, LogToUnity);
                    break;
                case LogType.Exception:
                    LogException(message, LogToUnity);
                    break;
                default:
                    Debug.LogError($"The ConnieSDK Logger.Write() function does not support logging of {Priority}s at this time. Inner log message:\n{message}");
                    break;
            };
        }

        public static void Log (string message, bool LogToUnity = true)
        {
            PrintMessage(message, LogType.Log);

            if(LogToUnity)
                Debug.Log(message);
        }

        public static void LogWarning (string message, bool LogToUnity = true)
        {
            PrintMessage(message, LogType.Warning);

            if (LogToUnity)
                Debug.LogWarning(message);
        }

        public static void LogError(string message, bool LogToUnity = true)
        {
            PrintMessage(message, LogType.Error);

            if (LogToUnity)
                Debug.LogError(message);
        }

        public static void LogException (string message, bool LogToUnity = true)
        {
            PrintMessage(message, LogType.Exception);

            if (LogToUnity)
                Debug.LogException(new System.Exception(message));
        }
    }
}
