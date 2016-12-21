using System;
using System.Collections.Generic;

using TaxiManager.Data.Model;

namespace TaxiManager.Data
{
    /// <summary>
    /// Интерфейс описывает репозиторий доступа к сущности водитель
    /// </summary>
    public interface IDriverRepository
    {
        /// <summary>
        /// Метод добавляет нового или обновляет существующего водителя
        /// </summary>
        Driver AddOrUpdateDriver(Guid agentGuid, Driver driver);

        /// <summary>
        /// Метод удаляет существующего водителя
        /// </summary>
        void DeleteDriver(Guid agentGuid, Driver driver);

        /// <summary>
        /// Метод возвращает список водителей по идентификаторам
        /// </summary>
        /// <returns></returns>
        IList<Driver> GetDriversByGuids(Guid agentGuid, IList<Guid> guids);
    }
}