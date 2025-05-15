
namespace RuleArchitectPrototype.Models
{
    public class SoftwareOptionSpecificationCode : BaseModel
    {
        private int _softwareOptionSpecificationCodeId;
        public int SoftwareOptionSpecificationCodeId
        {
            get => _softwareOptionSpecificationCodeId;
            set => SetField(ref _softwareOptionSpecificationCodeId, value);
        }

        // SoftwareOptionId is implicit

        private int _specCodeDefinitionId; // FK
        public int SpecCodeDefinitionId
        {
            get => _specCodeDefinitionId;
            set => SetField(ref _specCodeDefinitionId, value);
        }

        private SpecCodeDefinition _specCodeDefinition;
        public SpecCodeDefinition SpecCodeDefinition
        {
            get => _specCodeDefinition;
            set => SetField(ref _specCodeDefinition, value);
        }

        private int? _softwareOptionActivationRuleId; // FK, Nullable
        public int? SoftwareOptionActivationRuleId
        {
            get => _softwareOptionActivationRuleId;
            set => SetField(ref _softwareOptionActivationRuleId, value);
        }
        
        private SoftwareOptionActivationRule _activationRule; // The actual linked rule object
        public SoftwareOptionActivationRule ActivationRule 
        {
            get => _activationRule;
            set => SetField(ref _activationRule, value);
        }


        private string _specificInterpretation;
        public string SpecificInterpretation
        {
            get => _specificInterpretation;
            set => SetField(ref _specificInterpretation, value);
        }
    }
}
