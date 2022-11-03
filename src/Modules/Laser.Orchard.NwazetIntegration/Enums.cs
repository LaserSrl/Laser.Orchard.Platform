using Orchard.Localization;
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

    public enum CustomerTypeOptions {
        /*The maximum length of next options have to be 20 chars 
         * because the value of this enum is stored in a database 
         * column with length 20*/
        Individual, /*Consumatore finale, Consumer*/
        LegalEntity,/*Società o ditta individuale, Company or sole proprietorship*/
        Undefined
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