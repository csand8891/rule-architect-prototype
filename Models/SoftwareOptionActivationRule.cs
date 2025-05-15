
namespace RuleArchitectPrototype.Models
{
    public class SoftwareOptionActivationRule : BaseModel
    {
        private int _softwareOptionActivationRuleId;
        public int SoftwareOptionActivationRuleId
        {
            get => _softwareOptionActivationRuleId;
            set => SetField(ref _softwareOptionActivationRuleId, value);
        }

        // SoftwareOptionId is implicit

        private string _ruleName;
        public string RuleName // e.g., "Tool Break Main Enable"
        {
            get => _ruleName;
            set => SetField(ref _ruleName, value);
        }

        private string _activationSetting; // e.g., "1 = ON"
        public string ActivationSetting
        {
            get => _activationSetting;
            set => SetField(ref _activationSetting, value);
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }
    }
}
