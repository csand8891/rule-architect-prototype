using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using RuleArchitectPrototype.Models;
using RuleArchitectPrototype.Commands;
using System.Windows; // For MessageBox (consider a proper dialog service in a real app)

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

        public ObservableCollection<ControlSystem> AvailableControlSystems { get; }
        public ObservableCollection<MachineType> AvailableMachineTypes { get; }


        public TabbedRulesheetViewModel()
        {
            _allRulesheetsMasterList = new ObservableCollection<SoftwareOption>();
            OpenTabs = new ObservableCollection<RulesheetDetailTabViewModel>();
            SelectedTab = null;

            AvailableControlSystems = new ObservableCollection<ControlSystem>();
            AvailableMachineTypes = new ObservableCollection<MachineType>();

            LoadMasterData();
            LoadAllRulesheetsSampleData();

            AllRulesheetsView = CollectionViewSource.GetDefaultView(_allRulesheetsMasterList);
            AllRulesheetsView.Filter = FilterAllRulesheetsPredicate;

            OpenRulesheetInTabCommand = new RelayCommand(
                param => OpenRulesheetInTab(param as SoftwareOption),
                param => CanOpenRulesheetInTab(param as SoftwareOption)
            );
            AddNewRulesheetTabCommand = new RelayCommand(AddNewRulesheetTab);

            StatusMessage = "Select a rulesheet to open or click 'Add New'.";
        }

        private bool FilterAllRulesheetsPredicate(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchAllText)) return true;
            if (item is SoftwareOption option)
            {
                return (option.PrimaryName?.IndexOf(SearchAllText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                       (option.PrimaryOptionNumberDisplay?.IndexOf(SearchAllText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                       (option.ControlSystem?.Name?.IndexOf(SearchAllText, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return false;
        }

        private bool CanOpenRulesheetInTab(SoftwareOption option) => option != null;

        private void OpenRulesheetInTab(SoftwareOption optionToOpen)
        {
            if (optionToOpen == null) return;

            // 1. Check if a pinned tab for this option already exists
            var existingPinnedTab = OpenTabs.FirstOrDefault(tab => tab.IsPinned && tab.SoftwareOptionId == optionToOpen.SoftwareOptionId);
            if (existingPinnedTab != null)
            {
                SelectedTab = existingPinnedTab;
                StatusMessage = $"Activated pinned tab for: {optionToOpen.PrimaryName}";
                return;
            }

            // 2. Find an unpinned tab to reuse (the "preview" tab)
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
                { // Also check if it's already showing this one and not dirty
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
                var newSoftwareOption = new SoftwareOption { SoftwareOptionId = 0, PrimaryName = "New Rulesheet", CheckedDate = DateTime.Now };
                unpinnedPreviewTab.UpdateContent(newSoftwareOption);
                unpinnedPreviewTab.IsEditMode = true;
                SelectedTab = unpinnedPreviewTab;
                StatusMessage = "Creating new rulesheet...";
                return;
            }

            var newSO = new SoftwareOption { SoftwareOptionId = 0, PrimaryName = "New Rulesheet", CheckedDate = DateTime.Now };
            var newTab = new RulesheetDetailTabViewModel(newSO, this, AvailableControlSystems, AvailableMachineTypes)
            {
                IsPinned = false,
                IsEditMode = true
            };
            OpenTabs.Add(newTab);
            SelectedTab = newTab;
            StatusMessage = "Creating new rulesheet...";
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
                // If unpinning, check if there's another unpinned tab. If so, this one might need to close or change.
                // For now, just a status update. More complex logic could enforce only one "preview" tab.
                var otherUnpinnedTabs = OpenTabs.Where(t => !t.IsPinned && t != tab).ToList();
                if (otherUnpinnedTabs.Any())
                {
                    // Potentially close this newly unpinned tab if another preview tab exists
                    // Or, convert the other preview tab to pinned.
                    // For simplicity now, we allow multiple unpinned tabs.
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
            AvailableControlSystems.Add(csP300L);
            AvailableControlSystems.Add(csP300MMA);

            var mtLathe = new MachineType { MachineTypeId = 1, Name = "Lathe" };
            var mtMachiningCenter = new MachineType { MachineTypeId = 2, Name = "Machining Center" };
            AvailableMachineTypes.Add(mtLathe);
            AvailableMachineTypes.Add(mtMachiningCenter);
        }

        private void LoadAllRulesheetsSampleData()
        {
            var csP300L = AvailableControlSystems.First(cs => cs.ControlSystemId == 1);
            var csP300MMA = AvailableControlSystems.First(cs => cs.ControlSystemId == 2);
            var mtLathe = AvailableMachineTypes.First(mt => mt.MachineTypeId == 1);
            var mtMachiningCenter = AvailableMachineTypes.First(mt => mt.MachineTypeId == 2);

            var barfeederOption = new SoftwareOption
            {
                SoftwareOptionId = 1,
                PrimaryName = "BAR FEEDER AND UNLOADER I/F",
                SourceFileName = "Barfeeder & Unloader Interface.csv",
                PrimaryOptionNumberDisplay = "8284",
                Notes = "This option contains same software spec, with or without Any Bus. Difference is contained in the PIOD file.",
                CheckedBy = "Dave",
                CheckedDate = new DateTime(2011, 6, 23),
                ControlSystemId = csP300L.ControlSystemId,
                ControlSystem = csP300L
            };
            barfeederOption.OptionNumbers.Add(new OptionNumberRegistry { OptionNumberRegistryId = 1, OptionNumber = "8284" });
            var specDefBf1 = new SpecCodeDefinition { SpecCodeDefinitionId = 1, SpecCodeNo = "19", SpecCodeBit = "0", Description = "BAR FEEDER I/F 1", Category = "PLC", MachineTypeId = mtLathe.MachineTypeId, MachineType = mtLathe };
            var specDefBf2 = new SpecCodeDefinition { SpecCodeDefinitionId = 2, SpecCodeNo = "20", SpecCodeBit = "0", Description = "BF/PC BIT SWITCH", Category = "PLC", MachineTypeId = mtLathe.MachineTypeId, MachineType = mtLathe };
            var specDefBf3 = new SpecCodeDefinition { SpecCodeDefinitionId = 3, SpecCodeNo = "20", SpecCodeBit = "7", Description = "UNLOADER I/F", Category = "PLC", MachineTypeId = mtLathe.MachineTypeId, MachineType = mtLathe };
            var activationRuleBf1 = new SoftwareOptionActivationRule { SoftwareOptionActivationRuleId = 4, RuleName = "Barfeeder IF Enable", ActivationSetting = "1 = ON" };
            var activationRuleBf2 = new SoftwareOptionActivationRule { SoftwareOptionActivationRuleId = 5, RuleName = "BF/PC Bit Switch Enable", ActivationSetting = "1 = On" };
            barfeederOption.ActivationRules.Add(activationRuleBf1);
            barfeederOption.ActivationRules.Add(activationRuleBf2);
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 1, SpecCodeDefinitionId = specDefBf1.SpecCodeDefinitionId, SpecCodeDefinition = specDefBf1, SoftwareOptionActivationRuleId = activationRuleBf1.SoftwareOptionActivationRuleId, ActivationRule = activationRuleBf1 });
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 2, SpecCodeDefinitionId = specDefBf2.SpecCodeDefinitionId, SpecCodeDefinition = specDefBf2, SoftwareOptionActivationRuleId = activationRuleBf2.SoftwareOptionActivationRuleId, ActivationRule = activationRuleBf2 });
            barfeederOption.SpecificationCodes.Add(new SoftwareOptionSpecificationCode { SoftwareOptionSpecificationCodeId = 3, SpecCodeDefinitionId = specDefBf3.SpecCodeDefinitionId, SpecCodeDefinition = specDefBf3 });
            barfeederOption.Requirements.Add(new Requirement { RequirementId = 1, RequirementType = "OSP_FILE_VERSION", Condition = "HigherThanVersion", OspFileName = "PLC", OspFileVersion = "LU3-410J" });
            _allRulesheetsMasterList.Add(barfeederOption);

            var toolBreakageOption = new SoftwareOption
            {
                SoftwareOptionId = 2,
                PrimaryName = "TOOL BREAKAGE DETECTION",
                SourceFileName = "Tool Breakage Detection.csv",
                PrimaryOptionNumberDisplay = null,
                Notes = "PIOD Upgrade May Be Necessary. (*) Comments MSB Ref TL 150 is for CAT40 only. (**) Continuous Gauging Requirements. Related rule/option: **8743-9, Tool Breakage Adv/Ret Confirmation(( TouchSensMoves -> force on PLC(8,0) )). ***SPECIAL RULE FOR TBD on DOUBLE COLUMNS*** see MCR-A5CII P219912, additional MSB files MNCUJ85-EL010-1C (cross rail sensor), MNCUJ85-EL040-0C (table sensor). Revision (1/8/2020 DLN): Continuous Gauging Added. Revision (10/28/16 stb): added note for :8743-9.",
                CheckedBy = "Dave",
                CheckedDate = new DateTime(2011, 5, 3),
                ControlSystemId = csP300MMA.ControlSystemId,
                ControlSystem = csP300MMA
            };
            var specDefTb1 = new SpecCodeDefinition { SpecCodeDefinitionId = 4, SpecCodeNo = "4", SpecCodeBit = "2", Description = "Skip Function", Category = "NC", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };
            var specDefTb4 = new SpecCodeDefinition { SpecCodeDefinitionId = 7, SpecCodeNo = "21", SpecCodeBit = "6", Description = "Tl. Comp/brk", Category = "NC", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };
            var specDefTb7 = new SpecCodeDefinition { SpecCodeDefinitionId = 10, SpecCodeNo = "24", SpecCodeBit = "2", Description = "MSB-TLbrk. Det", Category = "NC", MachineTypeId = mtMachiningCenter.MachineTypeId, MachineType = mtMachiningCenter };
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
            _allRulesheetsMasterList.Add(toolBreakageOption);

            _allRulesheetsMasterList.Add(new SoftwareOption { SoftwareOptionId = 3, PrimaryName = "Another Lathe Option", ControlSystem = csP300L, PrimaryOptionNumberDisplay = "1001" });
            _allRulesheetsMasterList.Add(new SoftwareOption { SoftwareOptionId = 4, PrimaryName = "Machining Center Feature X", ControlSystem = csP300MMA, PrimaryOptionNumberDisplay = "2002" });
            _allRulesheetsMasterList.Add(new SoftwareOption { SoftwareOptionId = 5, PrimaryName = "General Purpose Utility", ControlSystem = csP300L, PrimaryOptionNumberDisplay = "3003" });
        }
    }
}
