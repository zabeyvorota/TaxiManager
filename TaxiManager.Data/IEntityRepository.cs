using System;
using System.Collections.Generic;

using TaxiManager.Data.Model;

namespace TaxiManager.Data
{
    /// <summary>
    /// Интерфейс описывающий репозиторий связий сущностей
    /// </summary>
    public interface IEntityRepository
    {
        /// <summary>
        /// Функция возвращает список идентификаторов сущностей по заданному идентификатору агента и типу сущности
        /// </summary>
        /// <param name="agentGuid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IList<Guid> GetEntitys(Guid agentGuid, EntityType type);

        /// <summary>
        /// Свойство добавляет новую связь с сущность в систему
        /// </summary>
        /// <param name="agentGuid"></param>
        /// <param name="newGuid"></param>
        /// <param name="type"></param>
        void AddEntity(Guid agentGuid, Guid newGuid, EntityType type);

        /// <summary>
        /// Функция удаляет связь агента с заданной сущностью
        /// </summary>
        /// <param name="agentGuid"></param>
        /// <param name="delGuid"></param>
        /// <param name="type"></param>
        void DeleteEntity(Guid agentGuid, Guid delGuid, EntityType type);
    }
}
