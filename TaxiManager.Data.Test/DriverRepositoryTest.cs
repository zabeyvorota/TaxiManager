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
    public class DriverRepositoryTest
    {
        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException1()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var mockRightRepository = new Mock<IRightRepository>();
            new EntityFrameworkDriverRepository(null, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException2()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            new EntityFrameworkDriverRepository(mockLogger.Object, null, mockRightRepository.Object, mockContext.Object);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException3()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            new EntityFrameworkDriverRepository(mockLogger.Object, mockEntityRepository.Object, null, mockContext.Object);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException4()
        {
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            new EntityFrameworkDriverRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, null);
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateDriver_DriverGuidEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkDriverRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
            repository.AddOrUpdateDriver(Guid.Empty, new Driver());
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateDriver_DriverNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkDriverRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
            repository.AddOrUpdateDriver(Guid.NewGuid(), null);
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateDriver_DriverSurnameNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkDriverRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
            repository.AddOrUpdateDriver(Guid.NewGuid(), new Driver());
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateDriver_DriverNameNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkDriverRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
            repository.AddOrUpdateDriver(Guid.NewGuid(), new Driver{Surname="1"});
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateDriver_DriverBirthdayEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkDriverRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
            repository.AddOrUpdateDriver(Guid.NewGuid(), new Driver { Surname = "1",Name="2" });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateDrivert_CannotAccesToAdd()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);

            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Driver,
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
            repository.AddOrUpdateDriver(agent, new Driver
            {
                Id = 1,
                Surname = "1",
                Name="2",
                Birthday=new DateTime(1980,1,1)
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateDriver_UpdateWithEmptyGuid()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Driver,
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
            var mockSet = new Mock<DbSet<Driver>>();
            var data = new List<Driver>
            {
                new Driver
                {
                    Id = 1,
                    Surname = "1",
                    Name = "2",
                    Birthday = new DateTime(1980, 1, 1)
                }
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Driver>())).Callback<Driver>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Drivers).Returns(mockSet.Object);
            repository.AddOrUpdateDriver(agent, new Driver
            {
                Id = 1,
                Surname = "1",
                Name = "2",
                Birthday = new DateTime(1980, 1, 1)
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateDriver_NotAcceptToUpdate()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var newGuid = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Driver,
                    OperationTypes = new[] {OperationType.AddOrUpdate}
                }
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            var mockSet = new Mock<DbSet<Driver>>();
            var data = new List<Driver>
            {
                new Driver
                {
                    Id = 1,
                    Surname = "1",
                    Name = "2",
                    Birthday = new DateTime(1980, 1, 1)
                }
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Driver>())).Callback<Driver>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int) ids[0]));
            mockContext.Setup(m => m.Drivers).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = Guid.NewGuid(),
                    EntityType = EntityType.Driver
                }
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.AddOrUpdateDriver(agent, new Driver
            {
                Id = 1,
                Surname = "1",
                Name = "2",
                Birthday = new DateTime(1980, 1, 1),
                Guid = newGuid
            });
        }

        [TestMethod]
        public void AddOrUpdateDriver_Add()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Driver,
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
            var mockSet = new Mock<DbSet<Driver>>();
            var data = new List<Driver>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Driver>())).Callback<Driver>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int) ids[0]));
            mockContext.Setup(m => m.Drivers).Returns(mockSet.Object);
            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>();
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            var addOrUpdateAgent = repository.AddOrUpdateDriver(agent, new Driver
            {
                Id = 1,
                Surname = "1",
                Name = "2",
                Birthday = new DateTime(1980, 1, 1)
            });

            var agents = entityRepository.GetEntitys(agent, EntityType.Driver);
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
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var newGuid = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Driver,
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
            var mockSet = new Mock<DbSet<Driver>>();
            var data = new List<Driver>
            {
                new Driver
                {
                    Id = 1,
                    Surname = "1",
                    Name = "2",
                    Birthday = new DateTime(1980, 1, 1)
                }
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Driver>())).Callback<Driver>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int) ids[0]));
            mockContext.Setup(m => m.Drivers).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = newGuid,
                    EntityType = EntityType.Driver
                }
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.AddOrUpdateDriver(agent, new Driver
            {
                Id = 1,
                Surname = "1",
                Name = "2",
                Birthday = new DateTime(1980, 1, 1),
                Guid = newGuid
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteDriver_GuidAgentEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteDriver(Guid.Empty, new Driver());
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteDriver_DriverNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteDriver(Guid.NewGuid(), null);
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteDriver_IdNotValid()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteDriver(Guid.NewGuid(), new Driver());
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteDriver_DriverGuidNotValid()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteDriver(Guid.NewGuid(), new Driver
            {
                Id = 1
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteDriver_NotAcceptToDelete()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var newGuid = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Driver,
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
            var mockSet = new Mock<DbSet<Driver>>();
            var data = new List<Driver>
            {
                new Driver
                {
                    Id = 1,
                    Surname = "1",
                    Name = "2",
                    Birthday = new DateTime(1980, 1, 1),
                    Guid = newGuid
                }
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Driver>())).Callback<Driver>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int) ids[0]));
            mockContext.Setup(m => m.Drivers).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = Guid.NewGuid(),
                    EntityType = EntityType.Driver

                },
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.DeleteDriver(agent, new Driver
            {
                Id = 1,
                Surname = "1",
                Name = "2",
                Birthday = new DateTime(1980, 1, 1),
                Guid = newGuid
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteDriver_NotRightToDelete()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var newGuid = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Driver,
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
            var mockSet = new Mock<DbSet<Driver>>();
            var data = new List<Driver>
            {
                new Driver
                {
                    Id = 1,
                    Surname = "1",
                    Name = "2",
                    Birthday = new DateTime(1980, 1, 1),
                    Guid = newGuid
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Driver>())).Callback<Driver>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int) ids[0]));
            mockContext.Setup(m => m.Drivers).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = newGuid,
                    EntityType = EntityType.Driver

                },
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.DeleteDriver(agent, new Driver
            {
                Id = 1,
                Surname = "1",
                Name = "2",
                Birthday = new DateTime(1980, 1, 1),
                Guid = newGuid
            });
        }

        [TestMethod]
        public void DeleteDriver()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var newGuid = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Driver,
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
            var mockSet = new Mock<DbSet<Driver>>();
            var data = new List<Driver>
            {
                new Driver
                {
                    Id = 1,
                    Surname = "1",
                    Name = "2",
                    Birthday = new DateTime(1980, 1, 1),
                    Guid = newGuid
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Driver>())).Callback<Driver>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int) ids[0]));
            mockContext.Setup(m => m.Drivers).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = newGuid,
                    EntityType = EntityType.Driver

                },
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.DeleteDriver(agent, new Driver
            {
                Id = 1,
                Surname = "1",
                Name = "2",
                Birthday = new DateTime(1980, 1, 1),
                Guid = newGuid
            });

            var agents = entityRepository.GetEntitys(agent, EntityType.Driver);
            Assert.AreEqual(0, agents.Count);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetDriversByGuids_GuidsEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.GetDriversByGuids(Guid.NewGuid(), new List<Guid>());
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetDriversByGuids_GuidsNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.GetDriversByGuids(Guid.NewGuid(), null);
        }
       
        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetDriversByGuids_GuidAgentEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.GetDriversByGuids(Guid.Empty, new List<Guid> { Guid.NewGuid() });
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetDriversByGuids_NotAccessAfterDelete()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Driver,
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

            var mockSet = new Mock<DbSet<Driver>>();
            var data = new List<Driver>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Driver>())).Callback<Driver>(s =>
                                                                         {
                                                                             s.Id++;
                                                                             data.Add(s);
                                                                         });
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Drivers).Returns(mockSet.Object);
            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>();
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            var addOrUpdateDriver = repository.AddOrUpdateDriver(agent, new Driver
            {
                Id = 1,
                Surname = "1",
                Name = "2",
                Birthday = new DateTime(1980, 1, 1),
            });

            var agents = entityRepository.GetEntitys(agent, EntityType.Driver);
            Assert.AreEqual(1, agents.Count);
            Assert.AreEqual(addOrUpdateDriver.Guid, agents[0]);

            var driversByGuids = repository.GetDriversByGuids(agent, new List<Guid> { addOrUpdateDriver.Guid });
            Assert.AreEqual(1, driversByGuids.Count);
            Assert.AreEqual("1", driversByGuids[0].Surname);
            Assert.AreEqual("2", driversByGuids[0].Name);
            Assert.AreEqual(new DateTime(1980, 1, 1), driversByGuids[0].Birthday);
            Assert.AreEqual(addOrUpdateDriver.Id, driversByGuids[0].Id);
            Assert.AreEqual(addOrUpdateDriver.Guid, driversByGuids[0].Guid);
            addOrUpdateDriver.Name = "22";
            repository.AddOrUpdateDriver(agent, addOrUpdateDriver);

            driversByGuids = repository.GetDriversByGuids(agent, new List<Guid> { addOrUpdateDriver.Guid });
            Assert.AreEqual(1, driversByGuids.Count);
            Assert.AreEqual("22", driversByGuids[0].Name);
            Assert.AreEqual(addOrUpdateDriver.Id, driversByGuids[0].Id);
            Assert.AreEqual(addOrUpdateDriver.Guid, driversByGuids[0].Guid);

            repository.DeleteDriver(agent, addOrUpdateDriver);

            driversByGuids = repository.GetDriversByGuids(agent, new List<Guid> { addOrUpdateDriver.Guid });
            Assert.AreEqual(0, driversByGuids.Count);
        }

        [TestMethod]
        public void GetDriversByGuids()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkDriverRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Driver,
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

            var mockSet = new Mock<DbSet<Driver>>();
            var data = new List<Driver>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Driver>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Driver>())).Callback<Driver>(s =>
            {
                s.Id++;
                data.Add(s);
            });
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Drivers).Returns(mockSet.Object);
            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>();
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            var addOrUpdateDriver = repository.AddOrUpdateDriver(agent, new Driver
            {
                Surname = "1",
                Name = "2",
                Birthday = new DateTime(1980, 1, 1),
            });

            var drivers = entityRepository.GetEntitys(agent, EntityType.Driver);
            Assert.AreEqual(1, drivers.Count);
            Assert.AreEqual(addOrUpdateDriver.Guid, drivers[0]);

            var driversByGuids = repository.GetDriversByGuids(agent, new List<Guid> { addOrUpdateDriver.Guid });
            Assert.AreEqual(1, driversByGuids.Count);
            Assert.AreEqual("1", driversByGuids[0].Surname);
            Assert.AreEqual("2", driversByGuids[0].Name);
            Assert.AreEqual(new DateTime(1980, 1, 1), driversByGuids[0].Birthday);
            Assert.AreEqual(addOrUpdateDriver.Id, driversByGuids[0].Id);
            Assert.AreEqual(addOrUpdateDriver.Guid, driversByGuids[0].Guid);
            addOrUpdateDriver.Name = "22";
            repository.AddOrUpdateDriver(agent, addOrUpdateDriver);

            driversByGuids = repository.GetDriversByGuids(agent, new List<Guid> { addOrUpdateDriver.Guid });
            Assert.AreEqual(1, driversByGuids.Count);
            Assert.AreEqual("22", driversByGuids[0].Name);
            Assert.AreEqual(addOrUpdateDriver.Id, driversByGuids[0].Id);
            Assert.AreEqual(addOrUpdateDriver.Guid, driversByGuids[0].Guid);

            repository.DeleteDriver(agent, addOrUpdateDriver);
            drivers = entityRepository.GetEntitys(agent, EntityType.Driver);
            Assert.AreEqual(0, drivers.Count);
        }
    }
}
