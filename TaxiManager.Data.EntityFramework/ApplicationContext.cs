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

        public ApplicationContext()
            : base("TaxiManager")
        {
        }
    }
}
