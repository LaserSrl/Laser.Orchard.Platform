namespace Laser.Orchard.StartupConfig.Models {
    public class JsonDataTablePartSettings {
        private string _columnsDefinition = "[{field:'name',title:'Name'},{field:'description',title:'Description'}]"; // default value
        public string ColumnsDefinition {
            get {
                return _columnsDefinition;
            }
            set {
                if(string.IsNullOrWhiteSpace(value) == false) {
                    _columnsDefinition = value;
                }
            }
        }
        public int MaxRows { get; set; }
    }
}