using RuleArchitectPrototype.Models;
using RuleArchitectPrototype.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows;

namespace RuleArchitectPrototype.ViewModels
{
    public class AddEditSpecCodeDialogViewModel : BaseModel
    {
        private readonly TabbedRulesheetViewModel _parentTabbedViewModel;
        private readonly ObservableCollection<SpecCodeDefinition> _allGlobalSpecCodeDefinitions;


        // --- Properties for UI Binding & Logic ---

        private MachineType _currentSoftwareOptionMachineType;
        public MachineType CurrentSoftwareOptionMachineType
        {
            get => _currentSoftwareOptionMachineType;
            private set => SetField(ref _currentSoftwareOptionMachineType, value);
        }

        private string _specCodeNo;
        public string SpecCodeNo
        {
            get => _specCodeNo;
            set
            {
                if (SetField(ref _specCodeNo, value))
                {
                    LoadExistingSpecCodeDefinition();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _specCodeBit;
        public string SpecCodeBit
        {
            get => _specCodeBit;
            set
            {
                if (SetField(ref _specCodeBit, value))
                {
                    LoadExistingSpecCodeDefinition();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private SpecCodeDefinition _selectedExistingSpecCodeDefinition;
        public SpecCodeDefinition SelectedExistingSpecCodeDefinition
        {
            get => _selectedExistingSpecCodeDefinition;
            private set // Keep this private as it's driven by LoadExistingSpecCodeDefinition
            {
                if (SetField(ref _selectedExistingSpecCodeDefinition, value))
                {
                    // This line will now use the public setter of IsCreatingNewSpecCodeDefinition
                    IsCreatingNewSpecCodeDefinition = (value == null && !string.IsNullOrWhiteSpace(SpecCodeNo) && !string.IsNullOrWhiteSpace(SpecCodeBit));
                    if (value != null)
                    {
                        NewSpecCodeDescription = value.Description;
                        SelectedNewSpecCodeCategory = AvailableCategories.FirstOrDefault(c => c == value.Category) ?? AvailableCategories.First();
                    }
                    else
                    {
                        if (IsCreatingNewSpecCodeDefinition) // Check the updated value
                        {
                            NewSpecCodeDescription = string.Empty;
                            SelectedNewSpecCodeCategory = AvailableCategories.First();
                        }
                    }
                }
            }
        }

        private bool _isCreatingNewSpecCodeDefinition;
        public bool IsCreatingNewSpecCodeDefinition
        {
            get => _isCreatingNewSpecCodeDefinition;
            set => SetField(ref _isCreatingNewSpecCodeDefinition, value); // Changed to public setter (or just 'set')
        }

        private string _newSpecCodeDescription;
        public string NewSpecCodeDescription
        {
            get => _newSpecCodeDescription;
            set => SetField(ref _newSpecCodeDescription, value);
        }

        public ObservableCollection<string> AvailableCategories { get; }
        private string _selectedNewSpecCodeCategory;
        public string SelectedNewSpecCodeCategory
        {
            get => _selectedNewSpecCodeCategory;
            set => SetField(ref _selectedNewSpecCodeCategory, value);
        }

        public ObservableCollection<SoftwareOptionActivationRule> AvailableActivationRules { get; }
        private SoftwareOptionActivationRule _selectedActivationRule;
        public SoftwareOptionActivationRule SelectedActivationRule
        {
            get => _selectedActivationRule;
            set => SetField(ref _selectedActivationRule, value);
        }

        private string _specificInterpretation;
        public string SpecificInterpretation
        {
            get => _specificInterpretation;
            set => SetField(ref _specificInterpretation, value);
        }

        public SpecCodeDefinition ResultSpecCodeDefinition { get; private set; }
        public int? ResultSoftwareOptionActivationRuleId { get; private set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddEditSpecCodeDialogViewModel(
            MachineType currentMachineType,
            ObservableCollection<SpecCodeDefinition> allGlobalSpecCodeDefinitions,
            ObservableCollection<SoftwareOptionActivationRule> availableParentActivationRules,
            TabbedRulesheetViewModel parentTabbedViewModel,
            SoftwareOptionSpecificationCode existingSosCodeToEdit = null)
        {
            _parentTabbedViewModel = parentTabbedViewModel;
            _allGlobalSpecCodeDefinitions = allGlobalSpecCodeDefinitions;

            CurrentSoftwareOptionMachineType = currentMachineType;
            AvailableActivationRules = new ObservableCollection<SoftwareOptionActivationRule>(availableParentActivationRules ?? new ObservableCollection<SoftwareOptionActivationRule>());
            AvailableActivationRules.Insert(0, new SoftwareOptionActivationRule { SoftwareOptionActivationRuleId = 0, RuleName = "(None)" });


            AvailableCategories = new ObservableCollection<string> { "NC1", "NC2", "PLC1", "PLC2" };
            SelectedNewSpecCodeCategory = AvailableCategories.FirstOrDefault();


            SaveCommand = new RelayCommand(PerformSave, CanPerformSave);
            CancelCommand = new RelayCommand(PerformCancel);

            if (existingSosCodeToEdit != null)
            {
                SpecCodeNo = existingSosCodeToEdit.SpecCodeDefinition?.SpecCodeNo;
                SpecCodeBit = existingSosCodeToEdit.SpecCodeDefinition?.SpecCodeBit;
                // LoadExistingSpecCodeDefinition will be called by setters of No/Bit.
                // If it finds the existing one, SelectedExistingSpecCodeDefinition will be set,
                // which in turn sets IsCreatingNewSpecCodeDefinition to false and populates description/category.
                if (existingSosCodeToEdit.SpecCodeDefinition != null && SelectedExistingSpecCodeDefinition == null)
                {
                    // If LoadExistingSpecCodeDefinition didn't immediately find it (e.g. due to timing or slight mismatch not caught by Find)
                    // ensure fields are populated from the item being edited.
                    NewSpecCodeDescription = existingSosCodeToEdit.SpecCodeDefinition.Description;
                    SelectedNewSpecCodeCategory = existingSosCodeToEdit.SpecCodeDefinition.Category;
                    IsCreatingNewSpecCodeDefinition = false; // Explicitly set if we are editing an existing one
                }


                SelectedActivationRule = existingSosCodeToEdit.SoftwareOptionActivationRuleId.HasValue
                    ? AvailableActivationRules.FirstOrDefault(ar => ar.SoftwareOptionActivationRuleId == existingSosCodeToEdit.SoftwareOptionActivationRuleId.Value)
                    : AvailableActivationRules.FirstOrDefault(ar => ar.SoftwareOptionActivationRuleId == 0);
                SpecificInterpretation = existingSosCodeToEdit.SpecificInterpretation;
            }
            else
            {
                SelectedActivationRule = AvailableActivationRules.FirstOrDefault(ar => ar.SoftwareOptionActivationRuleId == 0);
                LoadExistingSpecCodeDefinition(); // Initial check in case No/Bit are pre-filled for a new item somehow
            }
        }

        private void LoadExistingSpecCodeDefinition()
        {
            if (string.IsNullOrWhiteSpace(SpecCodeNo) || string.IsNullOrWhiteSpace(SpecCodeBit) || CurrentSoftwareOptionMachineType == null)
            {
                SelectedExistingSpecCodeDefinition = null; // This will trigger its setter logic
                return;
            }

            if (!int.TryParse(SpecCodeNo, out int noVal) || noVal < 1 || noVal > 32)
            {
                SelectedExistingSpecCodeDefinition = null; // This will trigger its setter logic
                return;
            }
            if (!int.TryParse(SpecCodeBit, out int bitVal) || bitVal < 0 || bitVal > 7)
            {
                SelectedExistingSpecCodeDefinition = null; // This will trigger its setter logic
                return;
            }

            SelectedExistingSpecCodeDefinition = _allGlobalSpecCodeDefinitions.FirstOrDefault(sd =>
                sd.SpecCodeNo == SpecCodeNo &&
                sd.SpecCodeBit == SpecCodeBit &&
                sd.MachineTypeId == CurrentSoftwareOptionMachineType.MachineTypeId);
        }

        private bool CanPerformSave()
        {
            if (string.IsNullOrWhiteSpace(SpecCodeNo) || string.IsNullOrWhiteSpace(SpecCodeBit)) return false;
            if (!int.TryParse(SpecCodeNo, out int noVal) || noVal < 1 || noVal > 32) return false;
            if (!int.TryParse(SpecCodeBit, out int bitVal) || bitVal < 0 || bitVal > 7) return false;

            if (IsCreatingNewSpecCodeDefinition)
            {
                if (string.IsNullOrWhiteSpace(NewSpecCodeDescription) || string.IsNullOrWhiteSpace(SelectedNewSpecCodeCategory))
                {
                    return false;
                }
            }
            return true;
        }

        private void PerformSave()
        {
            if (!CanPerformSave()) return;

            if (IsCreatingNewSpecCodeDefinition)
            {
                ResultSpecCodeDefinition = new SpecCodeDefinition
                {
                    SpecCodeDefinitionId = 0,
                    SpecCodeNo = SpecCodeNo,
                    SpecCodeBit = SpecCodeBit,
                    Description = NewSpecCodeDescription,
                    Category = SelectedNewSpecCodeCategory,
                    MachineTypeId = CurrentSoftwareOptionMachineType.MachineTypeId,
                    MachineType = CurrentSoftwareOptionMachineType
                };
            }
            else
            {
                ResultSpecCodeDefinition = SelectedExistingSpecCodeDefinition;
            }

            ResultSoftwareOptionActivationRuleId = (SelectedActivationRule != null && SelectedActivationRule.SoftwareOptionActivationRuleId != 0)
                ? SelectedActivationRule.SoftwareOptionActivationRuleId
                : (int?)null;

            OnRequestCloseDialog(true);
        }

        private void PerformCancel()
        {
            OnRequestCloseDialog(false);
        }

        public event Action<bool?> RequestCloseDialog;
        private void OnRequestCloseDialog(bool? dialogResult)
        {
            RequestCloseDialog?.Invoke(dialogResult);
        }
    }
}
