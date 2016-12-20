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

namespace TaxiManager.Data.Test
{
    [TestClass]
    public class RightRepositoryTest
    {
        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException1()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            var mockIEntityRepository = new Mock<IEntityRepository>();
            new EntityFrameworkRightRepository(null, mockContext.Object, mockIEntityRepository.Object);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException2()
        {
            var mockIEntityRepository = new Mock<IEntityRepository>();
            new EntityFrameworkRightRepository(new NLogLoggerPlugin(), null, mockIEntityRepository.Object);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException3()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            new EntityFrameworkRightRepository(new NLogLoggerPlugin(), mockContext.Object, null);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void UpdateRights_OwnerEmpry()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            var mockIEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkRightRepository(new NLogLoggerPlugin(), mockContext.Object, mockIEntityRepository.Object);
            repository.UpdateRights(Guid.Empty, Guid.NewGuid(), EntityType.Agent, null);
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void UpdateRights_OperationTypeNull()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            var mockIEntityRepository = new Mock<IEntityRepository>();
            var repository = new EntityFrameworkRightRepository(new NLogLoggerPlugin(), mockContext.Object, mockIEntityRepository.Object);
            repository.UpdateRights(Guid.NewGuid(),Guid.NewGuid(), EntityType.Agent, null);
        }

        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void UpdateRights_AgentNotFound()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository entityRepository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var repository = new EntityFrameworkRightRepository(new NLogLoggerPlugin(), mockContext.Object, entityRepository);
          
            repository.UpdateRights(Guid.NewGuid(),Guid.NewGuid(), EntityType.Agent, new OperationType[0]);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void UpdateRights_NotRight()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository entityFrameworkEntityRepository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var agent2 = Guid.NewGuid();
            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));
            entityFrameworkEntityRepository.AddEntity(agent, agent1, EntityType.Agent);
            entityFrameworkEntityRepository.AddEntity(agent1, agent2, EntityType.Agent);
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>();
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            var repository = new EntityFrameworkRightRepository(new NLogLoggerPlugin(), mockContext.Object, entityFrameworkEntityRepository);
            repository.UpdateRights(agent1, agent2, EntityType.Agent, new OperationType[0]);
        }

        [TestMethod]
        public void UpdateRights()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository entityFrameworkEntityRepository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var agent2 = Guid.NewGuid();
            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));
            entityFrameworkEntityRepository.AddEntity(agent, agent1, EntityType.Agent);
            entityFrameworkEntityRepository.AddEntity(agent1, agent2, EntityType.Agent);
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent1,
                    EntityType = EntityType.Agent,
                    OperationTypes = new[] {OperationType.Admin}
                }
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            var repository = new EntityFrameworkRightRepository(new NLogLoggerPlugin(), mockContext.Object, entityFrameworkEntityRepository);
            repository.UpdateRights(agent1, agent2, EntityType.Agent, new OperationType[0]);
            repository.UpdateRights(agent1, agent2, EntityType.Agent, new[] { OperationType.Select });
        }

        [TestMethod]
        public void GetRights()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository entityFrameworkEntityRepository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var agent = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var agent2 = Guid.NewGuid();
            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));
            entityFrameworkEntityRepository.AddEntity(agent, agent1, EntityType.Agent);
            entityFrameworkEntityRepository.AddEntity(agent1, agent2, EntityType.Agent);
            var mockSet1 = new Mock<DbSet<Right>>();
            var data1 = new List<Right>
            {
                new Right
                {
                    AgentGuid = agent1,
                    EntityType = EntityType.Agent,
                    OperationTypes = new[] {OperationType.Admin}
                },
                new Right
                {
                    AgentGuid = agent,
                    EntityType = EntityType.Agent,
                    OperationTypes = new[] {OperationType.Admin}
                }
            };
            var queryable1 = data1.AsQueryable();
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Provider).Returns(queryable1.Provider);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.Expression).Returns(queryable1.Expression);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.ElementType).Returns(queryable1.ElementType);
            mockSet1.As<IQueryable<Right>>().Setup(m => m.GetEnumerator()).Returns(queryable1.GetEnumerator());
            mockSet1.Setup(d => d.Add(It.IsAny<Right>())).Callback<Right>(s => data1.Add(s));
            mockContext.Setup(m => m.Rights).Returns(mockSet1.Object);
            var repository = new EntityFrameworkRightRepository(new NLogLoggerPlugin(), mockContext.Object, entityFrameworkEntityRepository);
            repository.UpdateRights(agent1, agent2, EntityType.Agent, new[] {OperationType.Select});
            repository.UpdateRights(agent1, agent2, EntityType.Driver, new[] {OperationType.Select});
            repository.UpdateRights(agent1, agent2, EntityType.Car, new[] {OperationType.Select});
           
            var rights = repository.GetRights(agent2, EntityType.Agent);
            Assert.AreEqual(1, rights.Length);
            Assert.AreEqual(OperationType.Select, rights[0]);

            rights = repository.GetRights(agent2, EntityType.Driver);
            Assert.AreEqual(1, rights.Length);
            Assert.AreEqual(OperationType.Select, rights[0]);

            rights = repository.GetRights(agent2, EntityType.Car);
            Assert.AreEqual(1, rights.Length);
            Assert.AreEqual(OperationType.Select, rights[0]);

            repository.UpdateRights(agent1, agent2, EntityType.Car, new[] { OperationType.Delete, OperationType.AddOrUpdate });

            rights = repository.GetRights(agent2, EntityType.Car);
            Assert.AreEqual(2, rights.Length);
            Assert.AreEqual(OperationType.Delete, rights[0]);
            Assert.AreEqual(OperationType.AddOrUpdate, rights[1]);

            repository.UpdateRights(agent, agent1, EntityType.Agent, new[] { OperationType.Select });

            try
            {
                repository.UpdateRights(agent1, agent2, EntityType.Car, new[] { OperationType.Delete, OperationType.AddOrUpdate });
                Assert.Fail();
            }
            catch (InvalidDataException)
            {
            }
        }
      
        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void GetRights_AgentEmpty()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository entityFrameworkEntityRepository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var repository = new EntityFrameworkRightRepository(new NLogLoggerPlugin(), mockContext.Object, entityFrameworkEntityRepository);
            repository.GetRights(Guid.Empty, EntityType.Agent);
        }
    }
}
