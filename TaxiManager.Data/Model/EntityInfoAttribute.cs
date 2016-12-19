using System;
using System.Linq;

namespace TaxiManager.Data.Model
{
    public sealed class EntityInfoAttribute : Attribute
    {
        public EntityType EntityType { get; private set; }


        public EntityInfoAttribute(EntityType entityType)
        {
            EntityType = entityType;
        }

        public static EntityInfoAttribute GetAttribute(Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(EntityInfoAttribute), true)
                .Cast<EntityInfoAttribute>()
                .ToArray();

            if (attributes.Length == 0)
                return null;

            if (attributes.Length > 1)
                throw new InvalidOperationException("Entity should have only one EntityInfoAttribute.");

            return attributes[0];
        }
    }
}
