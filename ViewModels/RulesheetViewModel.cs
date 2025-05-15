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
        public ICommand ClearFilterCommand { get; }

        public ObservableCollection<ControlSystem> AvailableControlSystems { get; set; }
        public ObservableCollection<MachineType> AvailableMachineTypes { get; set; }
        public ObservableCollection<SpecCodeDefinition> AllSpecCodeDefinitions { get; set; }
        public ObservableCollection<SoftwareOptionActivationRule> AllActivationRules { get; set; }


        public RulesheetViewModel()
        {
            Rulesheets = new ObservableCollection<SoftwareOption>();
            AvailableControlSystems = new ObservableCollection<ControlSystem>();
            AvailableMachineTypes = new ObservableCollection<MachineType>();
            AllSpecCodeDefinitions = new ObservableCollection<SpecCodeDefinition>();
            AllActivationRules = new ObservableCollection<SoftwareOptionActivationRule>();


            LoadMasterData();
            LoadStaticSampleData();

            RulesheetsView = CollectionViewSource.GetDefaultView(Rulesheets);
            RulesheetsView.Filter = FilterRulesheetsPredicate;

            AddNewCommand = new RelayCommand(AddNewRulesheet, CanAddNewRulesheet);
            EditCommand = new RelayCommand(EditRulesheet, CanEditRulesheet);
            SaveCommand = new RelayCommand(SaveEdit, CanSaveEdit);
            CancelCommand = new RelayCommand(CancelEdit, CanCancelEdit);
            DeleteCommand = new RelayCommand(DeleteRulesheet, CanDeleteRulesheet);
            ClearFilterCommand = new RelayCommand(ClearFilter, CanClearFilter);
        }

        private void LoadMasterData()
        {
            var csP300L = new ControlSystem { ControlSystemId = 1, Name = "P300L" };
            var csP300MMA = new ControlSystem { ControlSystemId = 2, Name = "P300M/MA" };
            var csOSP_Unknown = new ControlSystem { ControlSystemId = 3, Name = "OSP Unknown" };
            var csOSP_P300S_P300L = new ControlSystem { ControlSystemId = 4, Name = "OSP-P300S/P300L" };


            AvailableControlSystems.Add(csP300L);
            AvailableControlSystems.Add(csP300MMA);
            AvailableControlSystems.Add(csOSP_Unknown);
            AvailableControlSystems.Add(csOSP_P300S_P300L);


            var mtLathe = new MachineType { MachineTypeId = 1, Name = "Lathe" };
            var mtMachiningCenter = new MachineType { MachineTypeId = 2, Name = "Machining Center" };
            var mtUnknown = new MachineType { MachineTypeId = 3, Name = "Unknown" };
            var mtOkuma = new MachineType { MachineTypeId = 4, Name = "Okuma Generic" };


            AvailableMachineTypes.Add(mtLathe);
            AvailableMachineTypes.Add(mtMachiningCenter);
            AvailableMachineTypes.Add(mtUnknown);
            AvailableMachineTypes.Add(mtOkuma);
        }

        private SpecCodeDefinition GetOrCreateSpecCodeDefinition(int id, string specNo, string specBit, string desc, string cat, MachineType mt)
        {
            var existing = AllSpecCodeDefinitions.FirstOrDefault(s => s.SpecCodeDefinitionId == id);
            if (existing != null)
            {
                // Update if different, respecting new rules
                existing.SpecCodeNo = specNo; // Assuming new specNo is already validated
                existing.SpecCodeBit = specBit; // Assuming new specBit is already validated
                existing.Description = desc;
                existing.Category = cat; // Assuming new cat is one of "NC1", "NC2", "PLC1", "PLC2"
                existing.MachineTypeId = mt.MachineTypeId;
                existing.MachineType = mt;
                return existing;
            }

            var newSpecDef = new SpecCodeDefinition
            {
                SpecCodeDefinitionId = id,
                SpecCodeNo = specNo,
                SpecCodeBit = specBit,
                Description = desc,
                Category = cat,
                MachineTypeId = mt.MachineTypeId,
                MachineType = mt
            };
            AllSpecCodeDefinitions.Add(newSpecDef);
            return newSpecDef;
        }

        private SoftwareOptionActivationRule GetOrCreateActivationRule(int id, string ruleName, string setting)
        {
            var existing = AllActivationRules.FirstOrDefault(ar => ar.SoftwareOptionActivationRuleId == id);
            if (existing != null) return existing;

            var newRule = new SoftwareOptionActivationRule
            {
                SoftwareOptionActivationRuleId = id,
                RuleName = ruleName,
                ActivationSetting = setting
            };
            AllActivationRules.Add(newRule);
            return newRule;
        }


        private void LoadStaticSampleData()
        {
            var csP300L = AvailableControlSystems.FirstOrDefault(cs => cs.Name == "P300L");
            var csP300MMA = AvailableControlSystems.FirstOrDefault(cs => cs.Name == "P300M/MA");
            var csOSP_P300S_L = AvailableControlSystems.FirstOrDefault(cs => cs.Name == "OSP-P300S/P300L") ?? csP300MMA;
            var csUnknown = AvailableControlSystems.FirstOrDefault(cs => cs.Name == "OSP Unknown") ?? csP300MMA;

            var mtLathe = AvailableMachineTypes.FirstOrDefault(mt => mt.Name == "Lathe");
            var mtMachiningCenter = AvailableMachineTypes.FirstOrDefault(mt => mt.Name == "Machining Center");
            var mtUnknownParsed = AvailableMachineTypes.FirstOrDefault(mt => mt.Name == "Unknown") ?? mtMachiningCenter;
            var mtOkumaParsed = AvailableMachineTypes.FirstOrDefault(mt => mt.Name == "Okuma Generic") ?? mtMachiningCenter;


            int nextSpecDefId = AllSpecCodeDefinitions.Any() ? AllSpecCodeDefinitions.Max(s => s.SpecCodeDefinitionId) + 1 : 1;
            int nextActivationRuleId = AllActivationRules.Any() ? AllActivationRules.Max(ar => ar.SoftwareOptionActivationRuleId) + 1 : 1;
            int nextSoftwareOptionIdCounter = Rulesheets.Any() ? Rulesheets.Max(r => r.SoftwareOptionId) + 1 : 1; // Renamed for clarity


            // --- Barfeeder & Unloader Interface Data (Adjusted for new SpecCode rules) ---
            var barfeederOption = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "BAR FEEDER AND UNLOADER I/F",
                SourceFileName = "Barfeeder & Unloader Interface.csv",
                PrimaryOptionNumberDisplay = "8284",
                Notes = "This option contains same software spec, with or without Any Bus. Difference is contained in the PIOD file.",
                CheckedBy = "Dave",
                CheckedDate = new DateTime(2011, 6, 23),
                ControlSystemId = csP300L?.ControlSystemId,
                ControlSystem = csP300L
            };
            var specDefBf1 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "19", "0", "BAR FEEDER I/F 1", "PLC1", mtLathe); // Category updated
            var specDefBf2 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "20", "0", "BF/PC BIT SWITCH", "PLC1", mtLathe); // Category updated
            var specDefBf3 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "20", "7", "UNLOADER I/F", "PLC2", mtLathe);       // Category updated
            var activationRuleBf1 = GetOrCreateActivationRule(nextActivationRuleId++, "Barfeeder IF Enable", "1 = ON");
            var activationRuleBf2 = GetOrCreateActivationRule(nextActivationRuleId++, "BF/PC Bit Switch Enable", "1 = On");
            barfeederOption.ActivationRules.Add(activationRuleBf1);
            barfeederOption.ActivationRules.Add(activationRuleBf2);
            barfeederOption.OptionNumbers.Add(new OptionNumberRegistry { OptionNumberRegistryId = 1, OptionNumber = "8284" });
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 1, SpecCodeDefinitionId = specDefBf1.SpecCodeDefinitionId, SpecCodeDefinition = specDefBf1, SoftwareOptionActivationRuleId = activationRuleBf1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleBf1 });
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 2, SpecCodeDefinitionId = specDefBf2.SpecCodeDefinitionId, SpecCodeDefinition = specDefBf2, SoftwareOptionActivationRuleId = activationRuleBf2.SoftwareOptionActivationRuleId, ActivationRule = activationRuleBf2 });
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 3, SpecCodeDefinitionId = specDefBf3.SpecCodeDefinitionId, SpecCodeDefinition = specDefBf3 });
            barfeederOption.Requirements.Add(new Requirement { RequirementId = 1, RequirementType = "OSP_FILE_VERSION", Condition = "HigherThanVersion", OspFileName = "PLC", OspFileVersion = "LU3-410J" });
            Rulesheets.Add(barfeederOption);

            // --- Tool Breakage Detection Data (Adjusted for new SpecCode rules) ---
            var toolBreakageOption = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "TOOL BREAKAGE DETECTION",
                SourceFileName = "Tool Breakage Detection.csv",
                Notes = "PIOD Upgrade May Be Necessary. (*) Comments MSB Ref TL 150 is for CAT40 only. (**) Continuous Gauging Requirements. Related rule/option: **8743-9, Tool Breakage Adv/Ret Confirmation(( TouchSensMoves -> force on PLC(8,0) )). ***SPECIAL RULE FOR TBD on DOUBLE COLUMNS*** see MCR-A5CII P219912, additional MSB files MNCUJ85-EL010-1C (cross rail sensor), MNCUJ85-EL040-0C (table sensor). Revision (1/8/2020 DLN): Continuous Gauging Added. Revision (10/28/16 stb): added note for :8743-9.",
                CheckedBy = "Dave",
                CheckedDate = new DateTime(2011, 5, 3),
                ControlSystemId = csP300MMA?.ControlSystemId,
                ControlSystem = csP300MMA
            };
            var specDefTb1 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "4", "2", "Skip Function", "NC1", mtMachiningCenter); // Category updated
            var specDefTb2 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "21", "0", "SpareTL.change", "NC1", mtMachiningCenter); // Category updated
            var specDefTb3 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "21", "2", "CRT-display", "NC1", mtMachiningCenter);    // Category updated
            var specDefTb4 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "21", "6", "Tl. Comp/brk", "NC2", mtMachiningCenter);    // Category updated
            var specDefTb5 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "24", "0", "MSB-TLCmp/Brk1", "NC2", mtMachiningCenter); // Category updated
            var specDefTb6 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "24", "1", "MSB-TLCmp/brk", "NC2", mtMachiningCenter);  // Category updated
            var specDefTb7 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "24", "2", "MSB-TLbrk. Det", "NC1", mtMachiningCenter); // Category updated
            var specDefTb8 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "24", "7", "MSB-Ref. Tl. 150*", "NC1", mtMachiningCenter); // Category updated
            var specDefTb9 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "9", "4", "Conti.Gaug.tool**", "NC2", mtMachiningCenter); // Category updated from NC-B

            var activationRuleTb1 = GetOrCreateActivationRule(nextActivationRuleId++, "Tool Break General Enable", "1=ON");
            var activationRuleTb2 = GetOrCreateActivationRule(nextActivationRuleId++, "Tool Compensation/Break General Enable", "1=ON");
            var activationRuleTb3 = GetOrCreateActivationRule(nextActivationRuleId++, "Continuous Gauging Tool Enable", "1=ON");
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

            // --- NEW ENTRIES START HERE (Adjusted for new SpecCode rules) ---

            // 1. Hyper-Surface Function - 3 Axis
            var hyperSurfaceOption = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "Hyper-Surface Function - 3 Axis",
                SourceFileName = "Hyper-Surface Function - 3 Axis.csv",
                ControlSystemId = csP300MMA?.ControlSystemId,
                ControlSystem = csP300MMA,
                CheckedBy = "Auto",
                CheckedDate = DateTime.Now
            };
            // Original from parsing: (1, 0, "Hyper-Surface Function (G341) (3axis)", "NC Option")
            var specDefHs1 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "1", "0", "Hyper-Surface Function (G341) (3axis)", "NC1", mtMachiningCenter); // No/Bit OK, Category to NC1
            var activationRuleHs1 = GetOrCreateActivationRule(nextActivationRuleId++, "Hyper-Surface Enable", "1=ON");
            hyperSurfaceOption.ActivationRules.Add(activationRuleHs1);
            hyperSurfaceOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSoftwareOptionIdCounter++, SpecCodeDefinition = specDefHs1, SoftwareOptionActivationRuleId = activationRuleHs1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleHs1, SpecificInterpretation = "1=ON" });
            Rulesheets.Add(hyperSurfaceOption);

            // 2. DNC-T3
            var dncT3Option = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "DNC-T3",
                SourceFileName = "DNC-T3.csv",
                ControlSystemId = csOSP_P300S_L?.ControlSystemId,
                ControlSystem = csOSP_P300S_L,
                CheckedBy = "Auto",
                CheckedDate = DateTime.Now
            };
            // Original from parsing: (100, 0, "AIRBAG SYSTEM DNC-T1 -> T3", "") and (100, 6, "DNC-T3", "")
            // "100" is out of range (1-32). Let's use "32" and note original in desc.
            var specDefDnc1 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "32", "0", "AIRBAG SYSTEM DNC-T1 -> T3 (Orig No 100)", "PLC1", mtOkumaParsed);
            var specDefDnc2 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "32", "6", "DNC-T3 (Orig No 100)", "PLC2", mtOkumaParsed);
            var activationRuleDnc1 = GetOrCreateActivationRule(nextActivationRuleId++, "DNC Airbag Enable", "AIRBAG SYSTEM DNC-T1 -> T3");
            var activationRuleDnc2 = GetOrCreateActivationRule(nextActivationRuleId++, "DNC-T3 Enable", "1=ON");
            dncT3Option.ActivationRules.Add(activationRuleDnc1);
            dncT3Option.ActivationRules.Add(activationRuleDnc2);
            dncT3Option.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSoftwareOptionIdCounter++, SpecCodeDefinition = specDefDnc1, SoftwareOptionActivationRuleId = activationRuleDnc1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleDnc1, SpecificInterpretation = "DNC-T1->T3" });
            dncT3Option.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSoftwareOptionIdCounter++, SpecCodeDefinition = specDefDnc2, SoftwareOptionActivationRuleId = activationRuleDnc2.SoftwareOptionActivationRuleId, ActivationRule = activationRuleDnc2, SpecificInterpretation = "1=ON" });
            Rulesheets.Add(dncT3Option);

            // 3. Steady Rest Enable-Disable Switch
            var steadyRestSwitchOption = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "Steady Rest Enable-Disable Switch",
                SourceFileName = "Steady Rest Enable-Disable Switch.csv",
                ControlSystemId = csP300L?.ControlSystemId,
                ControlSystem = csP300L,
                CheckedBy = "Auto",
                CheckedDate = DateTime.Now
            };
            // Original example: (70, 5, "Steady Rest Enable Switch", "PLC") -> "70" is out of range.
            var specDefSr1 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "25", "5", "Steady Rest Enable Switch (Orig No 70)", "PLC1", mtLathe);
            var activationRuleSr1 = GetOrCreateActivationRule(nextActivationRuleId++, "SR Switch Enable", "1=ON");
            steadyRestSwitchOption.ActivationRules.Add(activationRuleSr1);
            steadyRestSwitchOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSoftwareOptionIdCounter++, SpecCodeDefinition = specDefSr1, SoftwareOptionActivationRuleId = activationRuleSr1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleSr1 });
            Rulesheets.Add(steadyRestSwitchOption);

            // 4. Dynamic Fixture Tracking
            var dynamicFixtureOption = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "Dynamic Fixture Tracking",
                SourceFileName = "Dynamic Fixture Tracking.csv",
                ControlSystemId = csP300MMA?.ControlSystemId,
                ControlSystem = csP300MMA,
                CheckedBy = "Auto",
                CheckedDate = DateTime.Now
            };
            // Original example: (80, 2, "Dynamic Fixture Tracking Active", "NC Option") -> "80" out of range.
            var specDefDf1 = GetOrCreateSpecCodeDefinition(nextSpecDefId++, "30", "2", "Dynamic Fixture Tracking Active (Orig No 80)", "NC1", mtMachiningCenter);
            var activationRuleDf1 = GetOrCreateActivationRule(nextActivationRuleId++, "DFT Active", "1=ON");
            dynamicFixtureOption.ActivationRules.Add(activationRuleDf1);
            dynamicFixtureOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSoftwareOptionIdCounter++, SpecCodeDefinition = specDefDf1, SoftwareOptionActivationRuleId = activationRuleDf1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleDf1 });
            Rulesheets.Add(dynamicFixtureOption);
        }


        private bool FilterRulesheetsPredicate(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            if (item is SoftwareOption option)
            {
                return (option.PrimaryName?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                       (option.PrimaryOptionNumberDisplay?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                       (option.ControlSystem?.Name?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return false;
        }

        private bool CanClearFilter() => !string.IsNullOrWhiteSpace(SearchText);
        private void ClearFilter()
        {
            SearchText = string.Empty;
        }

        private bool CanAddNewRulesheet() => !IsEditMode;
        private void AddNewRulesheet()
        {
            // Assign a temporary high ID for new items not yet saved to differentiate from persisted items
            int tempNewId = (Rulesheets.Any() ? Rulesheets.Max(r => r.SoftwareOptionId) : 0) + 1000;
            if (tempNewId < 1000) tempNewId = 1000; // Ensure it's always a high number

            EditableRulesheet = new SoftwareOption() { SoftwareOptionId = tempNewId, PrimaryName = "New Rulesheet", CheckedDate = DateTime.Now };
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

            // Check if it's an attempt to save an item that was for 'Add New' (using temp ID)
            bool isNewItemFromAdd = EditableRulesheet.SoftwareOptionId >= 1000;

            var originalItem = isNewItemFromAdd ? null : Rulesheets.FirstOrDefault(r => r.SoftwareOptionId == EditableRulesheet.SoftwareOptionId);

            if (originalItem != null) // Existing item being edited
            {
                int index = Rulesheets.IndexOf(originalItem);
                Rulesheets[index] = EditableRulesheet;
            }
            else // New item
            {
                // Assign a new permanent ID
                EditableRulesheet.SoftwareOptionId = (Rulesheets.Any(r => r.SoftwareOptionId < 1000) ? Rulesheets.Where(r => r.SoftwareOptionId < 1000).Max(r => r.SoftwareOptionId) : 0) + 1;
                Rulesheets.Add(EditableRulesheet);
            }
            SelectedRulesheet = EditableRulesheet;
            IsEditMode = false;
            RulesheetsView.Refresh();
            StatusMessage = $"Saved: {EditableRulesheet.PrimaryName}";
        }

        private bool CanCancelEdit() => IsEditMode;
        private void CancelEdit()
        {
            // If EditableRulesheet exists and its ID suggests it's not a new temporary item
            if (EditableRulesheet != null && EditableRulesheet.SoftwareOptionId < 1000 && EditableRulesheet.SoftwareOptionId != 0)
            {
                // Try to find the original based on SelectedRulesheet if it matches the ID, otherwise just clear.
                if (SelectedRulesheet != null && SelectedRulesheet.SoftwareOptionId == EditableRulesheet.SoftwareOptionId)
                {
                    EditableRulesheet = SelectedRulesheet.Clone(); // Revert to original clone of selected
                    StatusMessage = $"Edit cancelled for: {SelectedRulesheet.PrimaryName}";
                }
                else // Editable was changed, or selection changed. Fallback to clearing.
                {
                    EditableRulesheet = null;
                    StatusMessage = "Edit cancelled.";
                }
            }
            else // It was a new item (temp ID >= 1000 or 0), or EditableRulesheet is null
            {
                EditableRulesheet = null;
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
                Rulesheets.Remove(SelectedRulesheet);
                SelectedRulesheet = null;
                EditableRulesheet = null;
                StatusMessage = $"Rulesheet '{deletedName}' deleted.";
            }
        }

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