using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using TaxiManager.Core;
using TaxiManager.Data.Model;

namespace TaxiManager.Data.EntityFramework
{
    /// <summary>
    /// Класс описывающй репозиторий прав
    /// </summary>
    public class EntityFrameworkRightRepository : IRightRepository
    {
        private readonly ReaderWriterLockSlim _lock;

        private readonly Dictionary<Guid, Dictionary<EntityType, OperationType[]>> _cache;

        private readonly ILogger _logger;

        private readonly ApplicationContext _dataContext;

        private readonly IEntityRepository _entityTypeRepository;

        public EntityFrameworkRightRepository(ILogger logger, ApplicationContext dataContext, IEntityRepository entityTypeRepository)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");
            if (entityTypeRepository == null)
                throw new ArgumentNullException("entityTypeRepository");
            _lock = new ReaderWriterLockSlim();
            _cache = new Dictionary<Guid, Dictionary<EntityType, OperationType[]>>();
            _logger = logger;
            _dataContext = dataContext;
            _entityTypeRepository = entityTypeRepository;
        }

        /// <summary>
        /// Функция производит обновления прав
        /// </summary>
        /// <param name="owner">Агент назначающий права</param>
        /// <param name="agentGuid">агент которому обнавляют права</param>
        /// <param name="entityType">тип сущности на которую назначаются права</param>
        /// <param name="operations">Права</param>
        public void UpdateRights(Guid owner, Guid agentGuid, EntityType entityType, OperationType[] operations)
        {
            if (owner == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid owner {0}", owner));
            if (agentGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agentGuid {0}", agentGuid));
            if (operations == null)
                throw new InvalidDataException("Invalid operations is null");
            if (!_entityTypeRepository.Exist(owner,agentGuid, EntityType.Agent))
                throw new InvalidDataException(string.Format("Agent {0} not found", agentGuid));
            bool flag = false;
            var rights = GetRights(owner, EntityType.Agent);
            for (int i = 0; i < rights.Length; i++)
            {
                var right = rights[i];
                if (right == OperationType.Admin)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
                throw new InvalidDataException(string.Format("Agent {0} cannot access to add rules", owner));
            var utc = DateTime.UtcNow;
            var existEntity = (from right in _dataContext.Rights
                where agentGuid == right.AgentGuid && right.EntityType == entityType
                select right).FirstOrDefault();
            if (existEntity != null)
            {
                existEntity.OperationTypes = operations;
                existEntity.UpdateTime = utc;
            }
            else
            {
                _dataContext.Rights.Add(new Right
                {
                    AgentGuid = agentGuid,
                    EntityType = entityType,
                    CreateTime = utc,
                    UpdateTime = utc,
                    OperationTypes = operations
                });
            }
            _dataContext.SaveChanges();
            UpdateCache(agentGuid, entityType, operations);
            _logger.Info(string.Format("Add new right. agentGuid: {0}, entityType: {1}, operations:{2}", agentGuid, entityType,
                string.Join(", ", operations)));
        }

        /// <summary>
        /// Функция возвращает права для пользователя по заданной сущности
        /// </summary>
        /// <param name="agentGuid"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public OperationType[] GetRights(Guid agentGuid, EntityType entityType)
        {
            if (agentGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agentGuid {0}", agentGuid));
            //Поиск в кеше
            using (_lock.UseReadLock())
            {
                Dictionary<EntityType, OperationType[]> rightsByType;
                if (_cache.TryGetValue(agentGuid, out rightsByType))
                {
                    OperationType[] operationTypes;
                    if (rightsByType.TryGetValue(entityType, out operationTypes))
                    {
                        return operationTypes;
                    }
                }
            }
            var rights = (from right in _dataContext.Rights
                where right.AgentGuid == agentGuid && right.EntityType == entityType
                select right.OperationTypes).FirstOrDefault();
            if (rights == null)
                rights = new OperationType[0];
            UpdateCache(agentGuid, entityType, rights);
            return rights;
        }

        private void UpdateCache(Guid agentGuid, EntityType entityType, OperationType[] rights)
        {
            using (_lock.UseWriteLock())
            {
                
                Dictionary<EntityType, OperationType[]> operationByType;
                if (_cache.TryGetValue(agentGuid, out operationByType))
                {
                    if (operationByType.ContainsKey(entityType))
                    {
                        operationByType[entityType] = rights;
                    }
                    else
                    {
                        operationByType.Add(entityType, rights);
                    }
                }
                else
                {
                    operationByType = new Dictionary<EntityType, OperationType[]> {{entityType, rights}};
                    _cache.Add(agentGuid, operationByType);
                }
            }
        }
    }
}
