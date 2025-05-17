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
                    OnPropertyChanged(nameof(IsCreatingNewSpecCodeDefinition));
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
                    OnPropertyChanged(nameof(IsCreatingNewSpecCodeDefinition));
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private SpecCodeDefinition _selectedExistingSpecCodeDefinition;
        public SpecCodeDefinition SelectedExistingSpecCodeDefinition
        {
            get => _selectedExistingSpecCodeDefinition;
            private set
            {
                if (SetField(ref _selectedExistingSpecCodeDefinition, value))
                {
                    OnPropertyChanged(nameof(IsCreatingNewSpecCodeDefinition));
                    if (value != null)
                    {
                        NewSpecCodeDescription = value.Description;
                        SelectedNewSpecCodeCategory = AvailableCategories.FirstOrDefault(c => c == value.Category) ?? AvailableCategories.First();
                    }
                    else
                    {
                        if (this.IsCreatingNewSpecCodeDefinition)
                        {
                            NewSpecCodeDescription = string.Empty;
                            SelectedNewSpecCodeCategory = AvailableCategories.First();
                        }
                    }
                    // When SelectedExistingSpecCodeDefinition changes, it affects IsCreatingNewSpecCodeDefinition,
                    // which in turn affects CanPerformSave.
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCreatingNewSpecCodeDefinition
        {
            get
            {
                bool canBeNew = !string.IsNullOrWhiteSpace(SpecCodeNo) &&
                                !string.IsNullOrWhiteSpace(SpecCodeBit) &&
                                (int.TryParse(SpecCodeNo, out int noVal) && noVal >= 1 && noVal <= 32) &&
                                (int.TryParse(SpecCodeBit, out int bitVal) && bitVal >= 0 && bitVal <= 7);
                return canBeNew && SelectedExistingSpecCodeDefinition == null;
            }
        }

        private string _newSpecCodeDescription;
        public string NewSpecCodeDescription
        {
            get => _newSpecCodeDescription;
            set
            {
                if (SetField(ref _newSpecCodeDescription, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged(); // Description affects CanSave
                }
            }
        }

        public ObservableCollection<string> AvailableCategories { get; }
        private string _selectedNewSpecCodeCategory;
        public string SelectedNewSpecCodeCategory
        {
            get => _selectedNewSpecCodeCategory;
            set
            {
                if (SetField(ref _selectedNewSpecCodeCategory, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged(); // Category affects CanSave
                }
            }
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
            _allGlobalSpecCodeDefinitions = allGlobalSpecCodeDefinitions ?? new ObservableCollection<SpecCodeDefinition>();

            CurrentSoftwareOptionMachineType = currentMachineType;
            AvailableActivationRules = new ObservableCollection<SoftwareOptionActivationRule>(availableParentActivationRules ?? new ObservableCollection<SoftwareOptionActivationRule>());
            AvailableActivationRules.Insert(0, new SoftwareOptionActivationRule { SoftwareOptionActivationRuleId = 0, RuleName = "(None)" });


            AvailableCategories = new ObservableCollection<string> { "NC1", "NC2", "PLC1", "PLC2" };
            SelectedNewSpecCodeCategory = AvailableCategories.FirstOrDefault();


            SaveCommand = new RelayCommand(PerformSave, CanPerformSave);
            CancelCommand = new RelayCommand(PerformCancel); // No CanExecute delegate, so should always be enabled

            if (existingSosCodeToEdit != null)
            {
                // Set backing fields directly first to avoid premature LoadExistingSpecCodeDefinition calls
                _specCodeNo = existingSosCodeToEdit.SpecCodeDefinition?.SpecCodeNo;
                _specCodeBit = existingSosCodeToEdit.SpecCodeDefinition?.SpecCodeBit;
                OnPropertyChanged(nameof(SpecCodeNo));
                OnPropertyChanged(nameof(SpecCodeBit));

                LoadExistingSpecCodeDefinition(); // Now call it

                if (SelectedExistingSpecCodeDefinition == null && existingSosCodeToEdit.SpecCodeDefinition != null)
                {
                    NewSpecCodeDescription = existingSosCodeToEdit.SpecCodeDefinition.Description;
                    SelectedNewSpecCodeCategory = existingSosCodeToEdit.SpecCodeDefinition.Category;
                }

                SelectedActivationRule = existingSosCodeToEdit.SoftwareOptionActivationRuleId.HasValue
                    ? AvailableActivationRules.FirstOrDefault(ar => ar.SoftwareOptionActivationRuleId == existingSosCodeToEdit.SoftwareOptionActivationRuleId.Value)
                    : AvailableActivationRules.FirstOrDefault(ar => ar.SoftwareOptionActivationRuleId == 0);
                SpecificInterpretation = existingSosCodeToEdit.SpecificInterpretation;
            }
            else
            {
                SelectedActivationRule = AvailableActivationRules.FirstOrDefault(ar => ar.SoftwareOptionActivationRuleId == 0);
                LoadExistingSpecCodeDefinition();
            }
            OnPropertyChanged(nameof(IsCreatingNewSpecCodeDefinition));
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged(); // Initial check for SaveCommand
        }

        private void LoadExistingSpecCodeDefinition()
        {
            if (string.IsNullOrWhiteSpace(_specCodeNo) || string.IsNullOrWhiteSpace(_specCodeBit) || CurrentSoftwareOptionMachineType == null)
            {
                SelectedExistingSpecCodeDefinition = null;
                return;
            }

            if (!int.TryParse(_specCodeNo, out int noVal) || noVal < 1 || noVal > 32)
            {
                SelectedExistingSpecCodeDefinition = null;
                return;
            }
            if (!int.TryParse(_specCodeBit, out int bitVal) || bitVal < 0 || bitVal > 7)
            {
                SelectedExistingSpecCodeDefinition = null;
                return;
            }

            SelectedExistingSpecCodeDefinition = _allGlobalSpecCodeDefinitions.FirstOrDefault(sd =>
                sd.SpecCodeNo == _specCodeNo &&
                sd.SpecCodeBit == _specCodeBit &&
                sd.MachineTypeId == CurrentSoftwareOptionMachineType.MachineTypeId);
        }

        private bool CanPerformSave()
        {
            // No and Bit must be present and within range
            if (string.IsNullOrWhiteSpace(SpecCodeNo) || string.IsNullOrWhiteSpace(SpecCodeBit)) return false;
            if (!int.TryParse(SpecCodeNo, out int noVal) || noVal < 1 || noVal > 32) return false;
            if (!int.TryParse(SpecCodeBit, out int bitVal) || bitVal < 0 || bitVal > 7) return false;

            // If creating new, Description and Category must be selected
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
            if (!CanPerformSave())
            {
                MessageBox.Show("Cannot save. Please ensure all required fields are filled correctly.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
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
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during save: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Optionally, re-throw or log more details
            }
        }

        private void PerformCancel()
        {
            try
            {
                OnRequestCloseDialog(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during cancel: {ex.Message}", "Cancel Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event Action<bool?> RequestCloseDialog;
        private void OnRequestCloseDialog(bool? dialogResult)
        {
            RequestCloseDialog?.Invoke(dialogResult);
        }
    }
}
