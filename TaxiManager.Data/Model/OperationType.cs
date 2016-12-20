namespace TaxiManager.Data.Model
{
    /// <summary>
    /// Перечесления операций в системе
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// Получение данных
        /// </summary>
        Select,

        /// <summary>
        /// Добавление и обновление
        /// </summary>
        AddOrUpdate,

        /// <summary>
        /// Удаление
        /// </summary>
        Delete,
        /// <summary>
        /// Возможнасть раздавать права
        /// </summary>
        Admin
    }
}