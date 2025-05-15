
namespace RuleArchitectPrototype.Models
{
    public class ParameterMapping : BaseModel
    {
        private int _parameterMappingId;
        public int ParameterMappingId
        {
            get => _parameterMappingId;
            set => SetField(ref _parameterMappingId, value);
        }

        // SoftwareOptionId is implicit

        private string _relatedSheetName;
        public string RelatedSheetName
        {
            get => _relatedSheetName;
            set => SetField(ref _relatedSheetName, value);
        }

        private string _conditionIdentifier;
        public string ConditionIdentifier
        {
            get => _conditionIdentifier;
            set => SetField(ref _conditionIdentifier, value);
        }

        private string _conditionName;
        public string ConditionName
        {
            get => _conditionName;
            set => SetField(ref _conditionName, value);
        }

        private string _settingContext;
        public string SettingContext
        {
            get => _settingContext;
            set => SetField(ref _settingContext, value);
        }

        private string _configurationDetailsJson; // In a real app, might parse this into objects
        public string ConfigurationDetailsJson
        {
            get => _configurationDetailsJson;
            set => SetField(ref _configurationDetailsJson, value);
        }
    }
}
