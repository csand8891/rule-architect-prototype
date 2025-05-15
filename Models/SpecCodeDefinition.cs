
namespace RuleArchitectPrototype.Models
{
    public class SpecCodeDefinition : BaseModel
    {
        private int _specCodeDefinitionId;
        public int SpecCodeDefinitionId
        {
            get => _specCodeDefinitionId;
            set => SetField(ref _specCodeDefinitionId, value);
        }

        private string _specCodeNo;
        public string SpecCodeNo
        {
            get => _specCodeNo;
            set => SetField(ref _specCodeNo, value);
        }

        private string _specCodeBit;
        public string SpecCodeBit
        {
            get => _specCodeBit;
            set => SetField(ref _specCodeBit, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        private string _category;
        public string Category
        {
            get => _category;
            set => SetField(ref _category, value);
        }

        private int _machineTypeId; // FK
        public int MachineTypeId 
        {
            get => _machineTypeId;
            set => SetField(ref _machineTypeId, value);
        }


        private MachineType _machineType;
        public MachineType MachineType
        {
            get => _machineType;
            set => SetField(ref _machineType, value);
        }
        
        public string DisplayText => $"No: {SpecCodeNo}, Bit: {SpecCodeBit} ({Description}) - {MachineType?.Name}";
        public override string ToString() => DisplayText;

    }
}
