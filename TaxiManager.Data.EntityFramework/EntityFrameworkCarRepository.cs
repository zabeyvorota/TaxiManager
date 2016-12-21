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
    /// Класс описывающий реализацию репозитория машин 
    /// </summary>
    public sealed class EntityFrameworkCarRepository : ICarRepository
    {
        private readonly ReaderWriterLockSlim _lock;

        private readonly Dictionary<Guid, Car> _cache;

        private readonly ILogger _logger;

        private readonly IEntityRepository _entityRepository;

        private readonly IRightRepository _rightRepository;

        private readonly ApplicationContext _dataContext;

        public EntityFrameworkCarRepository(ILogger logger,
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
            _cache = new Dictionary<Guid, Car>();
            _logger = logger;
            _entityRepository = entityRepository;
            _rightRepository = rightRepository;
            _dataContext = dataContext;
        }

        /// <summary>
        /// Метод добавляет новый или обновляет существующий автомобиль
        /// </summary>
        public Car AddOrUpdateCar(Guid agentGuid, Car car)
        {
            if (agentGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agentGuid {0}", agentGuid));
            if (car == null)
                throw new InvalidDataException("Car is null");
            if (string.IsNullOrEmpty(car.Number))
                throw new InvalidDataException(string.Format("Invalid car.Name   {0}", car.Number));
            var operations = _rightRepository.GetRights(agentGuid, EntityType.Driver);
            if (!operations.Contains(OperationType.AddOrUpdate) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to add or update {1} {2}", agentGuid, EntityType.Car, car.Guid));
            Car result;
            var dt = DateTime.UtcNow;
            var updateList = new List<Car>();
            var addList = new List<Car>();
            var existCar = _dataContext.Cars.Find(car.Id);
            if (existCar == null)
            {
                car.Guid = Guid.NewGuid();
                car.UpdateTime = dt;
                car.CreateTime = dt;
                _dataContext.Cars.Add(car);
                _entityRepository.AddEntity(agentGuid, car.Guid, EntityType.Car);
                addList.Add(car);
                _logger.Info(string.Format("Add new car {0}. Owner {1}", car.Guid, agentGuid));
                result = car;
            }
            else
            {
                if (car.Guid == Guid.Empty)
                    throw new InvalidDataException(string.Format("Invalid car.Guid   {0}", car.Guid));
                if (!_entityRepository.Exist(agentGuid, car.Guid, EntityType.Car))
                    throw new InvalidDataException(string.Format("Agent {0} cannot access to object {1} {2}", agentGuid, EntityType.Car, car.Guid));
                existCar.Number = car.Number;
                existCar.Distance = car.Distance;
                existCar.Make = car.Make;
                existCar.Model = car.Model;
                existCar.NumberCode = car.NumberCode;
                existCar.SerialCode = car.SerialCode;
                existCar.VinCode = car.VinCode;
                existCar.UpdateTime = dt;
                updateList.Add(existCar);
                _logger.Info(string.Format("Update car {0}. Owner {1}", car.Guid, agentGuid));
                result = car;
            }
            _dataContext.SaveChanges();
            UpdateCache(addList,updateList, new List<Car>());
            return result;
        }

        /// <summary>
        /// Метод удаляет существующий автомобиль
        /// </summary>
        public void DeleteCar(Guid agentGuid, Car car)
        {
            if (agentGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid agentGuid {0}", agentGuid));
            if (car == null)
                throw new InvalidDataException("Car is null");
            if (car.Id <= 0)
                throw new InvalidDataException(string.Format("Invalid car.Id  {0}", car.Id));
            if (car.Guid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid car.Guid   {0}", car.Guid));
            if (!_entityRepository.Exist(agentGuid, car.Guid, EntityType.Car))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to object {1} {2}", agentGuid, EntityType.Car, car.Guid));
            var operations = _rightRepository.GetRights(agentGuid, EntityType.Car);
            if (!operations.Contains(OperationType.Delete) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to delete {1} {2}", agentGuid, EntityType.Car, car.Guid));
            var existCar = _dataContext.Cars.Find(car.Id);
            existCar.IsDelete = true;
            _entityRepository.DeleteEntity(agentGuid, car.Guid, EntityType.Car);
            _dataContext.SaveChanges();
            UpdateCache(new List<Car>(), new List<Car>(), new List<Car> { existCar });
        }

        /// <summary>
        /// Метод возвращает список автомобилей по идентификаторам
        /// </summary>
        /// <returns></returns>
        public IList<Car> GetCarsByGuids(Guid agentGuid, IList<Guid> guids)
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
                if (!_entityRepository.Exist(agentGuid, guid, EntityType.Car))
                {
                    throw new InvalidDataException(string.Format("Agent {0} cannot access to object {1} {2}", agentGuid, EntityType.Car, guid));
                }
            }
            var operations = _rightRepository.GetRights(agentGuid, EntityType.Car);
            if (!operations.Contains(OperationType.Select) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to select {1}", agentGuid, EntityType.Car));
            var cars = new List<Car>(guids.Count);
            var notFoundInCache = new List<Guid>();
            using (_lock.UseReadLock())
            {
                foreach (var guid in guids)
                {
                    Car car;
                    if (_cache.TryGetValue(guid, out car))
                    {
                        cars.Add(car);
                    }
                    else
                    {
                        notFoundInCache.Add(guid);
                    }
                }
            }
            if (notFoundInCache.Count == 0)
            {
                return cars;
            }
            var carsOnDb =
                (from car in _dataContext.Cars
                    where notFoundInCache.Contains(car.Guid) && !car.IsDelete
                    select car).ToList();
            cars.AddRange(carsOnDb);
            UpdateCache(carsOnDb, new List<Car>(), new List<Car>());
            return cars;
        }

        private void UpdateCache(List<Car> adding, List<Car> updating, List<Car> deleting)
        {
            using (_lock.UseWriteLock())
            {
                for (int i = 0; i < deleting.Count; i++)
                {
                    var car = deleting[i];
                    _cache.Remove(car.Guid);
                }
                for (int i = 0; i < updating.Count; i++)
                {
                    var car = updating[i];
                    _cache.Remove(car.Guid);
                    _cache.Add(car.Guid, car);
                }
                for (int i = 0; i < adding.Count; i++)
                {
                    var car = adding[i];
                    _cache.Add(car.Guid, car);
                }
            }
        }
    }
}
