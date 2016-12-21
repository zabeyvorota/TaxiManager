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
    /// Класс описывающий реализацию репозитория водителей 
    /// </summary>
    public sealed class EntityFrameworkDrivarRepository : IDriverRepository
    {
        private readonly ReaderWriterLockSlim _lock;

        private readonly Dictionary<Guid, Driver> _cache;

        private readonly ILogger _logger;

        private readonly IEntityRepository _entityRepository;

        private readonly IRightRepository _rightRepository;

        private readonly ApplicationContext _dataContext;

        public EntityFrameworkDrivarRepository(ILogger logger,
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
            _cache = new Dictionary<Guid, Driver>();
            _logger = logger;
            _entityRepository = entityRepository;
            _rightRepository = rightRepository;
            _dataContext = dataContext;
        }

        /// <summary>
        /// Метод возвращает список водителей по идентификаторам
        /// </summary>
        /// <returns></returns>
        public Driver AddOrUpdateDriver(Guid agentGuid, Driver driver)
        {
            if (agentGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agentGuid {0}", agentGuid));
            if (driver == null)
                throw new InvalidDataException("driver is null");
            if (string.IsNullOrEmpty(driver.Surname))
                throw new InvalidDataException(string.Format("Invalid driver.Surname   {0}", driver.Surname));
            if (string.IsNullOrEmpty(driver.Name))
                throw new InvalidDataException(string.Format("Invalid driver.Name   {0}", driver.Surname));
            if (driver.Birthday==DateTime.MinValue)
                throw new InvalidDataException(string.Format("Invalid driver.Birthday   {0}", driver.Birthday));
            var operations = _rightRepository.GetRights(agentGuid, EntityType.Driver);
            if (!operations.Contains(OperationType.AddOrUpdate) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to add or update {1} {2}", agentGuid, EntityType.Driver, driver.Guid));
            Driver result;
            var dt = DateTime.UtcNow;
            var updateList = new List<Driver>();
            var addList = new List<Driver>();
            var existDriver = _dataContext.Drivers.Find(driver.Id);
            if (existDriver == null)
            {
                driver.Guid = Guid.NewGuid();
                driver.UpdateTime = dt;
                driver.CreateTime = dt;
                _dataContext.Drivers.Add(driver);
                _entityRepository.AddEntity(agentGuid, driver.Guid, EntityType.Driver);
                addList.Add(driver);
                _logger.Info(string.Format("Add new driver {0}. Owner {1}", driver.Guid, agentGuid));
                result = driver;
            }
            else
            {
                if (driver.Guid == Guid.Empty)
                    throw new InvalidDataException(string.Format("Invalid driver.Guid   {0}", driver.Guid));
                if (!_entityRepository.Exist(agentGuid, driver.Guid, EntityType.Driver))
                    throw new InvalidDataException(string.Format("Agent {0} cannot access to object {1} {2}", agentGuid, EntityType.Driver, driver.Guid));

                existDriver.Birthday = driver.Birthday;
                existDriver.CarLicenseNumberCode = driver.CarLicenseNumberCode;
                existDriver.CarLicenseSerialCode = driver.CarLicenseSerialCode;
                existDriver.Name = driver.Name;
                existDriver.Patronymic = driver.Patronymic;
                existDriver.Surname = driver.Surname;
                existDriver.UpdateTime = dt;
                updateList.Add(existDriver);
                _logger.Info(string.Format("Update driver {0}. Owner {1}", driver.Guid, agentGuid));
                result = driver;
            }
            _dataContext.SaveChanges();
            UpdateCache(addList,updateList, new List<Driver>());
            return result;
        }

        /// <summary>
        /// Метод удаляет существующего водителя
        /// </summary>
        public void DeleteDriver(Guid agentGuid, Driver driver)
        {
            if (agentGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agentGuid {0}", agentGuid));
            if (driver == null)
                throw new InvalidDataException("driver is null");
            if (driver.Id <= 0)
                throw new InvalidDataException(string.Format("Invalid driver.Id  {0}", driver.Id));
            if (driver.Guid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid driver.Guid   {0}", driver.Guid));
            if (!_entityRepository.Exist(agentGuid, driver.Guid, EntityType.Driver))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to object {1} {2}", agentGuid, EntityType.Driver, driver.Guid));
            var operations = _rightRepository.GetRights(agentGuid, EntityType.Agent);
            if (!operations.Contains(OperationType.Delete) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to delete {1} {2}", agentGuid, EntityType.Driver, driver.Guid));
            var existDriver = _dataContext.Drivers.Find(driver.Id);
            existDriver.IsDelete = true;
            _entityRepository.DeleteEntity(agentGuid, driver.Guid, EntityType.Driver);
            _dataContext.SaveChanges();
            UpdateCache(new List<Driver>(), new List<Driver>(), new List<Driver> { existDriver });
        }

        /// <summary>
        /// Метод возвращает список водителей по идентификаторам
        /// </summary>
        /// <returns></returns>
        public IList<Driver> GetDriversByGuids(Guid agentGuid, IList<Guid> guids)
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
                if (!_entityRepository.Exist(agentGuid, guid, EntityType.Driver))
                {
                    throw new InvalidDataException(string.Format("Agent {0} cannot access to object {1} {2}", agentGuid, EntityType.Driver, guid));
                }
            }
            var operations = _rightRepository.GetRights(agentGuid, EntityType.Driver);
            if (!operations.Contains(OperationType.Select) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to select {1}", agentGuid, EntityType.Driver));
            var drivers = new List<Driver>(guids.Count);
            var notFoundInCache = new List<Guid>();
            using (_lock.UseReadLock())
            {
                foreach (var guid in guids)
                {
                    Driver driver;
                    if (_cache.TryGetValue(guid, out driver))
                    {
                        drivers.Add(driver);
                    }
                    else
                    {
                        notFoundInCache.Add(guid);
                    }
                }
            }
            if (notFoundInCache.Count == 0)
            {
                return drivers;
            }
            var driversOnDb =
                (from car in _dataContext.Drivers
                    where notFoundInCache.Contains(car.Guid) && !car.IsDelete
                    select car).ToList();
            drivers.AddRange(driversOnDb);
            UpdateCache(driversOnDb, new List<Driver>(), new List<Driver>());
            return drivers;
        }

        private void UpdateCache(List<Driver> adding, List<Driver> updating, List<Driver> deleting)
        {
            using (_lock.UseWriteLock())
            {
                for (int i = 0; i < deleting.Count; i++)
                {
                    var driver = deleting[i];
                    _cache.Remove(driver.Guid);
                }
                for (int i = 0; i < updating.Count; i++)
                {
                    var driver = updating[i];
                    _cache.Remove(driver.Guid);
                    _cache.Add(driver.Guid, driver);
                }
                for (int i = 0; i < adding.Count; i++)
                {
                    var driver = adding[i];
                    _cache.Add(driver.Guid, driver);
                }
            }
        }
    }
}
