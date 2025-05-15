using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RuleArchitectPrototype.Models
{
    public class SoftwareOption : BaseModel // Changed from YourRulesheetModel
    {
        private int _softwareOptionId;
        public int SoftwareOptionId
        {
            get => _softwareOptionId;
            set => SetField(ref _softwareOptionId, value);
        }

        private string _primaryName;
        public string PrimaryName // Was 'Name' in YourRulesheetModel
        {
            get => _primaryName;
            set => SetField(ref _primaryName, value);
        }

        private string _alternativeNames;
        public string AlternativeNames
        {
            get => _alternativeNames;
            set => SetField(ref _alternativeNames, value);
        }

        private string _sourceFileName;
        public string SourceFileName
        {
            get => _sourceFileName;
            set => SetField(ref _sourceFileName, value);
        }

        private string _primaryOptionNumberDisplay;
        public string PrimaryOptionNumberDisplay
        {
            get => _primaryOptionNumberDisplay;
            set => SetField(ref _primaryOptionNumberDisplay, value);
        }

        private string _notes; // Was 'Description' in YourRulesheetModel, now more general
        public string Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        private string _checkedBy;
        public string CheckedBy
        {
            get => _checkedBy;
            set => SetField(ref _checkedBy, value);
        }

        private DateTime? _checkedDate;
        public DateTime? CheckedDate
        {
            get => _checkedDate;
            set => SetField(ref _checkedDate, value);
        }

        private int? _controlSystemId; // FK
        public int? ControlSystemId
        {
            get => _controlSystemId;
            set => SetField(ref _controlSystemId, value);
        }

        private ControlSystem _controlSystem;
        public ControlSystem ControlSystem
        {
            get => _controlSystem;
            set => SetField(ref _controlSystem, value);
        }
        
        // IsEnabled from YourRulesheetModel can be removed or repurposed if needed.
        // For now, I'll remove it as its direct mapping isn't in the DB schema.

        // Collections for related entities
        public ObservableCollection<OptionNumberRegistry> OptionNumbers { get; set; }
        public ObservableCollection<SoftwareOptionActivationRule> ActivationRules { get; set; }
        public ObservableCollection<SoftwareOptionSpecificationCode> SpecificationCodes { get; set; }
        public ObservableCollection<Requirement> Requirements { get; set; }
        public ObservableCollection<ParameterMapping> ParameterMappings { get; set; }

        public SoftwareOption()
        {
            OptionNumbers = new ObservableCollection<OptionNumberRegistry>();
            ActivationRules = new ObservableCollection<SoftwareOptionActivationRule>();
            SpecificationCodes = new ObservableCollection<SoftwareOptionSpecificationCode>();
            Requirements = new ObservableCollection<Requirement>();
            ParameterMappings = new ObservableCollection<ParameterMapping>();
        }

        // Clone method needs to be more robust for complex objects if true deep cloning is needed for editing.
        // This is a starting point for cloning.
        public SoftwareOption Clone()
        {
            var clone = (SoftwareOption)this.MemberwiseClone(); // Shallow copy of value types

            // For reference types and collections, create new instances and copy items
            clone.ControlSystem = this.ControlSystem; // Assuming ControlSystem itself is not deeply cloned here, just reference.
                                                      // Or: clone.ControlSystem = this.ControlSystem?.CloneShallow(); if ControlSystem had such a method.

            clone.OptionNumbers = new ObservableCollection<OptionNumberRegistry>(this.OptionNumbers.Select(on => new OptionNumberRegistry { OptionNumberRegistryId = on.OptionNumberRegistryId, OptionNumber = on.OptionNumber }));
            
            clone.ActivationRules = new ObservableCollection<SoftwareOptionActivationRule>(
                this.ActivationRules.Select(ar => new SoftwareOptionActivationRule { 
                    SoftwareOptionActivationRuleId = ar.SoftwareOptionActivationRuleId, 
                    RuleName = ar.RuleName, 
                    ActivationSetting = ar.ActivationSetting, 
                    Notes = ar.Notes 
                }));

            clone.SpecificationCodes = new ObservableCollection<SoftwareOptionSpecificationCode>(
                this.SpecificationCodes.Select(sc => new SoftwareOptionSpecificationCode {
                    SoftwareOptionSpecificationCodeId = sc.SoftwareOptionSpecificationCodeId,
                    SpecCodeDefinitionId = sc.SpecCodeDefinitionId,
                    SpecCodeDefinition = sc.SpecCodeDefinition, // Reference, or clone if SpecCodeDefinition is complex/editable
                    SoftwareOptionActivationRuleId = sc.SoftwareOptionActivationRuleId,
                    ActivationRule = sc.ActivationRule, // Reference, or clone
                    SpecificInterpretation = sc.SpecificInterpretation
                }));

            clone.Requirements = new ObservableCollection<Requirement>(
                this.Requirements.Select(r => new Requirement { /* copy all relevant properties */
                    RequirementId = r.RequirementId,
                    RequirementType = r.RequirementType,
                    Condition = r.Condition,
                    GeneralRequiredValue = r.GeneralRequiredValue,
                    RequiredSoftwareOptionId = r.RequiredSoftwareOptionId,
                    RequiredSoftwareOptionDisplay = r.RequiredSoftwareOptionDisplay,
                    RequiredSpecCodeDefinitionId = r.RequiredSpecCodeDefinitionId,
                    RequiredSpecCodeDefinitionDisplay = r.RequiredSpecCodeDefinitionDisplay,
                    OspFileName = r.OspFileName,
                    OspFileVersion = r.OspFileVersion,
                    Notes = r.Notes
                }));

            clone.ParameterMappings = new ObservableCollection<ParameterMapping>(
                this.ParameterMappings.Select(pm => new ParameterMapping { /* copy all relevant properties */ 
                    ParameterMappingId = pm.ParameterMappingId,
                    RelatedSheetName = pm.RelatedSheetName,
                    ConditionIdentifier = pm.ConditionIdentifier,
                    ConditionName = pm.ConditionName,
                    SettingContext = pm.SettingContext,
                    ConfigurationDetailsJson = pm.ConfigurationDetailsJson
                }));
            
            return clone;
        }
    }
}
