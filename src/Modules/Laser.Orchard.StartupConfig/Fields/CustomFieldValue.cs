using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Fields {
    public class CustomFieldValue {
        /// <summary>
        /// Field value name as specified in parameter "valueName" of IFieldIndexService.Set(). 
        /// Usually "" applies to the main property of the field.
        /// </summary>
        public string ValueName { get; set; }
        /// <summary>
        /// Field value.
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// Field value type. Base type as specified in parameter "valueType" of IFieldIndexService.Set().
        /// </summary>
        public Type ValueType { get; set; }
    }
}