
namespace RuleArchitectPrototype.Models
{
    public class ControlSystem : BaseModel
    {
        private int _controlSystemId;
        public int ControlSystemId
        {
            get => _controlSystemId;
            set => SetField(ref _controlSystemId, value);
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
