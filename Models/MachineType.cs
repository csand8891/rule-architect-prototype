
namespace RuleArchitectPrototype.Models
{
    public class MachineType : BaseModel
    {
        private int _machineTypeId;
        public int MachineTypeId
        {
            get => _machineTypeId;
            set => SetField(ref _machineTypeId, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }
        
        public override string ToString() => Name;
    }
}
