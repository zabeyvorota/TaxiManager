using System;

namespace TaxiManager.Data.Model
{
    /// <summary>
    /// Класс описывающий водителя
    /// </summary>
    [EntityInfo(EntityType.Driver)]
    public sealed class Driver : Entity
    {
        /// <summary>
        /// Свойство задает и возвращает имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Свойство задает и возвращает фамилию
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Свойство задает и возвращает отчество
        /// </summary>
        public string Patronymic { get; set; }

        /// <summary>
        /// Свойство задает и возвращает день рождения
        /// </summary>
        public DateTime Birthday { get; set; }

        /// <summary>
        /// Свойство задает и возвращает серию прав
        /// </summary>
        public string CarLicenseSerialCode { get; set; }

        /// <summary>
        /// Свойство задает и возвращает номер прав
        /// </summary>
        public string CarLicenseNumberCode { get; set; }
    }
}
