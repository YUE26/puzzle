using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.Logger
{
    public static class GameLogger
    {
        public static readonly Color ColorInfo = Color.white;
        public static readonly Color ColorWarning = Color.yellow;
        public static readonly Color ColorError = Color.red;
        public static readonly Color ColorSuccess = Color.green;
        public static readonly Color ColorDebug = Color.cyan;
        public static readonly Color ColorImportant = Color.magenta;

        public static bool EnableLogging = true;
        public static bool EnableStackTrace = true;
        public static bool EnableTimestamp = true;

        public static bool EnableLogUpload = true;
        public static string CloudFlareServerUrl;
        public static float UploadIntervalMinutes;

        private static List<LogEntry> logEntries = new List<LogEntry>();
        public static string uniquePlayerId;
        private static Coroutine uploadCoroutine;
        private static MonoBehaviour coroutineRunner;

        /// <summary>
        /// 日志条目数据结构
        /// </summary>
        [Serializable]
        private class LogEntry
        {
            public string timestamp;
            public string level;
            public string message;
            public string stackTrace;
        }

        /// <summary>
        /// 上传的日志数据结构
        /// </summary>
        [Serializable]
        private class LogUploadData
        {
            public string Owner;
            public string CollectionTime;
            public LogCategoryData Logs;
        }

        /// <summary>
        /// 分类日志数据结构
        /// </summary>
        [Serializable]
        private class LogCategoryData
        {
            public List<LogEntry> Info = new List<LogEntry>();
            public List<LogEntry> Warning = new List<LogEntry>();
            public List<LogEntry> Error = new List<LogEntry>();
            public List<LogEntry> Success = new List<LogEntry>();
            public List<LogEntry> Debug = new List<LogEntry>();
            public List<LogEntry> Important = new List<LogEntry>();
            public List<LogEntry> Custom = new List<LogEntry>();
        }

        /// <summary>
        /// 初始化日志系统
        /// </summary>
        public static void Initialize(MonoBehaviour runner)
        {
            coroutineRunner = runner;
            Application.logMessageReceived += HandleUnityLog;
            if (EnableLogUpload && uploadCoroutine == null)
            {
                uploadCoroutine = coroutineRunner.StartCoroutine(LogUploadRoutine());
            }
        }

        /// <summary>
        /// 停止日志系统
        /// </summary>
        public static void Shutdown()
        {
            // 注销Unity日志回调
            Application.logMessageReceived -= HandleUnityLog;

            if (uploadCoroutine != null && coroutineRunner != null)
            {
                coroutineRunner.StopCoroutine(uploadCoroutine);
                uploadCoroutine = null;
            }

            // 上传剩余日志
            if (EnableLogUpload && logEntries.Count > 0)
            {
                coroutineRunner?.StartCoroutine(UploadLogsToServer());
            }
        }

        /// <summary>
        /// 处理Unity系统日志回调
        /// </summary>
        private static void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            if (!EnableLogUpload) return;

            // 将Unity的LogType映射到我们的日志级别
            string level;
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    level = "Error";
                    break;
                case LogType.Assert:
                    level = "Error";
                    break;
                case LogType.Warning:
                    level = "Warning";
                    break;
                case LogType.Log:
                    level = "Info";
                    break;
                default:
                    level = "Custom";
                    break;
            }

            // 添加到日志收集列表
            LogEntry entry = new LogEntry
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                level = level,
                message = logString,
                stackTrace = EnableStackTrace ? stackTrace : null
            };

            logEntries.Add(entry);

            // 防止内存溢出，限制日志数量
            if (logEntries.Count > 1000)
            {
                logEntries.RemoveRange(0, 200); // 移除最旧的200条日志
            }
        }

        /// <summary>
        /// 定时上传协程
        /// </summary>
        private static IEnumerator LogUploadRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(UploadIntervalMinutes * 60f);

                if (logEntries.Count > 0)
                {
                    yield return UploadLogsToServer();
                }
            }
        }

        /// <summary>
        /// 上传日志到服务器
        /// </summary>
        private static IEnumerator UploadLogsToServer()
        {
            if (!EnableLogUpload || string.IsNullOrEmpty(CloudFlareServerUrl) || logEntries.Count == 0)
                yield break;

            if (string.IsNullOrEmpty(uniquePlayerId))
            {
                yield break;
            }

            LogUploadData uploadData = new LogUploadData
            {
                Owner = uniquePlayerId,
                CollectionTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Logs = CategorizeLogEntries()
            };

            string jsonData = JsonUtility.ToJson(uploadData, true);

            // 创建HTTP请求
            using (UnityWebRequest request = new UnityWebRequest(CloudFlareServerUrl, "POST"))
            {
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    UnityEngine.Debug.Log(
                        $"<color=green>[GameLogger] Successfully uploaded {logEntries.Count} log entries to server</color>");
                    logEntries.Clear();
                }
                else
                {
                    UnityEngine.Debug.LogError($"<color=red>[GameLogger] Failed to upload logs: {request.error}</color>");
                }
            }
        }

        /// <summary>
        /// 将日志条目按类别分组
        /// </summary>
        private static LogCategoryData CategorizeLogEntries()
        {
            LogCategoryData categorizedLogs = new LogCategoryData();

            foreach (var entry in logEntries)
            {
                switch (entry.level.ToLower())
                {
                    case "info":
                        categorizedLogs.Info.Add(entry);
                        break;
                    case "warning":
                        categorizedLogs.Warning.Add(entry);
                        break;
                    case "error":
                        categorizedLogs.Error.Add(entry);
                        break;
                    case "success":
                        categorizedLogs.Success.Add(entry);
                        break;
                    case "debug":
                        categorizedLogs.Debug.Add(entry);
                        break;
                    case "important":
                        categorizedLogs.Important.Add(entry);
                        break;
                    default:
                        categorizedLogs.Custom.Add(entry);
                        break;
                }
            }

            return categorizedLogs;
        }

        /// <summary>
        /// 添加日志条目到收集列表
        /// </summary>
        private static void AddLogEntry(string level, string message, string stackTrace = null)
        {
            if (!EnableLogUpload) return;

            LogEntry entry = new LogEntry
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                level = level,
                message = message,
                stackTrace = EnableStackTrace ? stackTrace : null
            };

            logEntries.Add(entry);

            // 防止内存溢出，限制日志数量
            if (logEntries.Count > 1000)
            {
                logEntries.RemoveRange(0, 200); // 移除最旧的200条日志
            }
        }

        /// <summary>
        /// 普通信息日志（支持格式化字符串）
        /// </summary>
        public static void Info(string format, params object[] args)
        {
            string message = LogInternal(format, args, ColorInfo, false);
            AddLogEntry("Info", message, EnableStackTrace ? Environment.StackTrace : null);
        }

        /// <summary>
        /// 警告日志（支持格式化字符串）
        /// </summary>
        public static void Warning(string format, params object[] args)
        {
            string message = LogInternal(format, args, ColorWarning, false);
            AddLogEntry("Warning", message, EnableStackTrace ? Environment.StackTrace : null);
        }

        /// <summary>
        /// 错误日志（支持格式化字符串）
        /// </summary>
        public static void Error(string format, params object[] args)
        {
            string message = LogInternal(format, args, ColorError, true);
            AddLogEntry("Error", message, EnableStackTrace ? Environment.StackTrace : null);
        }

        /// <summary>
        /// 成功日志（支持格式化字符串）
        /// </summary>
        public static void Success(string format, params object[] args)
        {
            string message = LogInternal(format, args, ColorSuccess, false);
            AddLogEntry("Success", message, EnableStackTrace ? Environment.StackTrace : null);
        }

        /// <summary>
        /// 调试日志（支持格式化字符串，仅在开发版本输出）
        /// </summary>
        public static void Debug(string format, params object[] args)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            string message = LogInternal(format, args, ColorDebug, false);
            AddLogEntry("Debug", message, EnableStackTrace ? Environment.StackTrace : null);
