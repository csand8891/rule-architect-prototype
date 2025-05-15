
namespace RuleArchitectPrototype.Models
{
    public class OptionNumberRegistry : BaseModel
    {
        private int _optionNumberRegistryId;
        public int OptionNumberRegistryId
        {
            get => _optionNumberRegistryId;
            set => SetField(ref _optionNumberRegistryId, value);
        }

        private string _optionNumber;
        public string OptionNumber
        {
            get => _optionNumber;
            set => SetField(ref _optionNumber, value);
        }

        // SoftwareOptionId is implicitly handled by being in SoftwareOption's collection
    }
}
