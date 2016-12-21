using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using TaxiManager.Core;
using TaxiManager.Data.Model;

namespace TaxiManager.Data.EntityFramework
{
    /// <summary>
    /// Класс описывающий реализацию репозитория агентов 
    /// </summary>
    public sealed class EntityFrameworkAgentRepository : IAgentRepository
    {
        private readonly ReaderWriterLockSlim _lock;

        private readonly Dictionary<Guid, Agent> _cache;

        private readonly ILogger _logger;

        private readonly IEntityRepository _entityRepository;

        private readonly IRightRepository _rightRepository;

        private readonly ApplicationContext _dataContext;

        public EntityFrameworkAgentRepository(ILogger logger,
            IEntityRepository entityRepository,
            IRightRepository rightRepository,
            ApplicationContext dataContext)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (entityRepository == null)
                throw new ArgumentNullException("entityRepository");
            if (rightRepository == null)
                throw new ArgumentNullException("rightRepository");
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");
            _lock = new ReaderWriterLockSlim();
            _cache = new Dictionary<Guid, Agent>();
            _logger = logger;
            _entityRepository = entityRepository;
            _rightRepository = rightRepository;
            _dataContext = dataContext;
        }

        /// <summary>
        /// Метод добавляет нового или обновляет существующего агента
        /// </summary>
        public Agent AddOrUpdateAgent(Guid agentGuid, Agent agent)
        {
            if (agentGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agentGuid {0}", agentGuid));
            if (agent == null)
                throw new InvalidDataException("Agent is null");
            if (string.IsNullOrEmpty(agent.Name))
                throw new InvalidDataException(string.Format("Invalid agent.Name   {0}", agent.Name));
            var operations = _rightRepository.GetRights(agentGuid, EntityType.Agent);
            if (!operations.Contains(OperationType.AddOrUpdate) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to add or update {1} {2}", agentGuid, EntityType.Agent, agent.Guid));
            Agent result;
            var dt = DateTime.UtcNow;
            var updateList = new List<Agent>();
            var addList = new List<Agent>();
            var existAgent = _dataContext.Agents.Find(agent.Id);
            if (existAgent == null)
            {
                agent.Guid = Guid.NewGuid();
                agent.UpdateTime = dt;
                agent.CreateTime = dt;
                _dataContext.Agents.Add(agent);
                _entityRepository.AddEntity(agentGuid, agent.Guid, EntityType.Agent);
                addList.Add(agent);
                _logger.Info(string.Format("Add new agent {0}. Owner {1}", agent.Guid, agentGuid));
                result = agent;
            }
            else
            {
                if (agent.Guid == Guid.Empty)
                    throw new InvalidDataException(string.Format("Invalid agent.Guid   {0}", agent.Guid));
                if (!_entityRepository.Exist(agentGuid, agent.Guid, EntityType.Agent))
                    throw new InvalidDataException(string.Format("Agent {0} cannot access to object {1} {2}", agentGuid, EntityType.Agent, agent.Guid));
                existAgent.Name = agent.Name;
                existAgent.Description = agent.Description;
                existAgent.UpdateTime = dt;
                updateList.Add(existAgent);
                _logger.Info(string.Format("Update agent {0}. Owner {1}", agent.Guid, agentGuid));
                result = agent;
            }
            _dataContext.SaveChanges();
            UpdateCache(addList,updateList, new List<Agent>());
            return result;
        }

        /// <summary>
        /// Метод удаляет существующего агента
        /// </summary>
        public void DeleteAgent( Guid agentGuid, Agent agent)
        {
            if (agentGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agentGuid {0}", agentGuid));
            if (agent == null)
                throw new InvalidDataException("Agent is null");
            if (agent.Id <= 0)
                throw new InvalidDataException(string.Format("Invalid agent.Id  {0}", agent.Id));
            if (agent.Guid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agent.Guid   {0}", agent.Guid));
            if (!_entityRepository.Exist(agentGuid, agent.Guid, EntityType.Agent))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to object {1} {2}", agentGuid, EntityType.Agent, agent.Guid));
            var operations = _rightRepository.GetRights(agentGuid, EntityType.Agent);
            if (!operations.Contains(OperationType.Delete) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to delete {1} {2}", agentGuid, EntityType.Agent, agent.Guid));
            var deletedAgent = _dataContext.Agents.Find(agent.Id);
            deletedAgent.IsDelete = true;
            _entityRepository.DeleteEntity(agentGuid, agent.Guid, EntityType.Agent);
            _dataContext.SaveChanges();
            UpdateCache(new List<Agent>(), new List<Agent>(), new List<Agent> { deletedAgent });
        }

        /// <summary>
        /// Метод возвращает список агентов по идентификаторам
        /// </summary>
        /// <returns></returns>
        public IList<Agent> GetAgentsByGuids(Guid agentGuid, IList<Guid> guids)
        {
            if (agentGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agentGuid {0}", agentGuid));
            if (guids == null)
                throw new InvalidDataException("Guids is null");
            if (guids.Count == 0)
                throw new InvalidDataException("Guids is empty");
            for (int i = 0; i < guids.Count; i++)
            {
                var guid = guids[i];
                if (!_entityRepository.Exist(agentGuid, guid, EntityType.Agent))
                {
                    throw new InvalidDataException(string.Format("Agent {0} cannot access to object {1} {2}", agentGuid, EntityType.Agent, guid));
                }
            }
            var operations = _rightRepository.GetRights(agentGuid, EntityType.Agent);
            if (!operations.Contains(OperationType.Select) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to select {1}", agentGuid, EntityType.Agent));
            var agents = new List<Agent>(guids.Count);
            var notFoundInCache = new List<Guid>();
            using (_lock.UseReadLock())
            {
                foreach (var guid in guids)
                {
                    Agent agent;
                    if (_cache.TryGetValue(guid, out agent))
                    {
                        agents.Add(agent);
                    }
                    else
                    {
                        notFoundInCache.Add(guid);
                    }
                }
            }
            if (notFoundInCache.Count == 0)
            {
                return agents;
            }
            var agentsOnDb =
                (from agent in _dataContext.Agents
                    where notFoundInCache.Contains(agent.Guid) && !agent.IsDelete
                    select agent).ToList();
            agents.AddRange(agentsOnDb);
            UpdateCache(agentsOnDb, new List<Agent>(), new List<Agent>());
            return agents;
        }

        private void UpdateCache(List<Agent> adding, List<Agent> updating, List<Agent> deleting)
        {
            using (_lock.UseWriteLock())
            {
                for (int i = 0; i < deleting.Count; i++)
                {
                    var agent = deleting[i];
                    _cache.Remove(agent.Guid);
                }
                for (int i = 0; i < updating.Count; i++)
                {
                    var agent = updating[i];
                    _cache.Remove(agent.Guid);
                    _cache.Add(agent.Guid, agent);
                }
                for (int i = 0; i < adding.Count; i++)
                {
                    var agent = adding[i];
                    _cache.Add(agent.Guid, agent);
                }
            }
        }
    }
}
