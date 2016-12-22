using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.IO;
using System.Threading;

using TaxiManager.Core;
using TaxiManager.Data.Model;

namespace TaxiManager.Data.EntityFramework
{
    /// <summary>
    ///  Репозиторий аренд автомобилей водителями
    /// </summary>
    public class EntityFrameworkCarRentalRepository : ICarRentalRepository
    {
        private readonly ReaderWriterLockSlim _lock;

        private readonly Dictionary<Guid, List<CarRental>> _rentalByCar;

        private readonly Dictionary<Guid, List<CarRental>> _rentalByDriver;

        private readonly ILogger _logger;

        private readonly IEntityRepository _entityRepository;

        private readonly IRightRepository _rightRepository;

        private readonly ApplicationContext _dataContext;

        public EntityFrameworkCarRentalRepository(ILogger logger,
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
            _logger = logger;
            _entityRepository = entityRepository;
            _rightRepository = rightRepository;
            _dataContext = dataContext;
            _lock = new ReaderWriterLockSlim();
            _rentalByCar = new Dictionary<Guid, List<CarRental>>();
            _rentalByDriver = new Dictionary<Guid, List<CarRental>>();

        }

        /// <summary>
        /// Функция добавляет новую арену и возвращает ее
        /// </summary>
        /// <param name="owner">пользователь добавляющий аренду</param>
        /// <param name="carGuid">идентификтор автомобиля</param>
        /// <param name="driverGuid">идентификатор машины</param>
        /// <returns></returns>
        public CarRental AddRental(Guid owner, Guid carGuid, Guid driverGuid)
        {
            if (owner == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid owner {0}", owner));
            if (carGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid carGuid {0}", carGuid));
            if (driverGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid driverGuid {0}", driverGuid));
            var operations = _rightRepository.GetRights(owner, EntityType.CarRental);
            if (!operations.Contains(OperationType.AddOrUpdate) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to add or update rental from car {1} by driver {2}", owner, carGuid, driverGuid));
            var carsGuid = _entityRepository.GetEntitys(owner, EntityType.Car);
            if (!carsGuid.Contains(carGuid))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to car {1} ", owner, carGuid));
            var driversGuid = _entityRepository.GetEntitys(owner, EntityType.Driver);
            if (!driversGuid.Contains(driverGuid))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to driver {1} ", owner, driverGuid));
            var rental = new CarRental
            {
                CarGuid = carGuid,
                DriverGuid = driverGuid,
                StartRentalDate = DateTime.UtcNow
            };
            _dataContext.CarRentals.Add(rental);
            _dataContext.SaveChanges();
            UpdateCache(new List<CarRental> {rental}, new List<CarRental>(), new List<CarRental>());
            _logger.Info(string.Format("Agent {0} add rental to car {1} and driver {2}", owner, carGuid, driverGuid));
            return rental;
        }

        /// <summary>
        /// Функция закрывает аренду
        /// </summary>
        /// <param name="owner">пользователь закрывающий аренду</param>
        /// <param name="carGuid">идентификтор автомобиля</param>
        /// <param name="driverGuid">идентификатор машины</param>
        /// <returns></returns>
        public CarRental CloseRental(Guid owner, Guid carGuid, Guid driverGuid)
        {
            if (owner == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid owner {0}", owner));
            if (carGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid carGuid {0}", carGuid));
            if (driverGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid driverGuid {0}", driverGuid));
            var operations = _rightRepository.GetRights(owner, EntityType.CarRental);
            if (!operations.Contains(OperationType.AddOrUpdate) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to add or update rental from car {1} by driver {2}", owner, carGuid, driverGuid));
            var carsGuid = _entityRepository.GetEntitys(owner, EntityType.Car);
            if (!carsGuid.Contains(carGuid))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to car {1} ", owner, carGuid));
            var driversGuid = _entityRepository.GetEntitys(owner, EntityType.Driver);
            if (!driversGuid.Contains(driverGuid))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to driver {1} ", owner, driverGuid));
            var rental =
                (from carRental in _dataContext.CarRentals
                    where carRental.CarGuid == carGuid && carRental.DriverGuid == driverGuid && !carRental.IsClose
                    select carRental).FirstOrDefault();
            if (rental == null)
                throw new InvalidDataException(string.Format("Rental from car {0} by driver {1} not found", carGuid, driverGuid));
            rental.EndRentalDate = DateTime.UtcNow;
            rental.IsClose = true;
            _dataContext.SaveChanges();
            UpdateCache(new List<CarRental>(), new List<CarRental> {rental}, new List<CarRental>());
            _logger.Info(string.Format("Agent {0} close rental to car {1} and driver {2}", owner, carGuid, driverGuid));
            return rental;
        }

        /// <summary>
        /// Функция удаляет аренду
        /// </summary>
        /// <param name="owner">пользователь удаляющий аренду</param>
        /// <param name="carGuid">идентификтор автомобиля</param>
        /// <param name="driverGuid">идентификатор машины</param>
        /// <returns></returns>
        public void DeleteRental(Guid owner, Guid carGuid, Guid driverGuid)
        {
            if (owner == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid owner {0}", owner));
            if (carGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid carGuid {0}", carGuid));
            if (driverGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid driverGuid {0}", driverGuid));
            var operations = _rightRepository.GetRights(owner, EntityType.CarRental);
            if (!operations.Contains(OperationType.Delete) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to add or update rental from car {1} by driver {2}", owner, carGuid, driverGuid));
            var carsGuid = _entityRepository.GetEntitys(owner, EntityType.Car);
            if (!carsGuid.Contains(carGuid))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to car {1} ", owner, carGuid));
            var driversGuid = _entityRepository.GetEntitys(owner, EntityType.Driver);
            if (!driversGuid.Contains(driverGuid))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to driver {1} ", owner, driverGuid));
            var rental =
                (from carRental in _dataContext.CarRentals
                    where carRental.CarGuid == carGuid && carRental.DriverGuid == driverGuid && !carRental.IsClose
                    select carRental).FirstOrDefault();
            if (rental == null)
                throw new InvalidDataException(string.Format("Rental from car {0} by driver {1} not found", carGuid, driverGuid));
            rental.EndRentalDate = DateTime.UtcNow;
            rental.IsClose = true;
            rental.IsDelete = true;
            _dataContext.SaveChanges();
            UpdateCache(new List<CarRental>(), new List<CarRental>(), new List<CarRental> {rental});
            _logger.Info(string.Format("Agent {0} delete rental to car {1} and driver {2}", owner, carGuid, driverGuid));
        }

        /// <summary>
        /// Функция возвращает последнюю аренду автомобиля
        /// </summary>
        /// <param name="owner">пользователь запрашивающий аренду</param>
        /// <param name="carGuid">идентификтор автомобиля</param>
        /// <returns></returns>
        public CarRental GetLastRentalByCar(Guid owner, Guid carGuid)
        {
            if (owner == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid owner {0}", owner));
            if (carGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid carGuid {0}", carGuid));
            var operations = _rightRepository.GetRights(owner, EntityType.CarRental);
            if (!operations.Contains(OperationType.Select) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to select rental from car {1}", owner, carGuid));
            var carsGuid = _entityRepository.GetEntitys(owner, EntityType.Car);
            if (!carsGuid.Contains(carGuid))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to car {1} ", owner, carGuid));
            using (_lock.UseReadLock())
            {
                List<CarRental> existRental;
                if (_rentalByCar.TryGetValue(carGuid, out existRental))
                {
                    return existRental.OrderBy(_ => _.StartRentalDate).Last();
                }
            }
            var rentals =
                (from carRental in _dataContext.CarRentals
                    where carRental.CarGuid == carGuid && !carRental.IsClose
                    select carRental).ToList();
            if (rentals.Count == 0)
            {
                throw new ObjectNotFoundException(string.Format("Rental not found {0} to car {1} ", owner, carGuid));
            }
            UpdateCache(rentals, new List<CarRental>(), new List<CarRental>());
            return rentals.OrderBy(_ => _.StartRentalDate).Last();
        }

        /// <summary>
        /// Функция возвращает последнюю аренду водителя
        /// </summary>
        /// <param name="owner">пользователь запрашивающий аренду</param>
        /// <param name="driverGuid">идентификтор водителя</param>
        /// <returns></returns>
        public CarRental GetLastRentalByDriver(Guid owner, Guid driverGuid)
        {
            if (owner == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid owner {0}", owner));
            if (driverGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid driverGuid {0}", driverGuid));
            var operations = _rightRepository.GetRights(owner, EntityType.CarRental);
            if (!operations.Contains(OperationType.Select) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to select rental from driver {1}", owner, driverGuid));
            var divers = _entityRepository.GetEntitys(owner, EntityType.Driver);
            if (!divers.Contains(driverGuid))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to driver {1} ", owner, driverGuid));
            using (_lock.UseReadLock())
            {
                List<CarRental> existRental;
                if (_rentalByDriver.TryGetValue(driverGuid, out existRental))
                {
                    return existRental.OrderBy(_ => _.StartRentalDate).Last();
                }
            }
            var rentals =
                (from carRental in _dataContext.CarRentals
                    where carRental.DriverGuid == driverGuid && !carRental.IsClose
                    select carRental).ToList();
            if (rentals.Count == 0)
            {
                throw new ObjectNotFoundException(string.Format("Rental not found {0} to driver {1} ", owner, driverGuid));
            }
            UpdateCache(rentals, new List<CarRental>(), new List<CarRental>());
            return rentals.OrderBy(_ => _.StartRentalDate).Last();
        }

        /// <summary>
        /// Функция возвращает аренды автомобиля
        /// </summary>
        /// <param name="owner">пользователь запрашивающий аренду</param>
        /// <param name="carGuid">идентификтор автомобиля</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">конец периода</param>
        /// <returns></returns>
        public IList<CarRental> GetRentalByCar(Guid owner, Guid carGuid, DateTime startDate, DateTime endDate)
        {
            if (owner == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid owner {0}", owner));
            if (carGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid carGuid {0}", carGuid));
            var operations = _rightRepository.GetRights(owner, EntityType.CarRental);
            if (!operations.Contains(OperationType.Select) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to select rental from car {1}", owner, carGuid));
            var carsGuid = _entityRepository.GetEntitys(owner, EntityType.Car);
            if (!carsGuid.Contains(carGuid))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to car {1} ", owner, carGuid));
            using (_lock.UseReadLock())
            {
                List<CarRental> existRental;
                if (_rentalByCar.TryGetValue(carGuid, out existRental))
                {
                    return existRental.Where(_ => startDate <= _.EndRentalDate && endDate >= _.StartRentalDate).ToList();
                }
            }
            var rentals =
                (from carRental in _dataContext.CarRentals
                    where carRental.CarGuid == carGuid && startDate <= carRental.EndRentalDate && endDate >= carRental.StartRentalDate && !carRental.IsClose
                    select carRental).ToList();
            if (rentals.Count == 0)
            {
                throw new ObjectNotFoundException(string.Format("Rental not found {0} to car {1} ", owner, carGuid));
            }
            UpdateCache(rentals, new List<CarRental>(), new List<CarRental>());
            return rentals;
        }

        /// <summary>
        /// Функция возвращает аренды водителя
        /// </summary>
        /// <param name="owner">пользователь запрашивающий аренду</param>
        /// <param name="driverGuid">идентификтор водителя</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">конец периода</param>
        /// <returns></returns>
        public IList<CarRental> GetRentalByDriver(Guid owner, Guid driverGuid, DateTime startDate, DateTime endDate)
        {
            if (owner == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid owner {0}", owner));
            if (driverGuid == Guid.Empty)
                throw new InvalidDataException(string.Format("Invalid driverGuid {0}", driverGuid));
            var operations = _rightRepository.GetRights(owner, EntityType.CarRental);
            if (!operations.Contains(OperationType.Select) && !operations.Contains(OperationType.Admin))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to select rental from driver {1}", owner, driverGuid));
            var divers = _entityRepository.GetEntitys(owner, EntityType.Driver);
            if (!divers.Contains(driverGuid))
                throw new InvalidDataException(string.Format("Agent {0} cannot access to driver {1} ", owner, driverGuid));
            using (_lock.UseReadLock())
            {
                List<CarRental> existRental;
                if (_rentalByDriver.TryGetValue(driverGuid, out existRental))
                {
                    return existRental.Where(_ => startDate <= _.EndRentalDate && endDate >= _.StartRentalDate).ToList();
                }
            }
            var rentals =
                (from carRental in _dataContext.CarRentals
                    where carRental.DriverGuid == driverGuid && startDate <= carRental.EndRentalDate && endDate >= carRental.StartRentalDate && !carRental.IsClose
                    select carRental).ToList();
            if (rentals.Count == 0)
            {
                throw new ObjectNotFoundException(string.Format("Rental not found {0} to driver {1} ", owner, driverGuid));
            }
            UpdateCache(rentals, new List<CarRental>(), new List<CarRental>());
            return rentals;
        }

        private void UpdateCache(List<CarRental> adding, List<CarRental> updating, List<CarRental> deleting)
        {
            using (_lock.UseWriteLock())
            {
                for (int i = 0; i < deleting.Count; i++)
                {
                    var carRental = deleting[i];
                    List<CarRental> rentals;
                    if (_rentalByDriver.TryGetValue(carRental.DriverGuid, out rentals))
                    {
                        var existRental = rentals.FirstOrDefault(_ => _.Id == carRental.Id);
                        if (existRental != null)
                            rentals.Remove(existRental);
                        if (rentals.Count == 0)
                            _rentalByDriver.Remove(carRental.DriverGuid);
                    }
                    if (_rentalByCar.TryGetValue(carRental.DriverGuid, out rentals))
                    {
                        var existRental = rentals.FirstOrDefault(_ => _.Id == carRental.Id);
                        if (existRental != null)
                            rentals.Remove(existRental);
                        if (rentals.Count == 0)
                            _rentalByCar.Remove(carRental.DriverGuid);
                    }
                }
                for (int i = 0; i < updating.Count; i++)
                {
                    var carRental = updating[i];
                    List<CarRental> rentals;
                    if (_rentalByDriver.TryGetValue(carRental.DriverGuid, out rentals))
                    {
                        var existRental = rentals.FirstOrDefault(_ => _.Id == carRental.Id);
                        if (existRental != null)
                            rentals.Remove(existRental);
                        rentals.Add(carRental);
                    }
                    else
                    {
                        rentals = new List<CarRental> {carRental};
                        _rentalByDriver.Add(carRental.DriverGuid, rentals);
                    }
                    if (_rentalByCar.TryGetValue(carRental.CarGuid, out rentals))
                    {
                        var existRental = rentals.FirstOrDefault(_ => _.Id == carRental.Id);
                        if (existRental != null)
                            rentals.Remove(existRental);
                        rentals.Add(carRental);
                    }
                    else
                    {
                        rentals = new List<CarRental> { carRental };
                        _rentalByCar.Add(carRental.CarGuid, rentals);
                    }
                }
                for (int i = 0; i < adding.Count; i++)
                {
                    var carRental = adding[i];
                    List<CarRental> rentals;
                    if (_rentalByDriver.TryGetValue(carRental.DriverGuid, out rentals))
                    {
                        rentals.Add(carRental);
                    }
                    else
                    {
                        rentals = new List<CarRental> { carRental };
                        _rentalByDriver.Add(carRental.DriverGuid, rentals);
                    }
                    if (_rentalByCar.TryGetValue(carRental.CarGuid, out rentals))
                    {
                        rentals.Add(carRental);
                    }
                    else
                    {
                        rentals = new List<CarRental> { carRental };
                        _rentalByCar.Add(carRental.CarGuid, rentals);
                    }
                }
            }
        }
    }
}
