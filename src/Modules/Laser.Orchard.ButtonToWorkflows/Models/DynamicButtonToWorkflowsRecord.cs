namespace Laser.Orchard.ButtonToWorkflows.Models {
    public class DynamicButtonToWorkflowsRecord {
        public virtual int Id { get; set; }
        public virtual string ButtonName { get; set; }
        public virtual string ButtonText { get; set; }
        public virtual string ButtonDescription { get; set; }
        public virtual string ButtonMessage { get; set; }
        public virtual bool ButtonAsync { get; set; }
        public virtual string GlobalIdentifier { get; set; }
    }
}