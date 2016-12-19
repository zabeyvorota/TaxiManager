using System;

namespace TaxiManager.Core
{
    /// <summary>
    /// Интерфейс описывает методы доступа к логированию
    /// </summary>
    public interface ILogger
    {   
        /// <summary>
        /// Функция логирует сообщение 
        /// </summary>
        /// <param name="message">Сообщение</param>
        void Info(string message);

        /// <summary>
        /// Функция логирует важное сообщение
        /// </summary>
        /// <param name="message">Сообщение</param>
        void Warning(string message);

        /// <summary>
        /// Функция логирует исключение
        /// </summary>
        /// <param name="ex">Исключение</param>
        void Exception(Exception ex);

    }
}
