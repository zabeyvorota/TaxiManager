using System;

using TaxiManager.Data.Model;

namespace TaxiManager.Data
{
    /// <summary>
    /// Интерфейс описывающий репозиторий прав в системе
    /// </summary>
    public interface IRightRepository
    {
        /// <summary>
        /// Функция производит обновления прав
        /// </summary>
        /// <param name="owner">Агент назначающий права</param>
        /// <param name="agentGuid">агент которому обнавляют права</param>
        /// <param name="entityType">тип сущности на которую назначаются права</param>
        /// <param name="operations">Права</param>
        void UpdateRights(Guid owner, Guid agentGuid, EntityType entityType, OperationType[] operations);

        /// <summary>
        /// Функция возвращает права для пользователя по заданной сущности
        /// </summary>
        /// <param name="agentGuid"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        OperationType[] GetRights(Guid agentGuid, EntityType entityType);
    }
}
