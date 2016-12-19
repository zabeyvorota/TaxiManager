
using System;

namespace TaxiManager.Data.Model
{
    /// <summary>
    /// Базовый класс сущноссти
    /// </summary>
    public abstract class Entity
    {    
        /// <summary>
        /// Свойство задает и возвращает первичный ключ (нужен для быстрой вставки РСУБД)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Свойство задает и возвращает идентификатор сущности в системе
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Свойство задает и возвращает время создания сущноссти
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Свойство задает и возвращает время обновления сущноссти
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Свойство задает и возвращает маркер удаления сущноссти в системе
        /// </summary>
        public bool IsDelete { get; set; }
    }
}
