using System;

namespace TaxiManager.Data.Model
{
    /// <summary>
    /// Класс отвечающий за описание автомобиля
    /// </summary>
    [EntityInfo(EntityType.Car)]
    public sealed class Car : Entity
    {
        /// <summary>
        /// Свойство задает и возвращает  номер автомобиля
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Свойство задает и возвращает vin код
        /// </summary>
        public string VinCode { get; set; }

        /// <summary>
        /// Свойство задает и возвращает дату выпуска
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Свойство задает и возвращает серию свидетельства о регистрации
        /// </summary>
        public string SerialCode { get; set; }

        /// <summary>
        /// Свойство задает и возвращает номер свидетельства о регистрации
        /// </summary>
        public string NumberCode { get; set; }

        /// <summary>
        /// Свойство задает и возвращает модель
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Свойство задает и возвращает марку
        /// </summary>
        public string Make { get; set; }

        /// <summary>
        /// Свойство задает и возвращает пробег
        /// </summary>
        public double Distance { get; set; }
    }
}
