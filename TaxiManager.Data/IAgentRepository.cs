using System;
using System.Collections.Generic;

using TaxiManager.Data.Model;

namespace TaxiManager.Data
{
    /// <summary>
    /// Интерфейс описывает репозиторий доступа к сущности агента
    /// </summary>
    public interface IAgentRepository
    {
        /// <summary>
        /// Метод добавляет нового или обновляет существующего агента
        /// </summary>
        Agent AddOrUpdateAgent(Guid agentGuid, Agent agent);
        
        /// <summary>
        /// Метод удаляет существующего агента
        /// </summary>
        void DeleteAgent(Guid agentGuid, Agent agent);

        /// <summary>
        /// Метод возвращает список агентов по идентификаторам
        /// </summary>
        /// <param name="guids"></param>
        /// <returns></returns>
        IList<Agent> GetAgentsByGuids(IList<Guid> guids);
    }
}