#endif
        }

        /// <summary>
        /// 重要日志（支持格式化字符串）
        /// </summary>
        public static void Important(string format, params object[] args)
        {
            string message = LogInternal(format, args, ColorImportant, false);
            AddLogEntry("Important", message, EnableStackTrace ? Environment.StackTrace : null);
        }

        /// <summary>
        /// 自定义颜色日志（支持格式化字符串）
        /// </summary>
        public static void Custom(Color color, string format, params object[] args)
        {
            string message = LogInternal(format, args, color, false);
            AddLogEntry("Custom", message, EnableStackTrace ? Environment.StackTrace : null);
        }

        /// <summary>
        /// 内部日志处理核心方法
        /// </summary>
        private static string LogInternal(string format, object[] args, Color color, bool isError)
        {
            if (!EnableLogging) return "";

            try
            {
                string formattedMessage = FormatMessage(format, args);
                string finalMessage = AddTimestamp(formattedMessage);
                string coloredMessage = ColorizeMessage(finalMessage, color);

                // 添加堆栈跟踪信息（可选）
                if (EnableStackTrace)
                {
                    string stackTrace = Environment.StackTrace;
                    coloredMessage += $"\nStackTrace: {stackTrace}";
                }

                // 根据是否为错误选择不同的日志方法
                if (isError)
                {
                    UnityEngine.Debug.LogError(coloredMessage);
                }
                else
                {
                    UnityEngine.Debug.Log(coloredMessage);
                }

                return formattedMessage; // 返回未着色的消息用于上传
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Logger error: {ex.Message}");
                return format; // 返回原始格式字符串
            }
        }

        /// <summary>
        /// 格式化消息（支持格式化字符串）
        /// </summary>
        private static string FormatMessage(string format, object[] args)
        {
            if (string.IsNullOrEmpty(format))
                return "[Empty Message]";

            if (args == null || args.Length == 0)
                return format;

            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                // 如果格式化失败，回退到简单拼接
                StringBuilder sb = new StringBuilder();
                sb.Append(format);
                sb.Append(" [");
                for (int i = 0; i < args.Length; i++)
                {
                    sb.Append(args[i]?.ToString() ?? "[Null]");
                    if (i < args.Length - 1)
                        sb.Append(", ");
                }

                sb.Append("]");
                return sb.ToString();
            }
        }

        /// <summary>
        /// 添加时间戳
        /// </summary>
        private static string AddTimestamp(string message)
        {
            if (!EnableTimestamp) return message;

            return $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        }

        /// <summary>
        /// 为消息添加颜色标记
        /// </summary>
        private static string ColorizeMessage(string message, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>";
        }

        /// <summary>
        /// 记录异常信息
        /// </summary>
        public static void Exception(Exception ex, string context = null)
        {
            if (!EnableLogging) return;

            string message = string.IsNullOrEmpty(context) ? ex.ToString() : $"{context}\n{ex}";
            string finalMessage = AddTimestamp(message);
            UnityEngine.Debug.LogError(ColorizeMessage(finalMessage, ColorError));

            AddLogEntry("Error", message, ex.StackTrace);
        }

        /// <summary>
        /// 性能测试方法
        /// </summary>
        public static IDisposable MeasureTime(string operationName)
        {
            return new TimeMeasurer(operationName);
        }

        /// <summary>
        /// 手动触发日志上传
        /// </summary>
        public static void ForceUploadLogs()
        {
            if (coroutineRunner != null && logEntries.Count > 0)
            {
                coroutineRunner.StartCoroutine(UploadLogsToServer());
            }
        }

        /// <summary>
        /// 获取当前收集的日志数量
        /// </summary>
        public static int GetPendingLogCount()
        {
            return logEntries.Count;
        }

        /// <summary>
        /// 获取玩家唯一ID
        /// </summary>
        public static string GetUniquePlayerId()
        {
            return uniquePlayerId;
        }

        /// <summary>
        /// 时间测量辅助类
        /// </summary>
        private class TimeMeasurer : IDisposable
        {
            private readonly string _operationName;
            private readonly System.Diagnostics.Stopwatch _stopwatch;

            public TimeMeasurer(string operationName)
            {
                _operationName = operationName;
                _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                if (EnableLogging)
                {
                    Custom(Color.blue, $"[Performance] {_operationName} took {_stopwatch.ElapsedMilliseconds}ms");
                }
            }
        }
    }
}