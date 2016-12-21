using System.Data.Entity;

using TaxiManager.Data.Model;

namespace TaxiManager.Data.EntityFramework
{
    /// <summary>
    /// контекс данных 
    /// </summary>
    public class ApplicationContext : DbContext
    {
        public virtual DbSet<EntityGuids> EntityGuids { get; set; }

        public virtual DbSet<Right> Rights { get; set; }

        public virtual DbSet<Agent> Agents { get; set; }

        public virtual DbSet<Car> Cars { get; set; }

        public virtual DbSet<Driver> Drivers { get; set; }
        public ApplicationContext()
            : base("TaxiManager")
        {
        }
    }
}
