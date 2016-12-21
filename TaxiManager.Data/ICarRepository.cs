using System;
using System.Collections.Generic;

using TaxiManager.Data.Model;

namespace TaxiManager.Data
{
    /// <summary>
    /// Интерфейс описывает репозиторий доступа к сущности автомобиль
    /// </summary>
    public interface ICarRepository
    {
        /// <summary>
        /// Метод добавляет новый или обновляет существующий автомобиль
        /// </summary>
        Car AddOrUpdateCar(Guid agentGuid, Car car);
        
        /// <summary>
        /// Метод удаляет существующий автомобиль
        /// </summary>
        void DeleteCar(Guid agentGuid, Car car);

        /// <summary>
        /// Метод возвращает список автомобилей по идентификаторам
        /// </summary>
        /// <returns></returns>
        IList<Car> GetCarsByGuids(Guid agentGuid, IList<Guid> guids);
    }
}
