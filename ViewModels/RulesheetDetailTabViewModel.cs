using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using RuleArchitectPrototype.Models;
using RuleArchitectPrototype.Commands;
using RuleArchitectPrototype.Views; // Ensure this using directive is present
using System.Linq;
using System.Windows;
using materialDesign = MaterialDesignThemes.Wpf; // Alias for MaterialDesign PackIconKind
using System.Collections.Specialized; // For INotifyCollectionChanged

namespace RuleArchitectPrototype.ViewModels
{
    public class RulesheetDetailTabViewModel : BaseModel
    {
        private SoftwareOption _originalSoftwareOption;
        private SoftwareOption _editableSoftwareOption;
        public SoftwareOption EditableSoftwareOption
        {
            get => _editableSoftwareOption;
            set
            {
                // Detach old event handlers
                if (_editableSoftwareOption != null)
                {
                    _editableSoftwareOption.PropertyChanged -= EditableSoftwareOption_PropertyChanged;
                    if (_editableSoftwareOption.SpecificationCodes != null)
                    {
                        _editableSoftwareOption.SpecificationCodes.CollectionChanged -= DetailCollectionChanged;
                        // Item PropertyChanged for SpecificationCodes might be needed if inline editing for them is added later
                    }
                    if (_editableSoftwareOption.ActivationRules != null)
                    {
                        _editableSoftwareOption.ActivationRules.CollectionChanged -= DetailCollectionChanged;
                        foreach (var item in _editableSoftwareOption.ActivationRules) item.PropertyChanged -= DetailItemPropertyChanged;
                    }
                    // Detach for Requirements if they become inline editable
                    if (_editableSoftwareOption.Requirements != null)
                    {
                        _editableSoftwareOption.Requirements.CollectionChanged -= DetailCollectionChanged;
                        // foreach (var item in _editableSoftwareOption.Requirements) item.PropertyChanged -= DetailItemPropertyChanged; // If Requirement items become editable
                    }
                }

                if (SetField(ref _editableSoftwareOption, value))
                {
                    IsDirty = false;
                    if (_editableSoftwareOption != null)
                    {
                        _editableSoftwareOption.PropertyChanged += EditableSoftwareOption_PropertyChanged;
                        // Attach new event handlers
                        if (_editableSoftwareOption.SpecificationCodes != null)
                        {
                            _editableSoftwareOption.SpecificationCodes.CollectionChanged += DetailCollectionChanged;
                        }
                        if (_editableSoftwareOption.ActivationRules != null)
                        {
                            _editableSoftwareOption.ActivationRules.CollectionChanged += DetailCollectionChanged;
                            foreach (var item in _editableSoftwareOption.ActivationRules) item.PropertyChanged += DetailItemPropertyChanged;
                        }
                        if (_editableSoftwareOption.Requirements != null)
                        {
                            _editableSoftwareOption.Requirements.CollectionChanged += DetailCollectionChanged;
                        }
                    }
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(CanAccessDetailedTabs));
                    (AddSpecCodeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddActivationRuleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddRequirementCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Handler for property changes on items within detail collections (e.g., an ActivationRule's Name)
        private void DetailItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsEditMode) IsDirty = true;
        }

        // Handler for changes to the detail collections themselves (add/remove)
        private void DetailCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsEditMode) IsDirty = true;

            // If new items were added to ActivationRules, attach property changed handlers for inline editing
            if (sender == EditableSoftwareOption?.ActivationRules && e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<SoftwareOptionActivationRule>())
                {
                    item.PropertyChanged += DetailItemPropertyChanged;
                }
            }
            // If items were removed from ActivationRules, detach property changed handlers
            if (sender == EditableSoftwareOption?.ActivationRules && e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<SoftwareOptionActivationRule>())
                {
                    item.PropertyChanged -= DetailItemPropertyChanged;
                }
            }
            // Add similar logic for Requirements if their items become inline editable
        }


        private void EditableSoftwareOption_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsEditMode)
            {
                // Avoid marking dirty for changes to collections themselves if handled by DetailCollectionChanged
                // or for properties that don't signify a user edit that needs saving for the main object.
                if (e.PropertyName != nameof(SoftwareOption.SpecificationCodes) &&
                    e.PropertyName != nameof(SoftwareOption.ActivationRules) &&
                    e.PropertyName != nameof(SoftwareOption.Requirements) &&
                    e.PropertyName != nameof(SoftwareOption.OptionNumbers))
                {
                    IsDirty = true;
                }
            }
        }

        private bool _isDirty;
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (SetField(ref _isDirty, value))
                {
                    UpdateIconAndHeader();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }


        private string _tabHeader;
        public string TabHeader
        {
            get => _tabHeader;
            set => SetField(ref _tabHeader, value);
        }

        private materialDesign.PackIconKind _iconKind;
        public materialDesign.PackIconKind IconKind
        {
            get => _iconKind;
            set => SetField(ref _iconKind, value);
        }

        private bool _isPinned;
        public bool IsPinned
        {
            get => _isPinned;
            set
            {
                if (SetField(ref _isPinned, value))
                {
                    UpdateIconAndHeader();
                    _parentViewModel.OnTabPinnedStateChanged(this);
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetField(ref _isEditMode, value))
                {
                    UpdateIconAndHeader();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddSpecCodeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddActivationRuleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddRequirementCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _wasOriginallyNew;
        public bool WasOriginallyNew
        {
            get => _wasOriginallyNew;
            private set
            {
                if (SetField(ref _wasOriginallyNew, value))
                {
                    OnPropertyChanged(nameof(CanAccessDetailedTabs));
                    (AddSpecCodeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddActivationRuleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddRequirementCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool CanAccessDetailedTabs => EditableSoftwareOption != null && !WasOriginallyNew;


        public string TabDisplayName => EditableSoftwareOption?.PrimaryName ?? "New Rulesheet";
        public int SoftwareOptionId => EditableSoftwareOption?.SoftwareOptionId ?? 0;

        private readonly TabbedRulesheetViewModel _parentViewModel;

        public ObservableCollection<ControlSystem> AvailableControlSystems { get; }
        public ObservableCollection<MachineType> AvailableMachineTypes { get; }


        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CloseTabCommand { get; }
        public ICommand PinTabCommand { get; }
        public ICommand DeleteCommand { get; }

        public ICommand AddSpecCodeCommand { get; }
        public ICommand AddActivationRuleCommand { get; }
        public ICommand AddRequirementCommand { get; }


        public RulesheetDetailTabViewModel(SoftwareOption softwareOption,
                                           TabbedRulesheetViewModel parentViewModel,
                                           ObservableCollection<ControlSystem> availableControlSystems,
                                           ObservableCollection<MachineType> availableMachineTypes)
        {
            _parentViewModel = parentViewModel;
            AvailableControlSystems = availableControlSystems;
            AvailableMachineTypes = availableMachineTypes;

            _originalSoftwareOption = softwareOption.Clone();

            WasOriginallyNew = softwareOption.SoftwareOptionId >= 2000 || softwareOption.SoftwareOptionId == 0;
            EditableSoftwareOption = softwareOption; // This will attach property/collection changed handlers

            SaveCommand = new RelayCommand(PerformSave, CanPerformSave);
            EditCommand = new RelayCommand(PerformEdit, CanPerformEdit);
            CancelCommand = new RelayCommand(PerformCancelEdit, CanPerformCancelEdit);
            CloseTabCommand = new RelayCommand(() => _parentViewModel.CloseTab(this));
            PinTabCommand = new RelayCommand(PerformPinToggle);
            DeleteCommand = new RelayCommand(PerformDelete, CanPerformDelete);

            AddSpecCodeCommand = new RelayCommand(PerformAddSpecCode, CanAddDetailItem);
            AddActivationRuleCommand = new RelayCommand(PerformAddActivationRule, CanAddDetailItem);
            AddRequirementCommand = new RelayCommand(PerformAddRequirement, CanAddDetailItem);

            IsEditMode = WasOriginallyNew;
            UpdateIconAndHeader();
            OnPropertyChanged(nameof(CanAccessDetailedTabs));
        }

        public void UpdateContent(SoftwareOption newSoftwareOption)
        {
            _originalSoftwareOption = newSoftwareOption.Clone();
            WasOriginallyNew = newSoftwareOption.SoftwareOptionId >= 2000 || newSoftwareOption.SoftwareOptionId == 0;
            EditableSoftwareOption = newSoftwareOption; // This will re-attach handlers
            IsEditMode = WasOriginallyNew;
            UpdateIconAndHeader();
            OnPropertyChanged(nameof(CanAccessDetailedTabs));
        }


        private void UpdateIconAndHeader()
        {
            IconKind = IsEditMode ? materialDesign.PackIconKind.FileEditOutline : materialDesign.PackIconKind.FileDocumentOutline;
            string name = string.IsNullOrWhiteSpace(EditableSoftwareOption?.PrimaryName) ? "New Rulesheet" : EditableSoftwareOption.PrimaryName;
            TabHeader = IsDirty ? $"{name}*" : name;
        }

        private bool CanPerformSave() => IsEditMode && IsDirty && EditableSoftwareOption != null && !string.IsNullOrWhiteSpace(EditableSoftwareOption.PrimaryName);
        private void PerformSave()
        {
            if (!CanPerformSave()) return;

            if (EditableSoftwareOption.ControlSystemId.HasValue && EditableSoftwareOption.ControlSystem == null)
            {
                EditableSoftwareOption.ControlSystem = AvailableControlSystems.FirstOrDefault(cs => cs.ControlSystemId == EditableSoftwareOption.ControlSystemId.Value);
            }

            _parentViewModel.SaveRulesheetFromTab(EditableSoftwareOption);
            _originalSoftwareOption = EditableSoftwareOption.Clone();
            IsDirty = false;
            IsEditMode = false;
            _parentViewModel.StatusMessage = $"Saved: {TabDisplayName}";
            WasOriginallyNew = false;
        }

        private bool CanPerformEdit() => !IsEditMode && EditableSoftwareOption != null;
        private void PerformEdit()
        {
            IsEditMode = true;
            _parentViewModel.StatusMessage = $"Editing: {TabDisplayName}";
        }

        private bool CanPerformCancelEdit() => IsEditMode;
        private void PerformCancelEdit()
        {
            if (WasOriginallyNew && !IsPinned)
            {
                IsDirty = false;
                _parentViewModel.CloseTab(this);
                _parentViewModel.StatusMessage = "Add new rulesheet cancelled.";
            }
            else
            {
                // Detach handlers before replacing EditableSoftwareOption
                if (EditableSoftwareOption != null)
                {
                    EditableSoftwareOption.PropertyChanged -= EditableSoftwareOption_PropertyChanged;
                    if (EditableSoftwareOption.SpecificationCodes != null) EditableSoftwareOption.SpecificationCodes.CollectionChanged -= DetailCollectionChanged;
                    if (EditableSoftwareOption.ActivationRules != null)
                    {
                        EditableSoftwareOption.ActivationRules.CollectionChanged -= DetailCollectionChanged;
                        foreach (var item in EditableSoftwareOption.ActivationRules) item.PropertyChanged -= DetailItemPropertyChanged;
                    }
                    if (EditableSoftwareOption.Requirements != null) EditableSoftwareOption.Requirements.CollectionChanged -= DetailCollectionChanged;
                }

                bool oldWasNew = _originalSoftwareOption.SoftwareOptionId >= 2000 || _originalSoftwareOption.SoftwareOptionId == 0;

                EditableSoftwareOption = _originalSoftwareOption.Clone(); // This will re-attach handlers via its setter

                WasOriginallyNew = oldWasNew;
                IsDirty = false;
                IsEditMode = WasOriginallyNew;
                _parentViewModel.StatusMessage = $"Cancelled edits for: {TabDisplayName.TrimEnd('*')}";
            }
        }

        private void PerformPinToggle()
        {
            IsPinned = !IsPinned;
        }

        private bool CanPerformDelete()
        {
            return EditableSoftwareOption != null;
        }

        private void PerformDelete()
        {
            if (EditableSoftwareOption == null) return;

            string displayName = string.IsNullOrWhiteSpace(EditableSoftwareOption.PrimaryName) || WasOriginallyNew ?
                                 "this new/unsaved rulesheet" :
                                 EditableSoftwareOption.PrimaryName;

            var result = MessageBox.Show($"Are you sure you want to delete '{displayName}'?\nThis action cannot be undone, and any unsaved changes in this tab will be lost.",
                                         "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _parentViewModel.HandleDeleteRequest(this);
            }
        }

        private bool CanAddDetailItem()
        {
            return IsEditMode && CanAccessDetailedTabs;
        }

        // --- THIS IS THE UPDATED METHOD ---
        private void PerformAddSpecCode()
        {
            if (EditableSoftwareOption == null || _parentViewModel == null)
            {
                MessageBox.Show("Cannot add spec code: Software Option or Parent ViewModel is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MachineType machineTypeForDialog = null;
            // Attempt to determine MachineType based on the current SoftwareOption's ControlSystem
            if (EditableSoftwareOption.ControlSystem != null)
            {
                string controlNameUpper = EditableSoftwareOption.ControlSystem.Name.ToUpper();
                if (controlNameUpper.Contains("LATHE") || controlNameUpper.Contains("P300L"))
                {
                    machineTypeForDialog = _parentViewModel.AvailableMachineTypes.FirstOrDefault(mt => mt.Name.Equals("Lathe", StringComparison.OrdinalIgnoreCase));
                }
                else if (controlNameUpper.Contains("MACHINING CENTER") || controlNameUpper.Contains("P300M"))
                {
                    machineTypeForDialog = _parentViewModel.AvailableMachineTypes.FirstOrDefault(mt => mt.Name.Equals("Machining Center", StringComparison.OrdinalIgnoreCase));
                }
                // Add more specific mappings if needed, e.g., based on ControlSystem.DefaultMachineType if you add such a property

                if (machineTypeForDialog == null) // Fallback if no specific match from name
                {
                    // If ControlSystem has a default machine type concept, use it.
                    // Otherwise, we might need to prompt the user or use a general default.
                    // For now, using the first available as a last resort if ControlSystem is set.
                    machineTypeForDialog = _parentViewModel.AvailableMachineTypes.FirstOrDefault();
                    _parentViewModel.StatusMessage = $"Warning: Could not infer Machine Type from Control System '{EditableSoftwareOption.ControlSystem.Name}'. Defaulting. Please verify.";
                }
            }

            if (machineTypeForDialog == null) // If still null (e.g. ControlSystem not set on SO, or no AvailableMachineTypes)
            {
                machineTypeForDialog = _parentViewModel.AvailableMachineTypes.FirstOrDefault(); // Absolute fallback
                if (machineTypeForDialog == null)
                {
                    MessageBox.Show("No Machine Types available in the system. Cannot add Specification Code.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _parentViewModel.StatusMessage = $"Warning: Software Option has no Control System set. Defaulting Machine Type to '{machineTypeForDialog.Name}'. Please verify.";
            }


            var dialogViewModel = new AddEditSpecCodeDialogViewModel(
                machineTypeForDialog,
                _parentViewModel.AllGlobalSpecCodeDefinitions, // Pass the global list from TabbedRulesheetViewModel
                EditableSoftwareOption.ActivationRules,
                _parentViewModel // Pass the TabbedRulesheetViewModel instance
            );

            var dialogView = new AddEditSpecCodeDialogView
            {
                DataContext = dialogViewModel,
                Owner = Application.Current.MainWindow // Or find a more specific owner if possible
            };

            if (dialogView.ShowDialog() == true)
            {
                // Dialog was saved by the user
                SpecCodeDefinition resultingSpecDefFromDialog = dialogViewModel.ResultSpecCodeDefinition;

                if (resultingSpecDefFromDialog != null)
                {
                    SpecCodeDefinition finalSpecCodeDefToUse;
                    if (dialogViewModel.IsCreatingNewSpecCodeDefinition)
                    {
                        // The dialog prepared a new SpecCodeDefinition.
                        // Call the parent's GetOrCreate to add it to the global list and get the managed instance.
                        finalSpecCodeDefToUse = _parentViewModel.GetOrCreateSpecCodeDefinition(
                            0, // Pass 0 or temp ID; GetOrCreate will assign a new one if it's truly new
                            resultingSpecDefFromDialog.SpecCodeNo,
                            resultingSpecDefFromDialog.SpecCodeBit,
                            resultingSpecDefFromDialog.Description,
                            resultingSpecDefFromDialog.Category,
                            resultingSpecDefFromDialog.MachineType // Ensure MachineType object is passed
                        );
                    }
                    else
                    {
                        // User selected an existing SpecCodeDefinition
                        finalSpecCodeDefToUse = resultingSpecDefFromDialog;
                    }

                    if (finalSpecCodeDefToUse != null)
                    {
                        var newSosCode = new SoftwareOptionSpecificationCode
                        {
                            // SoftwareOptionSpecificationCodeId will be 0 or assigned by data layer upon actual DB save
                            SoftwareOptionId = EditableSoftwareOption.SoftwareOptionId,
                            SpecCodeDefinitionId = finalSpecCodeDefToUse.SpecCodeDefinitionId,
                            SpecCodeDefinition = finalSpecCodeDefToUse, // Assign the actual object
                            SoftwareOptionActivationRuleId = dialogViewModel.ResultSoftwareOptionActivationRuleId,
                            ActivationRule = dialogViewModel.ResultSoftwareOptionActivationRuleId.HasValue ?
                                             EditableSoftwareOption.ActivationRules.FirstOrDefault(ar => ar.SoftwareOptionActivationRuleId == dialogViewModel.ResultSoftwareOptionActivationRuleId.Value)
                                             : null,
                            SpecificInterpretation = dialogViewModel.SpecificInterpretation
                        };
                        EditableSoftwareOption.SpecificationCodes.Add(newSosCode);
                        // IsDirty will be set by DetailCollectionChanged handler
                        _parentViewModel.StatusMessage = $"Added Spec Code: {finalSpecCodeDefToUse.SpecCodeNo}/{finalSpecCodeDefToUse.SpecCodeBit}";
                    }
                    else
                    {
                        MessageBox.Show("Failed to obtain or create a valid Specification Code Definition from the dialog processing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Dialog did not return a valid Specification Code Definition.", "Operation Cancelled or Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            // else: User cancelled the dialog
        }
        // --- END UPDATED METHOD ---

        private void PerformAddActivationRule()
        {
            if (EditableSoftwareOption != null)
            {
                var newRule = new SoftwareOptionActivationRule
                {
                    // SoftwareOptionActivationRuleId will be 0 or assigned by data layer upon actual DB save
                    RuleName = "New Rule Name",
                    ActivationSetting = "1 = ON",
                    Notes = ""
                };
                EditableSoftwareOption.ActivationRules.Add(newRule);
                // IsDirty flag will be set by the DetailCollectionChanged handler.
            }
        }

        private void PerformAddRequirement()
        {
            MessageBox.Show("Placeholder: Open dialog to add a new Requirement (type, condition, values).", "Add Requirement", MessageBoxButton.OK, MessageBoxImage.Information);
            // When fully implemented, this would add to EditableSoftwareOption.Requirements
            // and IsDirty would be set by the DetailCollectionChanged handler.
        }
    }
}
