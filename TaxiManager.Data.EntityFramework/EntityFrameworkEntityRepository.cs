using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Threading;

using TaxiManager.Core;
using TaxiManager.Data.Model;

namespace TaxiManager.Data.EntityFramework
{
    /// <summary>
    /// Класс описывающий репозиторий доступа к модели связей сущностей в системе
    /// </summary>
    public class EntityFrameworkEntityRepository : IEntityRepository
    {
        private readonly ReaderWriterLockSlim _lock;

        private readonly Dictionary<Guid, Dictionary<EntityType, List<Guid>>> _cache;

        private readonly ILogger _logger;

        private readonly ApplicationContext _dataContext;

        public EntityFrameworkEntityRepository(ILogger logger, ApplicationContext dataContext)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");
            _lock = new ReaderWriterLockSlim();
            _cache = new Dictionary<Guid, Dictionary<EntityType, List<Guid>>>();
            _logger = logger;
            _dataContext = dataContext;
        }

        /// <summary>
        /// Функция возвращает список идентификаторов сущностей по заданному идентификатору агента и типу сущности
        /// </summary>
        /// <param name="agentGuid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IList<Guid> GetEntitys(Guid agentGuid, EntityType type)
        {
            List<Guid> guids = new List<Guid>();
            //Поиск в кеше
            using (_lock.UseReadLock())
            {
                Dictionary<EntityType, List<Guid>> agentGuids;
                if (_cache.TryGetValue(agentGuid, out agentGuids))
                {
                    //Получаем всех подчиненных агентов
                    List<Guid> agents;
                    if (agentGuids.TryGetValue(EntityType.Agent, out agents))
                    {
                        for (int i = 0; i < agents.Count; i++)
                        {
                            var agent = agents[i];
                            Dictionary<EntityType, List<Guid>> entityChildAgents;
                            if (_cache.TryGetValue(agent, out entityChildAgents))
                            {
                                //Получаем все что доступно вниз по уровню иерархии агентов
                                List<Guid> childAgentGuids;
                                if (entityChildAgents.TryGetValue(type, out childAgentGuids))
                                {
                                    guids.AddRange(childAgentGuids);
                                }
                            }
                        }
                    }
                    //Заполняем непосредственно своими связями
                    List<Guid> currentAgentGuids;
                    if (agentGuids.TryGetValue(type, out currentAgentGuids))
                    {
                        guids.AddRange(currentAgentGuids);
                    }
                }
            }
            if (guids.Count != 0)
                return guids;

            //получаем идентификаторы агентов
            var childAgentsGuids = from eg in _dataContext.EntityGuids
                                   where eg.AgentGuid == agentGuid && eg.EntityType == EntityType.Agent && !eg.IsDelete
                                   select eg.AgentGuid;
            //Получаем все что доступно вниз по уровню иерархии агентов
            guids = (from eg in _dataContext.EntityGuids
                     where (childAgentsGuids.Contains(eg.AgentGuid) || eg.AgentGuid == agentGuid) && eg.EntityType == type && !eg.IsDelete
                     select eg.EntityGuid).ToList();

            UpdateCache(agentGuid, type, guids, new List<Guid>());
            return guids;
        }

        /// <summary>
        /// Свойство добавляет новую связь с сущность в систему
        /// </summary>
        /// <param name="agentGuid"></param>
        /// <param name="newGuid"></param>
        /// <param name="type"></param>
        public void AddEntity(Guid agentGuid, Guid newGuid, EntityType type)
        {
            if (agentGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agentGuid {0}", agentGuid));
            if (newGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid newGuid {0}", newGuid));
            var existEntity = (from eg in _dataContext.EntityGuids
                               where newGuid == eg.EntityGuid
                               select eg).FirstOrDefault();
            if (existEntity != null)
                throw new InvalidDataException(string.Format("Guid {0} exist", newGuid));
            var utc = DateTime.UtcNow;
            _dataContext.EntityGuids.Add(new EntityGuids
            {
                AgentGuid = agentGuid,
                EntityGuid = newGuid,
                EntityType = type,
                CreateTime = utc,
                UpdateTime = utc,
                IsDelete = false
            });

            _dataContext.SaveChanges();
            UpdateCache(agentGuid, type, new List<Guid> { newGuid }, new List<Guid>());
            _logger.Info(string.Format("Add new EntityGuids. agentGuid: {0}, newGuid: {1}, type:{2}", agentGuid, newGuid,
                type));
        }

        /// <summary>
        /// Функция удаляет связь агента с заданной сущностью
        /// </summary>
        /// <param name="agentGuid"></param>
        /// <param name="delGuid"></param>
        /// <param name="type"></param>
        public void DeleteEntity(Guid agentGuid, Guid delGuid, EntityType type)
        {
            if (agentGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agentGuid {0}", agentGuid));
            if (delGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid delGuid {0}", delGuid));
            var delEntityGuid = _dataContext.EntityGuids.FirstOrDefault(_ => _.EntityGuid == delGuid);
            if (delEntityGuid == null)
                throw new InvalidDataException(string.Format("EntityGuid with Guid {0} not found", delGuid));
           var agents= GetEntitys(agentGuid, EntityType.Agent);
           if (agentGuid != delEntityGuid.AgentGuid && !agents.Contains(delEntityGuid.AgentGuid))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to object {1} {2}", agentGuid, type, delGuid));
            if (type != delEntityGuid.EntityType)
                throw new InvalidDataException(string.Format("EntityGuid with Guid {0} not valid", agentGuid));

            delEntityGuid.IsDelete = true;
            _dataContext.SaveChanges();
            UpdateCache(delEntityGuid.AgentGuid, type, new List<Guid>(), new List<Guid> { delGuid });
            _logger.Info(string.Format("Delete item EntityGuids. agentGuid: {0}, delGuid: {1}, type:{2}", agentGuid, delGuid, type));
        }

        private void UpdateCache(Guid agentGuid, EntityType type,
            IList<Guid> addGuids, IList<Guid> deleteGuids)
        {
            using (_lock.UseWriteLock())
            {
                List<Guid> guids;
                Dictionary<EntityType, List<Guid>> agentGuids;
                if (_cache.TryGetValue(agentGuid, out agentGuids))
                {
                    if (agentGuids.TryGetValue(type, out guids))
                    {
                        guids.AddRange(addGuids);
                    }
                    else
                    {
                        guids = new List<Guid>(addGuids);
                        agentGuids.Add(type, guids);
                    }
                }
                else
                {
                    guids = new List<Guid>(addGuids);
                    agentGuids = new Dictionary<EntityType, List<Guid>> { { type, guids } };
                    _cache.Add(agentGuid, agentGuids);
                }

                for (int i = 0; i < deleteGuids.Count; i++)
                {
                    var deleteGuid = deleteGuids[i];
                    guids.Remove(deleteGuid);
                }
            }
        }
    }
}
