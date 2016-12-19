using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.Data.Model
{
    /// <summary>
    /// класс  описывающий связи сущностей с агентами
    /// </summary>
   public class EntityGuids
    {  
        /// <summary>
        /// Свойство задает и возвращает первичный ключ (нужен для быстрой вставки РСУБД)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Свойство задает и возвращает маркер удаления сущноссти в системе
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// Свойство задает и возвращает идентификатор сущности
        /// </summary>
        public Guid EntityGuid { get; set; }

        /// <summary>
        /// Свойство задает и возвращает идентификатор агента которому доступна сущность
        /// </summary>
        public Guid AgentGuid { get; set; }

        /// <summary>
        /// Свойство задает и возвращает тип сущности
        /// </summary>
        public EntityType EntityType { get; set; }
       
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
