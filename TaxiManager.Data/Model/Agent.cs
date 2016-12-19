namespace TaxiManager.Data.Model
{
    /// <summary>
    /// Класс описывающий агентов
    /// </summary>
    [EntityInfo(EntityType.Agent)]
    public sealed class Agent : Entity
    {
        /// <summary>
        /// Свойство задающее название агента
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Свойство задающее описание
        /// </summary>
        public string Description { get; set; }
    }
}
