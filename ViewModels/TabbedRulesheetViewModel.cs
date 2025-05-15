using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using RuleArchitectPrototype.Models;
using RuleArchitectPrototype.Commands;
using System.Windows;
using System.Collections.Generic;

namespace RuleArchitectPrototype.ViewModels
{
    public class TabbedRulesheetViewModel : BaseModel
    {
        internal ObservableCollection<SoftwareOption> _allRulesheetsMasterList;
        public ICollectionView AllRulesheetsView { get; private set; }

        private string _searchAllText;
        public string SearchAllText
        {
            get => _searchAllText;
            set
            {
                if (SetField(ref _searchAllText, value))
                {
                    AllRulesheetsView?.Refresh();
                }
            }
        }

        private ControlSystem _selectedControlFilter;
        public ControlSystem SelectedControlFilter
        {
            get => _selectedControlFilter;
            set
            {
                if (SetField(ref _selectedControlFilter, value))
                {
                    AllRulesheetsView?.Refresh();
                    (ClearControlFilterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isSearchVisible;
        public bool IsSearchVisible
        {
            get => _isSearchVisible;
            set => SetField(ref _isSearchVisible, value);
        }

        private bool _isControlFilterVisible;
        public bool IsControlFilterVisible
        {
            get => _isControlFilterVisible;
            set => SetField(ref _isControlFilterVisible, value);
        }

        public ObservableCollection<RulesheetDetailTabViewModel> OpenTabs { get; }

        private RulesheetDetailTabViewModel _selectedTab;
        public RulesheetDetailTabViewModel SelectedTab
        {
            get => _selectedTab;
            set => SetField(ref _selectedTab, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        public ICommand OpenRulesheetInTabCommand { get; }
        public ICommand AddNewRulesheetTabCommand { get; }
        public ICommand ClearControlFilterCommand { get; }
        public ICommand ToggleSearchVisibilityCommand { get; }
        public ICommand ToggleControlFilterVisibilityCommand { get; }

        public ObservableCollection<ControlSystem> AvailableControlSystems { get; }
        public ObservableCollection<MachineType> AvailableMachineTypes { get; }

        // Made internal and ObservableCollection for consistency and potential binding if ever needed
        internal ObservableCollection<SpecCodeDefinition> AllGlobalSpecCodeDefinitions { get; private set; }
        private List<SoftwareOptionActivationRule> _globalActivationRules; // Kept as List if not directly bound


        public TabbedRulesheetViewModel()
        {
            _allRulesheetsMasterList = new ObservableCollection<SoftwareOption>();
            OpenTabs = new ObservableCollection<RulesheetDetailTabViewModel>();
            SelectedTab = null;

            AvailableControlSystems = new ObservableCollection<ControlSystem>();
            AvailableMachineTypes = new ObservableCollection<MachineType>();
            AllGlobalSpecCodeDefinitions = new ObservableCollection<SpecCodeDefinition>(); // Initialize
            _globalActivationRules = new List<SoftwareOptionActivationRule>();

            IsSearchVisible = false;
            IsControlFilterVisible = false;

            LoadMasterData();
            LoadAllRulesheetsSampleData();

            AllRulesheetsView = CollectionViewSource.GetDefaultView(_allRulesheetsMasterList);
            AllRulesheetsView.Filter = FilterAllRulesheetsPredicate;

            OpenRulesheetInTabCommand = new RelayCommand(
                param => OpenRulesheetInTab(param as SoftwareOption),
                param => CanOpenRulesheetInTab(param as SoftwareOption)
            );
            AddNewRulesheetTabCommand = new RelayCommand(AddNewRulesheetTab);
            ClearControlFilterCommand = new RelayCommand(PerformClearControlFilter, CanClearControlFilter);
            ToggleSearchVisibilityCommand = new RelayCommand(PerformToggleSearchVisibility);
            ToggleControlFilterVisibilityCommand = new RelayCommand(PerformToggleControlFilterVisibility);

            StatusMessage = "Select a rulesheet to open or click 'Add New'.";
        }

        private void PerformToggleSearchVisibility()
        {
            IsSearchVisible = !IsSearchVisible;
            if (!IsSearchVisible && !string.IsNullOrWhiteSpace(SearchAllText))
            {
                SearchAllText = string.Empty;
            }
        }

        private void PerformToggleControlFilterVisibility()
        {
            IsControlFilterVisible = !IsControlFilterVisible;
            if (!IsControlFilterVisible && SelectedControlFilter != null)
            {
                SelectedControlFilter = null;
            }
        }

        private bool FilterAllRulesheetsPredicate(object item)
        {
            if (!(item is SoftwareOption option)) return false;

            bool matchesSearchText = true;
            if (IsSearchVisible && !string.IsNullOrWhiteSpace(SearchAllText))
            {
                matchesSearchText = (option.PrimaryName?.IndexOf(SearchAllText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (option.PrimaryOptionNumberDisplay?.IndexOf(SearchAllText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (option.ControlSystem?.Name?.IndexOf(SearchAllText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            bool matchesControlFilter = true;
            if (IsControlFilterVisible && SelectedControlFilter != null)
            {
                matchesControlFilter = option.ControlSystemId == SelectedControlFilter.ControlSystemId;
            }

            return matchesSearchText && matchesControlFilter;
        }

        private bool CanClearControlFilter()
        {
            return SelectedControlFilter != null;
        }

        private void PerformClearControlFilter()
        {
            SelectedControlFilter = null;
        }

        private bool CanOpenRulesheetInTab(SoftwareOption option) => option != null;

        private void OpenRulesheetInTab(SoftwareOption optionToOpen)
        {
            if (optionToOpen == null) return;

            var existingPinnedTab = OpenTabs.FirstOrDefault(tab => tab.IsPinned && tab.SoftwareOptionId == optionToOpen.SoftwareOptionId);
            if (existingPinnedTab != null)
            {
                SelectedTab = existingPinnedTab;
                StatusMessage = $"Activated pinned tab for: {optionToOpen.PrimaryName}";
                return;
            }

            var unpinnedPreviewTab = OpenTabs.FirstOrDefault(tab => !tab.IsPinned);

            if (unpinnedPreviewTab != null)
            {
                if (unpinnedPreviewTab.IsDirty)
                {
                    var result = MessageBox.Show($"Save changes to '{unpinnedPreviewTab.TabDisplayName.TrimEnd('*')}' before switching?",
                                                 "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        unpinnedPreviewTab.SaveCommand.Execute(null);
                        if (unpinnedPreviewTab.IsDirty) return;
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                if (unpinnedPreviewTab.SoftwareOptionId == optionToOpen.SoftwareOptionId && !unpinnedPreviewTab.IsDirty)
                {
                    SelectedTab = unpinnedPreviewTab;
                    StatusMessage = $"Previewing: {optionToOpen.PrimaryName}";
                    return;
                }

                unpinnedPreviewTab.UpdateContent(optionToOpen.Clone());
                SelectedTab = unpinnedPreviewTab;
                StatusMessage = $"Previewing: {optionToOpen.PrimaryName}";
            }
            else
            {
                var newTabViewModel = new RulesheetDetailTabViewModel(optionToOpen.Clone(), this, AvailableControlSystems, AvailableMachineTypes)
                {
                    IsPinned = false
                };
                OpenTabs.Add(newTabViewModel);
                SelectedTab = newTabViewModel;
                StatusMessage = $"Previewing: {optionToOpen.PrimaryName}";
            }
        }

        private void AddNewRulesheetTab()
        {
            var unpinnedPreviewTab = OpenTabs.FirstOrDefault(tab => !tab.IsPinned);
            if (unpinnedPreviewTab != null)
            {
                if (unpinnedPreviewTab.IsDirty)
                {
                    var result = MessageBox.Show($"Save changes to '{unpinnedPreviewTab.TabDisplayName.TrimEnd('*')}' before creating a new one?",
                                                "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        unpinnedPreviewTab.SaveCommand.Execute(null);
                        if (unpinnedPreviewTab.IsDirty) return;
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                int tempNewId = (_allRulesheetsMasterList.Any() ? _allRulesheetsMasterList.Max(r => r.SoftwareOptionId) : 0) + 2000;
                if (tempNewId < 2000) tempNewId = 2000;

                var newSoftwareOption = new SoftwareOption { SoftwareOptionId = tempNewId, PrimaryName = "New Rulesheet", CheckedDate = DateTime.Now };
                unpinnedPreviewTab.UpdateContent(newSoftwareOption);
                unpinnedPreviewTab.IsEditMode = true;
                SelectedTab = unpinnedPreviewTab;
                StatusMessage = "Creating new rulesheet...";
                return;
            }

            int tempNewIdForNewTab = (_allRulesheetsMasterList.Any() ? _allRulesheetsMasterList.Max(r => r.SoftwareOptionId) : 0) + 2000;
            if (tempNewIdForNewTab < 2000) tempNewIdForNewTab = 2000;

            var newSO = new SoftwareOption { SoftwareOptionId = tempNewIdForNewTab, PrimaryName = "New Rulesheet", CheckedDate = DateTime.Now };
            var newTab = new RulesheetDetailTabViewModel(newSO, this, AvailableControlSystems, AvailableMachineTypes)
            {
                IsPinned = false,
                IsEditMode = true
            };
            OpenTabs.Add(newTab);
            SelectedTab = newTab;
            StatusMessage = "Creating new rulesheet...";
        }

        public void SaveRulesheetFromTab(SoftwareOption savedOption)
        {
            if (savedOption == null) return;

            var existingInMaster = _allRulesheetsMasterList.FirstOrDefault(so => so.SoftwareOptionId == savedOption.SoftwareOptionId);

            bool wasNewItem = savedOption.SoftwareOptionId >= 2000 || savedOption.SoftwareOptionId == 0 || existingInMaster == null;

            if (existingInMaster != null && !wasNewItem)
            {
                int index = _allRulesheetsMasterList.IndexOf(existingInMaster);
                _allRulesheetsMasterList[index] = savedOption.Clone();
            }
            else
            {
                savedOption.SoftwareOptionId = (_allRulesheetsMasterList.Any(r => r.SoftwareOptionId < 2000) ? _allRulesheetsMasterList.Where(r => r.SoftwareOptionId < 2000).Max(r => r.SoftwareOptionId) : 0) + 1;
                _allRulesheetsMasterList.Add(savedOption.Clone());
            }
            AllRulesheetsView.Refresh();
            StatusMessage = $"Saved: {savedOption.PrimaryName}";
        }

        public void HandleDeleteRequest(RulesheetDetailTabViewModel tabVMToDelete)
        {
            if (tabVMToDelete == null || tabVMToDelete.EditableSoftwareOption == null) return;

            SoftwareOption optionToDelete = tabVMToDelete.EditableSoftwareOption;
            string deletedName = string.IsNullOrWhiteSpace(optionToDelete.PrimaryName) || optionToDelete.SoftwareOptionId >= 2000 ?
                                 "New/Unsaved Rulesheet" :
                                 optionToDelete.PrimaryName;

            if (optionToDelete.SoftwareOptionId != 0 && optionToDelete.SoftwareOptionId < 2000)
            {
                var itemInMasterList = _allRulesheetsMasterList.FirstOrDefault(so => so.SoftwareOptionId == optionToDelete.SoftwareOptionId);
                if (itemInMasterList != null)
                {
                    _allRulesheetsMasterList.Remove(itemInMasterList);
                    AllRulesheetsView.Refresh();
                    StatusMessage = $"Rulesheet '{deletedName}' deleted.";
                }
                else
                {
                    StatusMessage = $"Rulesheet '{deletedName}' (ID: {optionToDelete.SoftwareOptionId}) not found in master list for deletion (this may be okay if it was never saved).";
                }
            }
            else
            {
                StatusMessage = $"New rulesheet '{deletedName}' (with temp ID) discarded without saving to master list.";
            }

            tabVMToDelete.IsDirty = false;
            CloseTab(tabVMToDelete);
        }

        public void CloseTab(RulesheetDetailTabViewModel tabToClose)
        {
            if (tabToClose != null && OpenTabs.Contains(tabToClose))
            {
                if (tabToClose.IsDirty)
                {
                    var result = MessageBox.Show($"Save changes to '{tabToClose.TabDisplayName.TrimEnd('*')}' before closing?",
                                                 "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        tabToClose.SaveCommand.Execute(null);
                        if (tabToClose.IsDirty) return;
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                OpenTabs.Remove(tabToClose);
                StatusMessage = $"Closed tab: {tabToClose.TabDisplayName.TrimEnd('*')}";
                if (SelectedTab == tabToClose)
                {
                    SelectedTab = OpenTabs.FirstOrDefault();
                }
                if (OpenTabs.Count == 0)
                {
                    StatusMessage = "Select a rulesheet to open or click 'Add New'.";
                }
            }
        }

        public void OnTabPinnedStateChanged(RulesheetDetailTabViewModel tab)
        {
            if (tab.IsPinned)
            {
                StatusMessage = $"Tab '{tab.TabDisplayName.TrimEnd('*')}' pinned.";
            }
            else
            {
                var otherUnpinnedTabs = OpenTabs.Where(t => !t.IsPinned && t != tab).ToList();
                if (otherUnpinnedTabs.Any())
                {
                    StatusMessage = $"Tab '{tab.TabDisplayName.TrimEnd('*')}' unpinned.";
                }
                else
                {
                    StatusMessage = $"Tab '{tab.TabDisplayName.TrimEnd('*')}' unpinned. It's now the preview tab.";
                }
            }
        }

        private void LoadMasterData()
        {
            var csP300L = new ControlSystem { ControlSystemId = 1, Name = "P300L" };
            var csP300MMA = new ControlSystem { ControlSystemId = 2, Name = "P300M/MA" };
            var csOSP_Unknown = new ControlSystem { ControlSystemId = 3, Name = "OSP Unknown" };
            var csOSP_P300S_P300L = new ControlSystem { ControlSystemId = 4, Name = "OSP-P300S/P300L" };

            if (!AvailableControlSystems.Any(cs => cs.Name == csP300L.Name)) AvailableControlSystems.Add(csP300L);
            if (!AvailableControlSystems.Any(cs => cs.Name == csP300MMA.Name)) AvailableControlSystems.Add(csP300MMA);
            if (!AvailableControlSystems.Any(cs => cs.Name == csOSP_Unknown.Name)) AvailableControlSystems.Add(csOSP_Unknown);
            if (!AvailableControlSystems.Any(cs => cs.Name == csOSP_P300S_P300L.Name)) AvailableControlSystems.Add(csOSP_P300S_P300L);

            var mtLathe = new MachineType { MachineTypeId = 1, Name = "Lathe" };
            var mtMachiningCenter = new MachineType { MachineTypeId = 2, Name = "Machining Center" };
            var mtUnknown = new MachineType { MachineTypeId = 3, Name = "Unknown" };
            var mtOkuma = new MachineType { MachineTypeId = 4, Name = "Okuma Generic" };

            if (!AvailableMachineTypes.Any(mt => mt.Name == mtLathe.Name)) AvailableMachineTypes.Add(mtLathe);
            if (!AvailableMachineTypes.Any(mt => mt.Name == mtMachiningCenter.Name)) AvailableMachineTypes.Add(mtMachiningCenter);
            if (!AvailableMachineTypes.Any(mt => mt.Name == mtUnknown.Name)) AvailableMachineTypes.Add(mtUnknown);
            if (!AvailableMachineTypes.Any(mt => mt.Name == mtOkuma.Name)) AvailableMachineTypes.Add(mtOkuma);
        }

        // Made internal to be callable from RulesheetDetailTabViewModel
        internal SpecCodeDefinition GetOrCreateSpecCodeDefinition(int id, string specNo, string specBit, string desc, string cat, MachineType mt)
        {
            // If an ID is passed (e.g. from an existing definition), try to find it.
            // However, the primary check for "existing" should be the composite key.
            // The 'id' parameter here is more for when you are creating a NEW one and need to assign an ID.
            // For GetOrCreate, we primarily care about the logical key (No, Bit, MachineType).

            var existing = AllGlobalSpecCodeDefinitions.FirstOrDefault(s =>
                s.SpecCodeNo == specNo &&
                s.SpecCodeBit == specBit &&
                s.MachineTypeId == mt.MachineTypeId);

            if (existing != null)
            {
                // Optionally update description/category if new one is more complete or if they were blank
                if (string.IsNullOrWhiteSpace(existing.Description) && !string.IsNullOrWhiteSpace(desc)) existing.Description = desc;
                if (string.IsNullOrWhiteSpace(existing.Category) && !string.IsNullOrWhiteSpace(cat)) existing.Category = cat;
                return existing;
            }

            // If not found, create a new one
            int newId = (AllGlobalSpecCodeDefinitions.Any() ? AllGlobalSpecCodeDefinitions.Max(s => s.SpecCodeDefinitionId) : 0) + 1;
            var newSpecDef = new SpecCodeDefinition
            {
                SpecCodeDefinitionId = newId,
                SpecCodeNo = specNo,
                SpecCodeBit = specBit,
                Description = desc,
                Category = cat,
                MachineTypeId = mt.MachineTypeId,
                MachineType = mt // Assign the object too
            };
            AllGlobalSpecCodeDefinitions.Add(newSpecDef);
            return newSpecDef;
        }

        // Made internal for consistency, though not directly called by child in this iteration
        internal SoftwareOptionActivationRule GetOrCreateActivationRule(int id, string ruleName, string setting)
        {
            var existing = _globalActivationRules.FirstOrDefault(ar => ar.SoftwareOptionActivationRuleId == id && ar.SoftwareOptionActivationRuleId != 0);
            if (existing != null) return existing;

            int newId = (_globalActivationRules.Any() ? _globalActivationRules.Max(ar => ar.SoftwareOptionActivationRuleId) : 0) + 1;
            var newRule = new SoftwareOptionActivationRule
            {
                SoftwareOptionActivationRuleId = newId, // Assign new ID
                RuleName = ruleName,
                ActivationSetting = setting
            };
            _globalActivationRules.Add(newRule);
            return newRule;
        }

        private void LoadAllRulesheetsSampleData()
        {
            var csP300L = AvailableControlSystems.FirstOrDefault(cs => cs.Name == "P300L") ?? AvailableControlSystems.First();
            var csP300MMA = AvailableControlSystems.FirstOrDefault(cs => cs.Name == "P300M/MA") ?? AvailableControlSystems.First();
            var csOSP_P300S_L = AvailableControlSystems.FirstOrDefault(cs => cs.Name == "OSP-P300S/P300L") ?? csP300MMA;
            var mtLathe = AvailableMachineTypes.FirstOrDefault(mt => mt.Name == "Lathe") ?? AvailableMachineTypes.First();
            var mtMachiningCenter = AvailableMachineTypes.FirstOrDefault(mt => mt.Name == "Machining Center") ?? AvailableMachineTypes.First();
            var mtOkumaParsed = AvailableMachineTypes.FirstOrDefault(mt => mt.Name == "Okuma Generic") ?? mtMachiningCenter;

            int nextSoftwareOptionIdCounter = 1;
            int nextSpecDefId = 1; // This will be managed by GetOrCreate
            int nextActivationRuleId = 1; // This will be managed by GetOrCreate
            int nextSOSpecCodeId = 1;
            int nextRequirementId = 1;
            int nextOptionNumberRegistryId = 1;

            AllGlobalSpecCodeDefinitions.Clear(); // Clear before loading sample data
            _globalActivationRules.Clear(); // Clear before loading sample data
            _allRulesheetsMasterList.Clear();

            var barfeederOption = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "BAR FEEDER AND UNLOADER I/F",
                SourceFileName = "Barfeeder & Unloader Interface.csv",
                PrimaryOptionNumberDisplay = "8284",
                Notes = "This option contains same software spec, with or without Any Bus. Difference is contained in the PIOD file.",
                CheckedBy = "Dave",
                CheckedDate = new DateTime(2011, 6, 23),
                ControlSystemId = csP300L.ControlSystemId,
                ControlSystem = csP300L
            };
            var specDefBf1 = GetOrCreateSpecCodeDefinition(0, "19", "0", "BAR FEEDER I/F 1", "PLC1", mtLathe); // Pass 0 for ID, GetOrCreate will assign
            var specDefBf2 = GetOrCreateSpecCodeDefinition(0, "20", "0", "BF/PC BIT SWITCH", "PLC1", mtLathe);
            var specDefBf3 = GetOrCreateSpecCodeDefinition(0, "20", "7", "UNLOADER I/F", "PLC2", mtLathe);
            var activationRuleBf1 = GetOrCreateActivationRule(0, "Barfeeder IF Enable", "1 = ON");
            var activationRuleBf2 = GetOrCreateActivationRule(0, "BF/PC Bit Switch Enable", "1 = On");
            barfeederOption.ActivationRules.Add(activationRuleBf1); barfeederOption.ActivationRules.Add(activationRuleBf2);
            barfeederOption.OptionNumbers.Add(new OptionNumberRegistry { OptionNumberRegistryId = nextOptionNumberRegistryId++, SoftwareOptionId = barfeederOption.SoftwareOptionId, OptionNumber = "8284" });
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefBf1, SoftwareOptionActivationRuleId = activationRuleBf1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleBf1 });
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefBf2, SoftwareOptionActivationRuleId = activationRuleBf2.SoftwareOptionActivationRuleId, ActivationRule = activationRuleBf2 });
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefBf3 });
            barfeederOption.Requirements.Add(new Requirement { RequirementId = nextRequirementId++, RequirementType = "OSP_FILE_VERSION", Condition = "HigherThanVersion", OspFileName = "PLC", OspFileVersion = "LU3-410J" });
            _allRulesheetsMasterList.Add(barfeederOption);

            var toolBreakageOption = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "TOOL BREAKAGE DETECTION",
                SourceFileName = "Tool Breakage Detection.csv",
                Notes = "PIOD Upgrade May Be Necessary...",
                CheckedBy = "Dave",
                CheckedDate = new DateTime(2011, 5, 3),
                ControlSystemId = csP300MMA.ControlSystemId,
                ControlSystem = csP300MMA
            };
            var specDefTb1 = GetOrCreateSpecCodeDefinition(0, "4", "2", "Skip Function", "NC1", mtMachiningCenter);
            var specDefTb4 = GetOrCreateSpecCodeDefinition(0, "21", "6", "Tl. Comp/brk", "NC2", mtMachiningCenter);
            var specDefTb7 = GetOrCreateSpecCodeDefinition(0, "24", "2", "MSB-TLbrk. Det", "NC1", mtMachiningCenter);
            var specDefTb9 = GetOrCreateSpecCodeDefinition(0, "9", "4", "Conti.Gaug.tool**", "NC2", mtMachiningCenter);
            var activationRuleTb1 = GetOrCreateActivationRule(0, "Tool Break General Enable", "1=ON");
            var activationRuleTb2 = GetOrCreateActivationRule(0, "Tool Comp/Break General Enable", "1=ON");
            var activationRuleTb3 = GetOrCreateActivationRule(0, "Continuous Gauging Enable", "1=ON");
            toolBreakageOption.ActivationRules.Add(activationRuleTb1); toolBreakageOption.ActivationRules.Add(activationRuleTb2); toolBreakageOption.ActivationRules.Add(activationRuleTb3);
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefTb9, SoftwareOptionActivationRuleId = activationRuleTb3.SoftwareOptionActivationRuleId, ActivationRule = activationRuleTb3 });
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefTb4, SoftwareOptionActivationRuleId = activationRuleTb2.SoftwareOptionActivationRuleId, ActivationRule = activationRuleTb2 });
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefTb7, SoftwareOptionActivationRuleId = activationRuleTb1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleTb1 });
            toolBreakageOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefTb1 });
            _allRulesheetsMasterList.Add(toolBreakageOption);

            var hyperSurfaceOption = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "Hyper-Surface Function - 3 Axis",
                SourceFileName = "Hyper-Surface Function - 3 Axis.csv",
                ControlSystemId = csP300MMA.ControlSystemId,
                ControlSystem = csP300MMA,
                CheckedBy = "Auto",
                CheckedDate = DateTime.Now
            };
            var specDefHs1 = GetOrCreateSpecCodeDefinition(0, "1", "0", "Hyper-Surface Function (G341) (3axis)", "NC1", mtMachiningCenter);
            var activationRuleHs1 = GetOrCreateActivationRule(0, "Hyper-Surface Enable", "1=ON");
            hyperSurfaceOption.ActivationRules.Add(activationRuleHs1);
            hyperSurfaceOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefHs1, SoftwareOptionActivationRuleId = activationRuleHs1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleHs1, SpecificInterpretation = "1=ON" });
            _allRulesheetsMasterList.Add(hyperSurfaceOption);

            var dncT3Option = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "DNC-T3",
                SourceFileName = "DNC-T3.csv",
                ControlSystemId = csOSP_P300S_L.ControlSystemId,
                ControlSystem = csOSP_P300S_L,
                CheckedBy = "Auto",
                CheckedDate = DateTime.Now
            };
            var specDefDnc1 = GetOrCreateSpecCodeDefinition(0, "32", "0", "AIRBAG SYSTEM DNC-T1 -> T3 (Orig No 100)", "PLC1", mtOkumaParsed);
            var specDefDnc2 = GetOrCreateSpecCodeDefinition(0, "32", "6", "DNC-T3 (Orig No 100)", "PLC2", mtOkumaParsed);
            var activationRuleDnc1 = GetOrCreateActivationRule(0, "DNC Airbag Enable", "AIRBAG SYSTEM DNC-T1 -> T3");
            var activationRuleDnc2 = GetOrCreateActivationRule(0, "DNC-T3 Enable", "1=ON");
            dncT3Option.ActivationRules.Add(activationRuleDnc1); dncT3Option.ActivationRules.Add(activationRuleDnc2);
            dncT3Option.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefDnc1, SoftwareOptionActivationRuleId = activationRuleDnc1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleDnc1, SpecificInterpretation = "DNC-T1->T3" });
            dncT3Option.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefDnc2, SoftwareOptionActivationRuleId = activationRuleDnc2.SoftwareOptionActivationRuleId, ActivationRule = activationRuleDnc2, SpecificInterpretation = "1=ON" });
            _allRulesheetsMasterList.Add(dncT3Option);

            var steadyRestSwitchOption = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "Steady Rest Enable-Disable Switch",
                SourceFileName = "Steady Rest Enable-Disable Switch.csv",
                ControlSystemId = csP300L.ControlSystemId,
                ControlSystem = csP300L,
                CheckedBy = "Auto",
                CheckedDate = DateTime.Now
            };
            var specDefSr1 = GetOrCreateSpecCodeDefinition(0, "25", "5", "Steady Rest Enable Switch (Orig No 70)", "PLC1", mtLathe);
            var activationRuleSr1 = GetOrCreateActivationRule(0, "SR Switch Enable", "1=ON");
            steadyRestSwitchOption.ActivationRules.Add(activationRuleSr1);
            steadyRestSwitchOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefSr1, SoftwareOptionActivationRuleId = activationRuleSr1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleSr1 });
            _allRulesheetsMasterList.Add(steadyRestSwitchOption);

            var dynamicFixtureOption = new SoftwareOption
            {
                SoftwareOptionId = nextSoftwareOptionIdCounter++,
                PrimaryName = "Dynamic Fixture Tracking",
                SourceFileName = "Dynamic Fixture Tracking.csv",
                ControlSystemId = csP300MMA.ControlSystemId,
                ControlSystem = csP300MMA,
                CheckedBy = "Auto",
                CheckedDate = DateTime.Now
            };
            var specDefDf1 = GetOrCreateSpecCodeDefinition(0, "30", "2", "Dynamic Fixture Tracking Active (Orig No 80)", "NC1", mtMachiningCenter);
            var activationRuleDf1 = GetOrCreateActivationRule(0, "DFT Active", "1=ON");
            dynamicFixtureOption.ActivationRules.Add(activationRuleDf1);
            dynamicFixtureOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = nextSOSpecCodeId++, SpecCodeDefinition = specDefDf1, SoftwareOptionActivationRuleId = activationRuleDf1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleDf1 });
            _allRulesheetsMasterList.Add(dynamicFixtureOption);

            StatusMessage = $"{_allRulesheetsMasterList.Count} rulesheets loaded.";
        }
    }
}
