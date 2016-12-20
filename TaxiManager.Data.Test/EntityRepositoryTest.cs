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
    public class EntityRepositoryTest
    {
        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException1()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);

            new EntityFrameworkEntityRepository(null, mockContext.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void ArgumentNullException2()
        {
            new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), null);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void AddEntity_AgentGuidEmpty()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            repository.AddEntity(Guid.Empty, Guid.NewGuid(), EntityType.Car);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void AddEntity_EntityGuidEmpty()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            repository.AddEntity(Guid.NewGuid(), Guid.Empty, EntityType.Car);
        }

        [TestMethod]
        public void AddEntity()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));
            
            repository.AddEntity(Guid.NewGuid(), Guid.NewGuid(), EntityType.Car);
            mockSet.Verify(m => m.Add(It.IsAny<EntityGuids>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        [TestMethod]
        public void DeleteEntity()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var agent = Guid.NewGuid();
            var entity = Guid.NewGuid();
            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));
            repository.AddEntity(agent, entity, EntityType.Car);

            repository.DeleteEntity(agent, entity, EntityType.Car);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void DeleteEntity_AgentGuidEmpty()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            repository.DeleteEntity(Guid.Empty, Guid.NewGuid(), EntityType.Car);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void DeleteEntity_EntityGuidEmpty()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            repository.DeleteEntity(Guid.NewGuid(), Guid.Empty, EntityType.Car);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void DeleteEntity_EntityNotFound()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);

            var data = new List<EntityGuids>
            {
                new EntityGuids {AgentGuid = Guid.NewGuid(), EntityGuid = Guid.NewGuid(), EntityType = EntityType.Car},
            }.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            repository.DeleteEntity(Guid.NewGuid(), Guid.NewGuid(), EntityType.Car);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void DeleteEntity_EntityTypeError()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var agent = Guid.NewGuid();
            var entity = Guid.NewGuid();
            var data = new List<EntityGuids>
            {
                new EntityGuids {AgentGuid = agent, EntityGuid = entity, EntityType = EntityType.Car},
            }.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            repository.DeleteEntity(agent, entity, EntityType.Driver);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void DeleteEntity_AgentError()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var agent = Guid.NewGuid();
            var entity = Guid.NewGuid();
            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));
            repository.AddEntity(agent, entity, EntityType.Car);
            repository.DeleteEntity(Guid.NewGuid(), entity, EntityType.Car);
        }

        [TestMethod]
        public void GetEntitys_Simple()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var agent = Guid.NewGuid();
            var entity = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var entity1 = Guid.NewGuid();
            var entity2 = Guid.NewGuid();
            var entity3 = Guid.NewGuid();

            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));
          
            repository.AddEntity(agent, entity, EntityType.Car);
            repository.AddEntity(agent1, entity1, EntityType.Car);

            var guids = repository.GetEntitys(agent, EntityType.Car);
            Assert.AreEqual(1, guids.Count);
            Assert.AreEqual(entity, guids[0]);
            guids = repository.GetEntitys(agent1, EntityType.Car);
            Assert.AreEqual(1, guids.Count);
            Assert.AreEqual(entity1, guids[0]);

            repository.DeleteEntity(agent, entity, EntityType.Car);
            guids = repository.GetEntitys(agent, EntityType.Car);
            Assert.AreEqual(0, guids.Count);
            guids = repository.GetEntitys(agent1, EntityType.Car);
            Assert.AreEqual(1, guids.Count);
            Assert.AreEqual(entity1, guids[0]);
            repository.AddEntity(agent1, entity3, EntityType.Driver);
            repository.AddEntity(agent1, entity2, EntityType.Driver);
            guids = repository.GetEntitys(agent1, EntityType.Car);
            Assert.AreEqual(1, guids.Count);
            guids = repository.GetEntitys(agent1, EntityType.Driver);
            Assert.AreEqual(2, guids.Count);
            Assert.AreEqual(entity3, guids[0]);
            Assert.AreEqual(entity2, guids[1]);
            try
            {
                repository.DeleteEntity(agent1, entity3, EntityType.Car);
                Assert.Fail();
            }
            catch (InvalidDataException)
            {

            }
            repository.DeleteEntity(agent1, entity3, EntityType.Driver);
            guids = repository.GetEntitys(agent1, EntityType.Driver);
            Assert.AreEqual(1, guids.Count);
            Assert.AreEqual(entity2, guids[0]);
            repository.DeleteEntity(agent1, entity2, EntityType.Driver);
            guids = repository.GetEntitys(agent1, EntityType.Driver);
            Assert.AreEqual(0, guids.Count);
        }

        [TestMethod]
        public void GetEntitys_Hierarchy()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var agent = Guid.NewGuid();
            var entity = Guid.NewGuid();
            var agent1 = Guid.NewGuid();
            var entity1 = Guid.NewGuid();
            var entity2 = Guid.NewGuid();
            var entity3 = Guid.NewGuid();
            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));

            repository.AddEntity(agent, entity, EntityType.Driver);
            repository.AddEntity(agent, agent1, EntityType.Agent);
            repository.AddEntity(agent1, entity1, EntityType.Driver);
            repository.AddEntity(agent1, entity2, EntityType.Car);
            repository.AddEntity(agent1, entity3, EntityType.Car);

            var agents=repository.GetEntitys(agent, EntityType.Agent);
            Assert.AreEqual(1, agents.Count);
            Assert.AreEqual(agent1, agents[0]);

            var drivers = repository.GetEntitys(agent, EntityType.Driver);
            Assert.AreEqual(2, drivers.Count);
            Assert.AreEqual(entity, drivers[1]);
            Assert.AreEqual(entity1, drivers[0]);

            drivers = repository.GetEntitys(agent1, EntityType.Driver);
            Assert.AreEqual(1, drivers.Count);
            Assert.AreEqual(entity1, drivers[0]);

            var cars = repository.GetEntitys(agent1, EntityType.Car);
            Assert.AreEqual(2, cars.Count);
            Assert.AreEqual(entity2, cars[0]);
            Assert.AreEqual(entity3, cars[1]);

            cars = repository.GetEntitys(agent, EntityType.Car);
            Assert.AreEqual(2, cars.Count);
            Assert.AreEqual(entity2, cars[0]);
            Assert.AreEqual(entity3, cars[1]);

            repository.DeleteEntity(agent, entity3, EntityType.Car);

            cars = repository.GetEntitys(agent1, EntityType.Car);
            Assert.AreEqual(1, cars.Count);
            Assert.AreEqual(entity2, cars[0]);

            cars = repository.GetEntitys(agent, EntityType.Car);
            Assert.AreEqual(1, cars.Count);
            Assert.AreEqual(entity2, cars[0]);

            repository.DeleteEntity(agent1, entity1, EntityType.Driver);

            drivers = repository.GetEntitys(agent, EntityType.Driver);
            Assert.AreEqual(1, drivers.Count);
            Assert.AreEqual(entity, drivers[0]);

            drivers = repository.GetEntitys(agent1, EntityType.Driver);
            Assert.AreEqual(0, drivers.Count);

            repository.DeleteEntity(agent1, entity2, EntityType.Car);

            cars = repository.GetEntitys(agent1, EntityType.Car);
            Assert.AreEqual(0, cars.Count);

            cars = repository.GetEntitys(agent, EntityType.Car);
            Assert.AreEqual(0, cars.Count);

            repository.DeleteEntity(agent, entity, EntityType.Driver);

            drivers = repository.GetEntitys(agent, EntityType.Driver);
            Assert.AreEqual(0, drivers.Count);

            drivers = repository.GetEntitys(agent1, EntityType.Driver);
            Assert.AreEqual(0, drivers.Count);

            repository.DeleteEntity(agent, agent1, EntityType.Agent);

             agents = repository.GetEntitys(agent, EntityType.Agent);
            Assert.AreEqual(0, agents.Count);
        }

        [TestMethod]
        public void Exist()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var agent = Guid.NewGuid();
            var entity = Guid.NewGuid();
            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));

            repository.AddEntity(agent, entity, EntityType.Driver);

            var result = repository.Exist(agent, entity, EntityType.Agent);
            Assert.AreEqual(false, result);

            result = repository.Exist(agent, entity, EntityType.Driver);
            Assert.AreEqual(true, result);

            repository.DeleteEntity(agent, entity, EntityType.Driver);

            result = repository.Exist(agent, entity, EntityType.Driver);
            Assert.AreEqual(false, result);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void Exist_AgentEmpty()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var agent = Guid.Empty;
            var entity = Guid.NewGuid();
            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));
            repository.Exist(agent, entity, EntityType.Car);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void Exist_EntityEmpty()
        {
            var mockSet = new Mock<DbSet<EntityGuids>>();
            var mockContext = new Mock<ApplicationContext>();
            mockContext.Setup(m => m.EntityGuids).Returns(mockSet.Object);
            IEntityRepository repository = new EntityFrameworkEntityRepository(new NLogLoggerPlugin(), mockContext.Object);
            var agent = Guid.NewGuid();
            var entity = Guid.Empty;
            var data = new List<EntityGuids>();
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<EntityGuids>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<EntityGuids>())).Callback<EntityGuids>(s => data.Add(s));
            repository.Exist(agent, entity, EntityType.Car);
        }
    }
}
