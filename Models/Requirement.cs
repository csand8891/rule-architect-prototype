
namespace RuleArchitectPrototype.Models
{
    public class Requirement : BaseModel
    {
        private int _requirementId;
        public int RequirementId
        {
            get => _requirementId;
            set => SetField(ref _requirementId, value);
        }

        // SoftwareOptionId is implicit

        private string _requirementType; // e.g., "SOFTWARE_OPTION", "SPEC_CODE_DEFINITION", "OSP_FILE_VERSION", "GENERAL_TEXT"
        public string RequirementType
        {
            get => _requirementType;
            set => SetField(ref _requirementType, value);
        }

        private string _condition; // e.g., "Requires", "Excludes", "HigherThanVersion"
        public string Condition
        {
            get => _condition;
            set => SetField(ref _condition, value);
        }

        private string _generalRequiredValue;
        public string GeneralRequiredValue // For GENERAL_TEXT or simple values
        {
            get => _generalRequiredValue;
            set => SetField(ref _generalRequiredValue, value);
        }

        private int? _requiredSoftwareOptionId; // FK
        public int? RequiredSoftwareOptionId
        {
            get => _requiredSoftwareOptionId;
            set => SetField(ref _requiredSoftwareOptionId, value);
        }
        // In a full app, you might have:
        // private SoftwareOption _requiredSoftwareOption;
        // public SoftwareOption RequiredSoftwareOption { get => _requiredSoftwareOption; set => SetField(ref _requiredSoftwareOption, value); }
        public string RequiredSoftwareOptionDisplay { get; set; } // For display purposes


        private int? _requiredSpecCodeDefinitionId; // FK
        public int? RequiredSpecCodeDefinitionId
        {
            get => _requiredSpecCodeDefinitionId;
            set => SetField(ref _requiredSpecCodeDefinitionId, value);
        }
        // In a full app, you might have:
        // private SpecCodeDefinition _requiredSpecCodeDefinition;
        // public SpecCodeDefinition RequiredSpecCodeDefinition { get => _requiredSpecCodeDefinition; set => SetField(ref _requiredSpecCodeDefinition, value); }
        public string RequiredSpecCodeDefinitionDisplay { get; set; } // For display

        private string _ospFileName;
        public string OspFileName
        {
            get => _ospFileName;
            set => SetField(ref _ospFileName, value);
        }

        private string _ospFileVersion;
        public string OspFileVersion
        {
            get => _ospFileVersion;
            set => SetField(ref _ospFileVersion, value);
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }
    }
}
