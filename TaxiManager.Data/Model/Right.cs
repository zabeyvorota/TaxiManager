using System;

namespace TaxiManager.Data.Model
{
    /// <summary>
    /// Класс описывающий права пользователя
    /// </summary>
    public sealed class Right
    {
        /// <summary>
        /// Свойство задает и возвращает первичный ключ (нужен для быстрой вставки РСУБД)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Свойство задающее и возвращающее идентификатор пользователя
        /// </summary>
        public Guid AgentGuid { get; set; }

        /// <summary>
        /// Идентификатор отвечающий за тип сущности в системе
        /// </summary>
        public EntityType EntityType { get; set; }

        /// <summary>
        /// Идентификатор отображающий права системы
        /// </summary>
        public OperationType[] OperationTypes { get; set; }

        /// <summary>
        /// Свойство задает и возвращает время создания связи
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Свойство задает и возвращает время обновления связи
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
