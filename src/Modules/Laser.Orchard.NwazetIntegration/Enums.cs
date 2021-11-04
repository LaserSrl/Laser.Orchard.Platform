using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration {
    public enum TerritoryAdministrativeType {
        None = 0,
        Country = 1,
        Province = 2,
        City = 3
    }

	public enum TypeId {
    	Id,
    	Sku
    }

    static class EnumExtension<T> {
        public static T ParseEnum(string value) {
            if (string.IsNullOrWhiteSpace(value))
                return default(T);
            try {
                return (T)Enum.Parse(typeof(T), value);
            } catch (Exception) {
                return default(T);
            }
        }
    }
}