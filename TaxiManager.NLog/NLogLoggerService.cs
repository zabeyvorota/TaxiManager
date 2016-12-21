using System;

using NLog;

namespace TaxiManager.NLog
{
    public class NLogLoggerService : Core.ILogger
    {
        private readonly Logger _logger;

        public NLogLoggerService()
        {
            _logger = LogManager.GetLogger("gsdn");
        }

        public NLogLoggerService(string logName)
        {
            _logger = LogManager.GetLogger(logName);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Warning(string message)
        {
            _logger.Warn(message);
        }

        public void Exception(Exception ex)
        {
            _logger.Error(ex);
        }
    }
}
