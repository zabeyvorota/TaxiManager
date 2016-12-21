using System;

namespace TaxiManager.Data.Model
{
    /// <summary>
    /// Класс описывающий модель аренды автомобиля
    /// </summary>
    public sealed class CarRental
    {
        /// <summary>
        /// Свойство задающее и возвращающее идентификатор автомобиля
        /// </summary>
        public Guid CarGuid { get; set; }

        /// <summary>
        /// Свойство задающее и возвращающее идентификатор водителя
        /// </summary>
        public Guid DriverGuid { get; set; }

        /// <summary>
        /// Свойство задающее и возвращающее флаг окончания аренды
        /// </summary>
        public bool IsClose { get; set; }

        /// <summary>
        /// Свойство задающее и возвращающее время начало аренды
        /// </summary>
        public DateTime StartRentalDate { get; set; }

        /// <summary>
        /// Свойство задающее и возвращающее время окончания аренды
        /// </summary>
        public DateTime EndRentalDate { get; set; }
    }
}
