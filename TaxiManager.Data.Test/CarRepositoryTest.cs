using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TaxiManager.Data.EntityFramework;
using TaxiManager.Data.Model;
using TaxiManager.NLog;
using Moq;

using TaxiManager.Core;

namespace TaxiManager.Data.Test
{
    [TestClass]
    public class CarRepositoryTest
    {
        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException1()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var mockRightRepository = new Mock<IRightRepository>();
            new EntityFrameworkCarRepository(null, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException2()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            new EntityFrameworkCarRepository(mockLogger.Object, null, mockRightRepository.Object, mockContext.Object);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException3()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            new EntityFrameworkCarRepository(mockLogger.Object, mockEntityRepository.Object, null, mockContext.Object);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException4()
        {
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            new EntityFrameworkCarRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, null);
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateCar_CarGuidEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkCarRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
            repository.AddOrUpdateCar(Guid.Empty, new Car());
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateCar_CarNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkCarRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
            repository.AddOrUpdateCar(Guid.NewGuid(), null);
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateCar_CarNumberNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkCarRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
            repository.AddOrUpdateCar(Guid.NewGuid(), new Car());
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateCart_CannotAccesToAdd()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);

            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Car,
                    OperationTypes = new[] {OperationType.Select}
                },
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            repository.AddOrUpdateCar(agent, new Car
            {
                Id = 1,
                Number = "1"
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateCar_UpdateWithEmptyGuid()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Car,
                    OperationTypes = new[] {OperationType.AddOrUpdate}
                },
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            var mockSet = new Mock<DbSet<Car>>();
            var data = new List<Car>
            {
                new Car
                {
                    Id = 1,
                    Number = "1"
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Car>())).Callback<Car>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Cars).Returns(mockSet.Object);
            repository.AddOrUpdateCar(agent, new Car
            {
                Id = 1,
                Number = "1",
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateCar_NotAcceptToUpdate()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Car,
                    OperationTypes = new[] {OperationType.AddOrUpdate}
                },
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            var mockSet = new Mock<DbSet<Car>>();
            var data = new List<Car>
            {
                new Car
                {
                    Id = 1,
                    Number = "1"
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Car>())).Callback<Car>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Cars).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = Guid.NewGuid(),
                    EntityType = EntityType.Car
                }
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.AddOrUpdateCar(agent, new Car
            {
                Id = 1,
                Number = "1",
                Guid = agent1
            });
        }

        [TestMethod]
        public void AddOrUpdateCar_Add()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
           var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Car,
                    OperationTypes = new[] {OperationType.AddOrUpdate}
                },
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            var mockSet = new Mock<DbSet<Car>>();
            var data = new List<Car>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Car>())).Callback<Car>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Cars).Returns(mockSet.Object);
            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>();
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            var addOrUpdateAgent = repository.AddOrUpdateCar(agent, new Car
            {
                Id = 0,
                Number = "1",
            });

            var agents = entityRepository.GetEntitys(agent, EntityType.Car);
            Assert.AreEqual(1, agents.Count);
            Assert.AreEqual(addOrUpdateAgent.Guid, agents[0]);
        }

        [TestMethod]
        public void AddOrUpdateAgent_Update()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Car,
                    OperationTypes = new[] {OperationType.AddOrUpdate}
                },
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            var mockSet = new Mock<DbSet<Car>>();
            var data = new List<Car>
            {
                new Car
                {
                    Id = 1,
                    Number = "1"
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Car>())).Callback<Car>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Cars).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = agent1,
                    EntityType = EntityType.Car
                }
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.AddOrUpdateCar(agent, new Car
            {
                Id = 1,
                Number = "22",
                Guid = agent1
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteCar_GuidAgentEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteCar(Guid.Empty, new Car());
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteCar_CarNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteCar(Guid.NewGuid(), null);
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteCar_IdNotValid()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteCar(Guid.NewGuid(), new Car());
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteCar_CarGuidNotValid()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteCar(Guid.NewGuid(), new Car
            {
                Id = 1
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteCar_NotAcceptToDelete()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Car,
                    OperationTypes = new[] {OperationType.Delete}
                },
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            var mockSet = new Mock<DbSet<Car>>();
            var data = new List<Car>
            {
                new Car
                {
                    Id = 1,
                    Number = "1",
                    Guid = agent1
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Car>())).Callback<Car>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Cars).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = Guid.NewGuid(),
                    EntityType = EntityType.Car

                },
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.DeleteCar(agent, new Car
            {
                Id = 1,
                Number = "1",
                Guid = agent1
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteCar_NotRightToDelete()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Car,
                    OperationTypes = new[] {OperationType.Select}
                },
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            var mockSet = new Mock<DbSet<Car>>();
            var data = new List<Car>
            {
                new Car
                {
                    Id = 1,
                    Number = "1",
                    Guid = agent1
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Car>())).Callback<Car>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Cars).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = agent1,
                    EntityType = EntityType.Car

                },
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.DeleteCar(agent, new Car
            {
                Id = 1,
                Number = "1",
                Guid = agent1
            });
        }

        [TestMethod]
        public void DeleteCar()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Car,
                    OperationTypes = new[] {OperationType.Delete}
                },
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            var mockSet = new Mock<DbSet<Car>>();
            var data = new List<Car>
            {
                new Car
                {
                    Id = 1,
                    Number = "1",
                    Guid = agent1
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Car>())).Callback<Car>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Cars).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = agent1,
                    EntityType = EntityType.Car

                },
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.DeleteCar(agent, new Car
            {
                Id = 1,
                Number = "1",
                Guid = agent1
            });

            var agents = entityRepository.GetEntitys(agent, EntityType.Car);
            Assert.AreEqual(0, agents.Count);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetCarsByGuids_GuidsEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.GetCarsByGuids(Guid.NewGuid(), new List<Guid>());
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetCarsByGuids_GuidsNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.GetCarsByGuids(Guid.NewGuid(), null);
        }
       
        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetCarsByGuids_GuidAgentEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.GetCarsByGuids(Guid.Empty, new List<Guid> { Guid.NewGuid() });
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetCarsByGuids_NotAccessAfterDelete()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Car,
                    OperationTypes = new[] {OperationType.AddOrUpdate,OperationType.Select,OperationType.Delete}
                },
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);

            var mockSet = new Mock<DbSet<Car>>();
            var data = new List<Car>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Car>())).Callback<Car>(s =>
                                                                         {
                                                                             s.Id++;
                                                                             data.Add(s);
                                                                         });
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Cars).Returns(mockSet.Object);
            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>();
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            var addOrUpdateCar = repository.AddOrUpdateCar(agent, new Car
            {
                Id = 0,
                Number = "1",
            });

            var agents = entityRepository.GetEntitys(agent, EntityType.Car);
            Assert.AreEqual(1, agents.Count);
            Assert.AreEqual(addOrUpdateCar.Guid, agents[0]);

            var carsByGuids = repository.GetCarsByGuids(agent, new List<Guid> { addOrUpdateCar.Guid });
            Assert.AreEqual(1, carsByGuids.Count);
            Assert.AreEqual("1", carsByGuids[0].Number);
            Assert.AreEqual(addOrUpdateCar.Id, carsByGuids[0].Id);
            Assert.AreEqual(addOrUpdateCar.Guid, carsByGuids[0].Guid);
            addOrUpdateCar.Number = "22";
            repository.AddOrUpdateCar(agent, addOrUpdateCar);

            carsByGuids = repository.GetCarsByGuids(agent, new List<Guid> { addOrUpdateCar.Guid });
            Assert.AreEqual(1, carsByGuids.Count);
            Assert.AreEqual("22", carsByGuids[0].Number);
            Assert.AreEqual(addOrUpdateCar.Id, carsByGuids[0].Id);
            Assert.AreEqual(addOrUpdateCar.Guid, carsByGuids[0].Guid);

            repository.DeleteCar(agent, addOrUpdateCar);

            carsByGuids = repository.GetCarsByGuids(agent, new List<Guid> { addOrUpdateCar.Guid });
            Assert.AreEqual(0, carsByGuids.Count);
        }

        [TestMethod]
        public void GetCarsByGuids()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkCarRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Car,
                    OperationTypes = new[] {OperationType.AddOrUpdate,OperationType.Select,OperationType.Delete}
                },
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);

            var mockSet = new Mock<DbSet<Car>>();
            var data = new List<Car>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Car>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Car>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Car>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Car>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Car>())).Callback<Car>(s =>
            {
                s.Id++;
                data.Add(s);
            });
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Cars).Returns(mockSet.Object);
            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>();
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            var addOrUpdateCar = repository.AddOrUpdateCar(agent, new Car
            {
                Id = 0,
                Number = "1",
            });

            var cars = entityRepository.GetEntitys(agent, EntityType.Car);
            Assert.AreEqual(1, cars.Count);
            Assert.AreEqual(addOrUpdateCar.Guid, cars[0]);

            var carsByGuids = repository.GetCarsByGuids(agent, new List<Guid> { addOrUpdateCar.Guid });
            Assert.AreEqual(1, carsByGuids.Count);
            Assert.AreEqual("1", carsByGuids[0].Number);
            Assert.AreEqual(addOrUpdateCar.Id, carsByGuids[0].Id);
            Assert.AreEqual(addOrUpdateCar.Guid, carsByGuids[0].Guid);
            addOrUpdateCar.Number = "22";
            repository.AddOrUpdateCar(agent, addOrUpdateCar);

            carsByGuids = repository.GetCarsByGuids(agent, new List<Guid> { addOrUpdateCar.Guid });
            Assert.AreEqual(1, carsByGuids.Count);
            Assert.AreEqual("22", carsByGuids[0].Number);
            Assert.AreEqual(addOrUpdateCar.Id, carsByGuids[0].Id);
            Assert.AreEqual(addOrUpdateCar.Guid, carsByGuids[0].Guid);

            repository.DeleteCar(agent, addOrUpdateCar);
            cars = entityRepository.GetEntitys(agent, EntityType.Car);
            Assert.AreEqual(0, cars.Count);
        }
    }
}
