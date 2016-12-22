using System;
using System.Collections.Generic;

using TaxiManager.Data.Model;

namespace TaxiManager.Data
{
    /// <summary>
    /// Интерфейс описывающий доступ к репозиторию аренд автомобилей водителями
    /// </summary>
    public interface ICarRentalRepository
    {
        /// <summary>
        /// Функция добавляет новую арену и возвращает ее
        /// </summary>
        /// <param name="owner">пользователь добавляющий аренду</param>
        /// <param name="carGuid">идентификтор автомобиля</param>
        /// <param name="driverGuid">идентификатор машины</param>
        /// <returns></returns>
        CarRental AddRental(Guid owner, Guid carGuid, Guid driverGuid);

        /// <summary>
        /// Функция закрывает аренду
        /// </summary>
        /// <param name="owner">пользователь закрывающий аренду</param>
        /// <param name="carGuid">идентификтор автомобиля</param>
        /// <param name="driverGuid">идентификатор машины</param>
        /// <returns></returns>
        CarRental CloseRental(Guid owner, Guid carGuid, Guid driverGuid);

        /// <summary>
        /// Функция удаляет аренду
        /// </summary>
        /// <param name="owner">пользователь удаляющий аренду</param>
        /// <param name="carGuid">идентификтор автомобиля</param>
        /// <param name="driverGuid">идентификатор машины</param>
        /// <returns></returns>
        void DeleteRental(Guid owner, Guid carGuid, Guid driverGuid);

        /// <summary>
        /// Функция возвращает последнюю аренду автомобиля
        /// </summary>
        /// <param name="owner">пользователь запрашивающий аренду</param>
        /// <param name="carGuid">идентификтор автомобиля</param>
        /// <returns></returns>
        CarRental GetLastRentalByCar(Guid owner, Guid carGuid);

        /// <summary>
        /// Функция возвращает последнюю аренду водителя
        /// </summary>
        /// <param name="owner">пользователь запрашивающий аренду</param>
        /// <param name="driverGuid">идентификтор водителя</param>
        /// <returns></returns>
        CarRental GetLastRentalByDriver(Guid owner, Guid driverGuid);

        /// <summary>
        /// Функция возвращает аренды автомобиля
        /// </summary>
        /// <param name="owner">пользователь запрашивающий аренду</param>
        /// <param name="carGuid">идентификтор автомобиля</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">конец периода</param>
        /// <returns></returns>
        IList<CarRental> GetRentalByCar(Guid owner, Guid carGuid, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Функция возвращает аренды водителя
        /// </summary>
        /// <param name="owner">пользователь запрашивающий аренду</param>
        /// <param name="driverGuid">идентификтор водителя</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">конец периода</param>
        /// <returns></returns>
        IList<CarRental> GetRentalByDriver(Guid owner, Guid driverGuid, DateTime startDate, DateTime endDate);
    }
}
