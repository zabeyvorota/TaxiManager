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
    public class AgentRepositoryTest
    {
        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException1()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var mockRightRepository = new Mock<IRightRepository>();
            new EntityFrameworkAgentRepository(null, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException2()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            new EntityFrameworkAgentRepository(mockLogger.Object, null, mockRightRepository.Object, mockContext.Object);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException3()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            new EntityFrameworkAgentRepository(mockLogger.Object, mockEntityRepository.Object, null, mockContext.Object);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException4()
        {
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            new EntityFrameworkAgentRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, null);
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateAgent_AgentGuidEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkAgentRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
            repository.AddOrUpdateAgent(Guid.Empty, new Agent());
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateAgent_AgentNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkAgentRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
            repository.AddOrUpdateAgent(Guid.NewGuid(), null);
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateAgent_AgentNameNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var mockLogger = new Mock<ILogger>();
            var mockRightRepository = new Mock<IRightRepository>();
            var mockEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkAgentRepository(mockLogger.Object, mockEntityRepository.Object, mockRightRepository.Object, mockContext.Object);
            repository.AddOrUpdateAgent(Guid.NewGuid(), new Agent());
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateAgent_CannotAccesToAdd()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);

            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Agent,
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
            repository.AddOrUpdateAgent(agent, new Agent
            {
                Id = 1,
                Name = "1"
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateAgent_UpdateWithEmptyGuid()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Agent,
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
            var mockSet = new Mock<DbSet<Agent>>();
            var data = new List<Agent>
            {
                new Agent
                {
                    Id = 1,
                    Name = "1"
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Agent>())).Callback<Agent>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Agents).Returns(mockSet.Object);
            repository.AddOrUpdateAgent(agent, new Agent
            {
                Id = 1,
                Name = "1",
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void AddOrUpdateAgent_NotAcceptToUpdate()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Agent,
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
            var mockSet = new Mock<DbSet<Agent>>();
            var data = new List<Agent>
            {
                new Agent
                {
                    Id = 1,
                    Name = "1"
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Agent>())).Callback<Agent>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Agents).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = Guid.NewGuid(),
                    EntityType = EntityType.Agent
                }
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.AddOrUpdateAgent(agent, new Agent
            {
                Id = 1,
                Name = "1",
                Guid = agent1
            });
        }

        [TestMethod]
        public void AddOrUpdateAgent_Add()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Agent,
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
            var mockSet = new Mock<DbSet<Agent>>();
            var data = new List<Agent>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Agent>())).Callback<Agent>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Agents).Returns(mockSet.Object);
            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>();
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

           var addOrUpdateAgent= repository.AddOrUpdateAgent(agent, new Agent
            {
                Id = 0,
                Name = "1",
            });

            var agents = entityRepository.GetEntitys(agent, EntityType.Agent);
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
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Agent,
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
            var mockSet = new Mock<DbSet<Agent>>();
            var data = new List<Agent>
            {
                new Agent
                {
                    Id = 1,
                    Name = "1"
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Agent>())).Callback<Agent>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Agents).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = agent1,
                    EntityType = EntityType.Agent
                }
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.AddOrUpdateAgent(agent, new Agent
            {
                Id = 1,
                Name = "22",
                Guid = agent1
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteAgent_GuidAgentEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteAgent(Guid.Empty, new Agent());
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteAgent_AgentNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteAgent(Guid.NewGuid(), null);
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteAgent_IdNotValid()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteAgent(Guid.NewGuid(), new Agent());
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteAgent_AgentGuidNotValid()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.DeleteAgent(Guid.NewGuid(), new Agent
            {
                Id = 1
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteAgent_NotAcceptToDelete()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Agent,
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
            var mockSet = new Mock<DbSet<Agent>>();
            var data = new List<Agent>
            {
                new Agent
                {
                    Id = 1,
                    Name = "1",
                    Guid = agent1
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Agent>())).Callback<Agent>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Agents).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = Guid.NewGuid(),
                    EntityType = EntityType.Agent

                },
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.DeleteAgent(agent, new Agent
            {
                Id = 1,
                Name = "1",
                Guid = agent1
            });
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void DeleteAgent_NotRightToDelete()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Agent,
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
            var mockSet = new Mock<DbSet<Agent>>();
            var data = new List<Agent>
            {
                new Agent
                {
                    Id = 1,
                    Name = "1",
                    Guid = agent1
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Agent>())).Callback<Agent>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Agents).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = agent1,
                    EntityType = EntityType.Agent

                },
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.DeleteAgent(agent, new Agent
            {
                Id = 1,
                Name = "1",
                Guid = agent1
            });
        }

        [TestMethod]
        public void DeleteAgent()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Agent,
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
            var mockSet = new Mock<DbSet<Agent>>();
            var data = new List<Agent>
            {
                new Agent
                {
                    Id = 1,
                    Name = "1",
                    Guid = agent1
                },
            };
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Agent>())).Callback<Agent>(s => data.Add(s));
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Agents).Returns(mockSet.Object);


            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>
            {
                new EntityGuids
                {
                    Id = 1,
                    AgentGuid = agent,
                    EntityGuid = agent1,
                    EntityType = EntityType.Agent

                },
            };
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            repository.DeleteAgent(agent, new Agent
            {
                Id = 1,
                Name = "1",
                Guid = agent1
            });

            var agents = entityRepository.GetEntitys(agent, EntityType.Agent);
            Assert.AreEqual(0, agents.Count);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetAgentsByGuids_GuidsEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.GetAgentsByGuids(Guid.NewGuid(),new List<Guid>());
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetAgentsByGuids_GuidsNull()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.GetAgentsByGuids(Guid.NewGuid(), null);
        }
       
        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetAgentsByGuids_GuidAgentEmpty()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            repository.GetAgentsByGuids(Guid.Empty, new List<Guid> { Guid.NewGuid() });
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetAgentsByGuids_NotAccessAfterDelete()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Agent,
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
            
            var mockSet = new Mock<DbSet<Agent>>();
            var data = new List<Agent>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Agent>())).Callback<Agent>(s =>
                                                                         {
                                                                             s.Id++;
                                                                             data.Add(s);
                                                                         });
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Agents).Returns(mockSet.Object);
            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>();
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            var newAgent = repository.AddOrUpdateAgent(agent, new Agent
            {
                Id = 0,
                Name = "1",
            });

            var agents = entityRepository.GetEntitys(agent, EntityType.Agent);
            Assert.AreEqual(1, agents.Count);
            Assert.AreEqual(newAgent.Guid, agents[0]);

            var agentsByGuids = repository.GetAgentsByGuids(agent,new List<Guid> {newAgent.Guid});
            Assert.AreEqual(1, agentsByGuids.Count);
            Assert.AreEqual("1", agentsByGuids[0].Name);
            Assert.AreEqual(newAgent.Id, agentsByGuids[0].Id);
            Assert.AreEqual(newAgent.Guid, agentsByGuids[0].Guid);
            newAgent.Name = "22";
            repository.AddOrUpdateAgent(agent, newAgent);

            agentsByGuids = repository.GetAgentsByGuids(agent, new List<Guid> { newAgent.Guid });
            Assert.AreEqual(1, agentsByGuids.Count);
            Assert.AreEqual("22", agentsByGuids[0].Name);
            Assert.AreEqual(newAgent.Id, agentsByGuids[0].Id);
            Assert.AreEqual(newAgent.Guid, agentsByGuids[0].Guid);

            repository.DeleteAgent(agent, newAgent);

            agentsByGuids = repository.GetAgentsByGuids(agent, new List<Guid> { newAgent.Guid });
            Assert.AreEqual(0, agentsByGuids.Count);
        }

        [TestMethod]
        public void GetAgentsByGuids()
        {
            var mockContext = new Mock<ApplicationContext>();
            var logger = new NLogLoggerService();
            var entityRepository = new EntityFrameworkEntityRepository(logger, mockContext.Object);
            var rightRepository = new EntityFrameworkRightRepository(logger, mockContext.Object, entityRepository);
            var repository = new EntityFrameworkAgentRepository(logger, entityRepository, rightRepository, mockContext.Object);
            var agent = Guid.NewGuid();
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Agent,
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

            var mockSet = new Mock<DbSet<Agent>>();
            var data = new List<Agent>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Agent>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Agent>())).Callback<Agent>(s =>
            {
                s.Id++;
                data.Add(s);
            });
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => data.FirstOrDefault(d => d.Id == (int)ids[0]));
            mockContext.Setup(m => m.Agents).Returns(mockSet.Object);
            var mockSet2 = new Mock<DbSet<EntityGuids>>();
            var data2 = new List<EntityGuids>();
            var queryable2 = data2.AsQueryable();
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable2.Provider);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable2.Expression);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable2.ElementType);
            mockSet2.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable2.GetEnumerator());
            mockSet2.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data2.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet2.Object);

            var newAgent = repository.AddOrUpdateAgent(agent, new Agent
            {
                Id = 0,
                Name = "1",
            });

            var agents = entityRepository.GetEntitys(agent, EntityType.Agent);
            Assert.AreEqual(1, agents.Count);
            Assert.AreEqual(newAgent.Guid, agents[0]);

            var agentsByGuids = repository.GetAgentsByGuids(agent, new List<Guid> { newAgent.Guid });
            Assert.AreEqual(1, agentsByGuids.Count);
            Assert.AreEqual("1", agentsByGuids[0].Name);
            Assert.AreEqual(newAgent.Id, agentsByGuids[0].Id);
            Assert.AreEqual(newAgent.Guid, agentsByGuids[0].Guid);
            newAgent.Name = "22";
            repository.AddOrUpdateAgent(agent, newAgent);

            agentsByGuids = repository.GetAgentsByGuids(agent, new List<Guid> { newAgent.Guid });
            Assert.AreEqual(1, agentsByGuids.Count);
            Assert.AreEqual("22", agentsByGuids[0].Name);
            Assert.AreEqual(newAgent.Id, agentsByGuids[0].Id);
            Assert.AreEqual(newAgent.Guid, agentsByGuids[0].Guid);

            repository.DeleteAgent(agent, newAgent);
             agents = entityRepository.GetEntitys(agent, EntityType.Agent);
            Assert.AreEqual(0, agents.Count);
        }
    }
}
