using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data; // Required for CollectionViewSource and ICollectionView
using System.Windows.Input;
using RuleArchitectPrototype.Models;
using RuleArchitectPrototype.Commands;
using System.Collections.Generic;

namespace RuleArchitectPrototype.ViewModels
{
    public class RulesheetViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SoftwareOption> Rulesheets { get; set; }
        public ICollectionView RulesheetsView { get; private set; } // For filtering

        private SoftwareOption _selectedRulesheet;
        public SoftwareOption SelectedRulesheet
        {
            get => _selectedRulesheet;
            set
            {
                if (_selectedRulesheet != value)
                {
                    _selectedRulesheet = value;
                    OnPropertyChanged();
                    // When selecting from the filtered view, EditableRulesheet should still clone the original object from the main list if needed,
                    // or ensure cloning is robust if item comes from RulesheetsView. For simplicity, direct clone for now.
                    EditableRulesheet = _selectedRulesheet?.Clone();
                    IsEditMode = false;
                    StatusMessage = _selectedRulesheet != null ? $"Selected: {_selectedRulesheet.PrimaryName}" : "Ready";
                    (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private SoftwareOption _editableRulesheet;
        public SoftwareOption EditableRulesheet
        {
            get => _editableRulesheet;
            set => SetField(ref _editableRulesheet, value);
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetField(ref _isEditMode, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddNewCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    RulesheetsView?.Refresh(); // Trigger filtering
                    StatusMessage = !string.IsNullOrWhiteSpace(_searchText) ? $"Filtering for: '{_searchText}'" : (Rulesheets.Count > 0 ? "Filter cleared." : "Ready");
                    (ClearFilterCommand as RelayCommand)?.RaiseCanExecuteChanged(); // Update CanExecute for ClearFilter
                }
            }
        }

        public ICommand AddNewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearFilterCommand { get; } // New Command

        // Master data for ComboBoxes, etc. (example from previous response)
        public ObservableCollection<ControlSystem> AvailableControlSystems { get; set; }
        public ObservableCollection<MachineType> AvailableMachineTypes { get; set; }


        public RulesheetViewModel()
        {
            Rulesheets = new ObservableCollection<SoftwareOption>();

            AvailableControlSystems = new ObservableCollection<ControlSystem>();
            AvailableMachineTypes = new ObservableCollection<MachineType>();
            LoadMasterData(); // Populate AvailableControlSystems and AvailableMachineTypes

            LoadStaticSampleData(); // This populates Rulesheets

            // Setup ICollectionView for filtering AFTER Rulesheets is populated
            RulesheetsView = CollectionViewSource.GetDefaultView(Rulesheets);
            RulesheetsView.Filter = FilterRulesheetsPredicate;

            AddNewCommand = new RelayCommand(AddNewRulesheet, CanAddNewRulesheet);
            EditCommand = new RelayCommand(EditRulesheet, CanEditRulesheet);
            SaveCommand = new RelayCommand(SaveEdit, CanSaveEdit);
            CancelCommand = new RelayCommand(CancelEdit, CanCancelEdit);
            DeleteCommand = new RelayCommand(DeleteRulesheet, CanDeleteRulesheet);
            ClearFilterCommand = new RelayCommand(ClearFilter, CanClearFilter); // Initialize new command
        }

        private void LoadMasterData()
        {
            // Ensure these are populated before LoadStaticSampleData might try to use them by reference
            // Or, ensure LoadStaticSampleData creates these and then they are added here.
            // For simplicity, let's add them here. The sample data creation will use these instances.

            var csP300L = new ControlSystem { ControlSystemId = 1, Name = "P300L" };
            var csP300MMA = new ControlSystem { ControlSystemId = 2, Name = "P300M/MA" };
            AvailableControlSystems.Add(csP300L);
            AvailableControlSystems.Add(csP300MMA);

            var mtLathe = new MachineType { MachineTypeId = 1, Name = "Lathe" };
            var mtMachiningCenter = new MachineType { MachineTypeId = 2, Name = "Machining Center" };
            AvailableMachineTypes.Add(mtLathe);
            AvailableMachineTypes.Add(mtMachiningCenter);
        }


        private void LoadStaticSampleData()
        {
            // Use ControlSystem and MachineType instances from Available collections
            var csP300L = AvailableControlSystems.FirstOrDefault(cs => cs.ControlSystemId == 1);
            var csP300MMA = AvailableControlSystems.FirstOrDefault(cs => cs.ControlSystemId == 2);
            var mtLathe = AvailableMachineTypes.FirstOrDefault(mt => mt.MachineTypeId == 1);
            var mtMachiningCenter = AvailableMachineTypes.FirstOrDefault(mt => mt.MachineTypeId == 2);

            // --- Barfeeder & Unloader Interface Data ---
            var barfeederOption = new SoftwareOption
            {
                SoftwareOptionId = 1,
                PrimaryName = "BAR FEEDER AND UNLOADER I/F",
                SourceFileName = "Barfeeder & Unloader Interface.csv",
                PrimaryOptionNumberDisplay = "8284",
                Notes = "This option contains same software spec, with or without Any Bus. Difference is contained in the PIOD file.",
                CheckedBy = "Dave",
                CheckedDate = new DateTime(2011, 6, 23),
                ControlSystemId = csP300L?.ControlSystemId, // Use ?. for safety if not found
                ControlSystem = csP300L
            };
            // ... (rest of Barfeeder data population as before, ensuring SpecCodeDefinitions use mtLathe) ...
            // Example for one spec def:
            var specDefBf1 = new SpecCodeDefinition { SpecCodeDefinitionId = 1, SpecCodeNo = "19", SpecCodeBit = "0", Description = "BAR FEEDER I/F 1", Category = "PLC", MachineTypeId = mtLathe.MachineTypeId, MachineType = mtLathe };
            // Ensure all SpecCodeDefinitions under barfeederOption use mtLathe
            var specDefBf2 = new SpecCodeDefinition { SpecCodeDefinitionId = 2, SpecCodeNo = "20", SpecCodeBit = "0", Description = "BF/PC BIT SWITCH", Category = "PLC", MachineTypeId = mtLathe.MachineTypeId, MachineType = mtLathe };
            var specDefBf3 = new SpecCodeDefinition { SpecCodeDefinitionId = 3, SpecCodeNo = "20", SpecCodeBit = "7", Description = "UNLOADER I/F", Category = "PLC", MachineTypeId = mtLathe.MachineTypeId, MachineType = mtLathe };

            var activationRuleBf1 = new SoftwareOptionActivationRule { SoftwareOptionActivationRuleId = 4, RuleName = "Barfeeder IF Enable", ActivationSetting = "1 = ON" };
            var activationRuleBf2 = new SoftwareOptionActivationRule { SoftwareOptionActivationRuleId = 5, RuleName = "BF/PC Bit Switch Enable", ActivationSetting = "1 = On" };
            barfeederOption.ActivationRules.Add(activationRuleBf1);
            barfeederOption.ActivationRules.Add(activationRuleBf2);

            barfeederOption.OptionNumbers.Add(new OptionNumberRegistry { OptionNumberRegistryId = 1, OptionNumber = "8284" });
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 1, SpecCodeDefinitionId = specDefBf1.SpecCodeDefinitionId, SpecCodeDefinition = specDefBf1, SoftwareOptionActivationRuleId = activationRuleBf1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleBf1 });
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 2, SpecCodeDefinitionId = specDefBf2.SpecCodeDefinitionId, SpecCodeDefinition = specDefBf2, SoftwareOptionActivationRuleId = activationRuleBf2.SoftwareOptionActivationRuleId, ActivationRule = activationRuleBf2 });
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 3, SpecCodeDefinitionId = specDefBf3.SpecCodeDefinitionId, SpecCodeDefinition = specDefBf3, SoftwareOptionActivationRuleId = null });
            barfeederOption.Requirements.Add(new Requirement { RequirementId = 1, RequirementType = "OSP_FILE_VERSION", Condition = "HigherThanVersion", OspFileName = "PLC", OspFileVersion = "LU3-410J" });


            Rulesheets.Add(barfeederOption);

            // --- Tool Breakage Detection Data ---
            var toolBreakageOption = new SoftwareOption
            {
                SoftwareOptionId = 2,
                PrimaryName = "TOOL BREAKAGE DETECTION",
                SourceFileName = "Tool Breakage Detection.csv",
                PrimaryOptionNumberDisplay = null,
                Notes = "PIOD Upgrade May Be Necessary. (*) Comments MSB Ref TL 150 is for CAT40 only. (**) Continuous Gauging Requirements. Related rule/option: **8743-9, Tool Breakage Adv/Ret Confirmation(( TouchSensMoves -> force on PLC(8,0) )). ***SPECIAL RULE FOR TBD on DOUBLE COLUMNS*** see MCR-A5CII P219912, additional MSB files MNCUJ85-EL010-1C (cross rail sensor), MNCUJ85-EL040-0C (table sensor). Revision (1/8/2020 DLN): Continuous Gauging Added. Revision (10/28/16 stb): added note for :8743-9.",
                CheckedBy = "Dave",
                CheckedDate = new DateTime(2011, 5, 3),
                ControlSystemId = csP300MMA?.ControlSystemId,
                ControlSystem = csP300MMA
            };
            // ... (rest of Tool Breakage data population as before, ensuring SpecCodeDefinitions use mtMachiningCenter) ...
            // Example for one spec def:
            var specDefTb1 = new SpecCodeDefinition { SpecCodeDefinitionId = 4, SpecCodeNo = "4", SpecCodeBit = "2", Description = "Skip Function", Category = "NC", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };
            // Ensure all SpecCodeDefinitions under toolBreakageOption use mtMachiningCenter
            var specDefTb2 = new SpecCodeDefinition { SpecCodeDefinitionId = 5, SpecCodeNo = "21", SpecCodeBit = "0", Description = "SpareTL.change", Category = "NC", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };
            var specDefTb3 = new SpecCodeDefinition { SpecCodeDefinitionId = 6, SpecCodeNo = "21", SpecCodeBit = "2", Description = "CRT-display", Category = "NC", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };
            var specDefTb4 = new SpecCodeDefinition { SpecCodeDefinitionId = 7, SpecCodeNo = "21", SpecCodeBit = "6", Description = "Tl. Comp/brk", Category = "NC", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };
            var specDefTb5 = new SpecCodeDefinition { SpecCodeDefinitionId = 8, SpecCodeNo = "24", SpecCodeBit = "0", Description = "MSB-TLCmp/Brk1", Category = "NC", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };
            var specDefTb6 = new SpecCodeDefinition { SpecCodeDefinitionId = 9, SpecCodeNo = "24", SpecCodeBit = "1", Description = "MSB-TLCmp/brk", Category = "NC", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };
            var specDefTb7 = new SpecCodeDefinition { SpecCodeDefinitionId = 10, SpecCodeNo = "24", SpecCodeBit = "2", Description = "MSB-TLbrk. Det", Category = "NC", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };
            var specDefTb8 = new SpecCodeDefinition { SpecCodeDefinitionId = 11, SpecCodeNo = "24", SpecCodeBit = "7", Description = "MSB-Ref. Tl. 150*", Category = "NC", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };
            var specDefTb9 = new SpecCodeDefinition { SpecCodeDefinitionId = 12, SpecCodeNo = "9", SpecCodeBit = "4", Description = "Conti.Gaug.tool**", Category = "NC-B", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };

            var activationRuleTb1 = new SoftwareOptionActivationRule { SoftwareOptionActivationRuleId = 1, RuleName = "Tool Break General Enable", ActivationSetting = "1=ON" };
            var activationRuleTb2 = new SoftwareOptionActivationRule { SoftwareOptionActivationRuleId = 2, RuleName = "Tool Compensation/Break General Enable", ActivationSetting = "1=ON" };
            var activationRuleTb3 = new SoftwareOptionActivationRule { SoftwareOptionActivationRuleId = 3, RuleName = "Continuous Gauging Tool Enable", ActivationSetting = "1=ON" };
            toolBreakageOption.ActivationRules.Add(activationRuleTb1);
            toolBreakageOption.ActivationRules.Add(activationRuleTb2);
            toolBreakageOption.ActivationRules.Add(activationRuleTb3);

            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 4, SpecCodeDefinition = specDefTb9, SoftwareOptionActivationRuleId = activationRuleTb3.SoftwareOptionActivationRuleId, ActivationRule = activationRuleTb3 });
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 5, SpecCodeDefinition = specDefTb4, SoftwareOptionActivationRuleId = activationRuleTb2.SoftwareOptionActivationRuleId, ActivationRule = activationRuleTb2 });
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 6, SpecCodeDefinition = specDefTb7, SoftwareOptionActivationRuleId = activationRuleTb1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleTb1 });
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 7, SpecCodeDefinition = specDefTb1 });
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 8, SpecCodeDefinition = specDefTb2 });
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 9, SpecCodeDefinition = specDefTb3 });
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 10, SpecCodeDefinition = specDefTb5 });
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 11, SpecCodeDefinition = specDefTb6 });
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 12, SpecCodeDefinition = specDefTb8 });

            toolBreakageOption.Requirements.Add(new Requirement { RequirementId = 2, RequirementType = "OSP_FILE_VERSION", Condition = "MinimumVersion", OspFileName = "NC", OspFileVersion = "MNC-307M-P300, or MNC-307K-Y093B-P300, or MNC-307L-Y040C-P300", Notes = "or Higher" });
            toolBreakageOption.Requirements.Add(new Requirement { RequirementId = 3, RequirementType = "OSP_FILE_VERSION", Condition = "HigherThanVersion", OspFileName = "MMSH015A.MSB" });
            toolBreakageOption.Requirements.Add(new Requirement { RequirementId = 4, RequirementType = "OSP_FILE_VERSION", Condition = "HigherThanVersion", OspFileName = "MMSA121M*", Notes = "for P300 Standard" });
            toolBreakageOption.Requirements.Add(new Requirement { RequirementId = 6, RequirementType = "GENERAL_TEXT", GeneralRequiredValue = "PIOD Upgrade May Be Necessary" });


            Rulesheets.Add(toolBreakageOption);
        }

        private bool FilterRulesheetsPredicate(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true; // No filter, show all

            if (item is SoftwareOption option)
            {
                // Search in PrimaryName, OptionNumberDisplay, and ControlSystem Name
                return (option.PrimaryName?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                       (option.PrimaryOptionNumberDisplay?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                       (option.ControlSystem?.Name?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return false;
        }

        private bool CanClearFilter() => !string.IsNullOrWhiteSpace(SearchText);
        private void ClearFilter()
        {
            SearchText = string.Empty; // This will trigger RulesheetsView.Refresh() via setter
        }

        // --- Existing CRUD methods ---
        private bool CanAddNewRulesheet() => !IsEditMode;
        private void AddNewRulesheet()
        {
            EditableRulesheet = new SoftwareOption() { PrimaryName = "New Rulesheet", CheckedDate = DateTime.Now };
            IsEditMode = true;
            StatusMessage = "Adding new rulesheet...";
        }

        private bool CanEditRulesheet() => SelectedRulesheet != null && !IsEditMode;
        private void EditRulesheet()
        {
            if (SelectedRulesheet != null)
            {
                EditableRulesheet = SelectedRulesheet.Clone();
                IsEditMode = true;
                StatusMessage = $"Editing: {EditableRulesheet.PrimaryName}";
            }
        }

        private bool CanSaveEdit() => IsEditMode && EditableRulesheet != null && !string.IsNullOrWhiteSpace(EditableRulesheet.PrimaryName);
        private void SaveEdit()
        {
            if (EditableRulesheet == null) return;

            // Find original in the main list (if it's an edit)
            var originalItem = Rulesheets.FirstOrDefault(r => r.SoftwareOptionId == EditableRulesheet.SoftwareOptionId);

            if (originalItem != null) // Existing item being edited
            {
                int index = Rulesheets.IndexOf(originalItem);
                Rulesheets[index] = EditableRulesheet;
            }
            else // New item
            {
                // Assign a new ID if it's a truly new item (simple max ID + 1 for this demo)
                if (EditableRulesheet.SoftwareOptionId == 0)
                {
                    EditableRulesheet.SoftwareOptionId = (Rulesheets.Any() ? Rulesheets.Max(r => r.SoftwareOptionId) : 0) + 1;
                }
                Rulesheets.Add(EditableRulesheet);
            }
            SelectedRulesheet = EditableRulesheet; // Make the saved/added item the current selection
            IsEditMode = false;
            RulesheetsView.Refresh(); // Refresh view in case of add/edit affecting filter
            StatusMessage = $"Saved: {EditableRulesheet.PrimaryName}";
        }

        private bool CanCancelEdit() => IsEditMode;
        private void CancelEdit()
        {
            if (SelectedRulesheet != null && Rulesheets.Any(r => r.SoftwareOptionId == SelectedRulesheet.SoftwareOptionId && SelectedRulesheet.SoftwareOptionId != 0))
            {
                // Re-fetch the original from the master list if it was an edit operation.
                EditableRulesheet = Rulesheets.FirstOrDefault(r => r.SoftwareOptionId == SelectedRulesheet.SoftwareOptionId)?.Clone();
                StatusMessage = $"Edit cancelled for: {SelectedRulesheet.PrimaryName}";
            }
            else
            {
                EditableRulesheet = null; // It was a new item, just discard
                StatusMessage = "Add new rulesheet cancelled.";
            }
            IsEditMode = false;
        }

        private bool CanDeleteRulesheet() => SelectedRulesheet != null && !IsEditMode;
        private void DeleteRulesheet()
        {
            if (SelectedRulesheet != null)
            {
                string deletedName = SelectedRulesheet.PrimaryName;
                Rulesheets.Remove(SelectedRulesheet); // Remove from the source collection
                // RulesheetsView will refresh automatically or explicitly call RulesheetsView.Refresh();
                SelectedRulesheet = null;
                EditableRulesheet = null;
                StatusMessage = $"Rulesheet '{deletedName}' deleted.";
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}