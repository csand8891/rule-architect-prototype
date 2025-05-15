using System;
using System.Collections.ObjectModel;
using System.ComponentModel; // Required for PropertyChangedEventArgs for manual IsDirty
using System.Windows.Input;
using RuleArchitectPrototype.Models;
using RuleArchitectPrototype.Commands;
using System.Linq;

namespace RuleArchitectPrototype.ViewModels
{
    public class RulesheetDetailTabViewModel : BaseModel
    {
        private SoftwareOption _originalSoftwareOption; // Represents the state when loaded or last saved
        private SoftwareOption _editableSoftwareOption;
        public SoftwareOption EditableSoftwareOption
        {
            get => _editableSoftwareOption;
            set
            {
                if (_editableSoftwareOption != null)
                {
                    _editableSoftwareOption.PropertyChanged -= EditableSoftwareOption_PropertyChanged;
                }
                if (SetField(ref _editableSoftwareOption, value))
                {
                    IsDirty = false; // Reset dirty flag when the whole object is swapped
                    if (_editableSoftwareOption != null)
                    {
                        _editableSoftwareOption.PropertyChanged += EditableSoftwareOption_PropertyChanged;
                    }
                }
            }
        }

        private void EditableSoftwareOption_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsEditMode) // Only mark as dirty if in edit mode
            {
                IsDirty = true;
            }
        }

        private string _tabDisplayName;
        public string TabDisplayName
        {
            get => _tabDisplayName;
            set => SetField(ref _tabDisplayName, value);
        }

        private string _iconKind;
        public string IconKind
        {
            get => _iconKind;
            set => SetField(ref _iconKind, value);
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
                    (CancelEditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    UpdateIconAndHeader();
                    if (!_isEditMode && IsDirty) // If exiting edit mode and was dirty (e.g. after save/cancel)
                    {
                        // IsDirty should be reset by Save/Cancel explicitly
                    }
                }
            }
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

        private bool _isDirty;
        public bool IsDirty
        {
            get => _isDirty;
            private set // Can only be set internally or by property changes
            {
                if (SetField(ref _isDirty, value))
                {
                    UpdateIconAndHeader();
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged(); // Save should be enabled if dirty
                }
            }
        }

        private TabbedRulesheetViewModel _parentViewModel;

        public ObservableCollection<ControlSystem> AvailableControlSystems { get; }
        public ObservableCollection<MachineType> AvailableMachineTypes { get; }

        public ICommand CloseTabCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand PinTabCommand { get; }

        public int SoftwareOptionId => _originalSoftwareOption.SoftwareOptionId;
        // This helps identify if the tab was for a "New Rulesheet" that hasn't been saved yet.
        public bool WasOriginallyNew => _originalSoftwareOption.SoftwareOptionId == 0;


        public RulesheetDetailTabViewModel(SoftwareOption softwareOption, TabbedRulesheetViewModel parentViewModel,
                                           ObservableCollection<ControlSystem> availableControlSystems,
                                           ObservableCollection<MachineType> availableMachineTypes)
        {
            _originalSoftwareOption = softwareOption; // This is the definitive state (either from master list or a new placeholder)
            EditableSoftwareOption = softwareOption.Clone(); // Initial editable copy

            _parentViewModel = parentViewModel;
            AvailableControlSystems = availableControlSystems;
            AvailableMachineTypes = availableMachineTypes;

            if (EditableSoftwareOption.ControlSystemId.HasValue && EditableSoftwareOption.ControlSystem == null)
            {
                EditableSoftwareOption.ControlSystem = AvailableControlSystems.FirstOrDefault(cs => cs.ControlSystemId == EditableSoftwareOption.ControlSystemId.Value);
            }

            // New items start unpinned and in edit mode. Existing items start pinned and in view mode.
            IsPinned = (softwareOption.SoftwareOptionId != 0);
            IsEditMode = (softwareOption.SoftwareOptionId == 0);
            IsDirty = false; // Starts clean

            UpdateIconAndHeader(); // Initial setup

            CloseTabCommand = new RelayCommand(() => _parentViewModel.CloseTab(this));
            EditCommand = new RelayCommand(PerformEdit, CanPerformEdit);
            SaveCommand = new RelayCommand(PerformSave, CanPerformSave);
            CancelEditCommand = new RelayCommand(PerformCancelEdit, CanPerformCancelEdit);
            PinTabCommand = new RelayCommand(PerformPinToggle);
        }

        public void UpdateContent(SoftwareOption newSoftwareOption)
        {
            // Detach old event handler
            if (EditableSoftwareOption != null)
            {
                EditableSoftwareOption.PropertyChanged -= EditableSoftwareOption_PropertyChanged;
            }

            _originalSoftwareOption = newSoftwareOption; // Update the base state
            EditableSoftwareOption = newSoftwareOption.Clone(); // Set new editable content

            IsPinned = false; // When content is replaced, it's a new preview, so unpin
            IsEditMode = false; // Start in view mode for existing items
            IsDirty = false;    // Reset dirty state
            UpdateIconAndHeader();
            OnPropertyChanged(nameof(EditableSoftwareOption)); // Notify UI that the whole object changed
        }

        private void UpdateIconAndHeader()
        {
            string baseName = EditableSoftwareOption.PrimaryName ?? "New Rulesheet";
            TabDisplayName = IsDirty ? baseName + "*" : baseName;

            if (IsEditMode)
            {
                IconKind = WasOriginallyNew && !IsPinned ? "FilePlusOutline" : "FileEditOutline";
            }
            else // View Mode
            {
                IconKind = IsPinned ? "FileDocument" : "FileFindOutline"; // Different icon for unpinned preview
            }
        }

        private bool CanPerformEdit() => !IsEditMode && (_originalSoftwareOption.SoftwareOptionId != 0 || IsPinned); // Can edit saved items or pinned new items
        private void PerformEdit()
        {
            IsEditMode = true;
        }

        private bool CanPerformSave() => IsEditMode && IsDirty;
        private void PerformSave()
        {
            if (string.IsNullOrWhiteSpace(EditableSoftwareOption.PrimaryName))
            {
                _parentViewModel.StatusMessage = "Error: Primary Name cannot be empty.";
                // Consider a more user-friendly error display (e.g., dialog)
                return;
            }

            var masterList = _parentViewModel._allRulesheetsMasterList;
            bool isSavingNewItem = (_originalSoftwareOption.SoftwareOptionId == 0);

            if (isSavingNewItem)
            {
                // Assign a new ID if it's truly new (SoftwareOptionId on Editable might have been set if loaded from a template)
                EditableSoftwareOption.SoftwareOptionId = (masterList.Any() ? masterList.Max(r => r.SoftwareOptionId) : 0) + 1;
                masterList.Add(EditableSoftwareOption.Clone()); // Add a clone to the master list
                _parentViewModel.StatusMessage = $"New rulesheet '{EditableSoftwareOption.PrimaryName}' added.";
            }
            else // Updating an existing item
            {
                var masterItem = masterList.FirstOrDefault(so => so.SoftwareOptionId == EditableSoftwareOption.SoftwareOptionId);
                if (masterItem != null)
                {
                    int index = masterList.IndexOf(masterItem);
                    masterList[index] = EditableSoftwareOption.Clone(); // Replace with a clone of the edited version
                    _parentViewModel.StatusMessage = $"Saved changes to: {EditableSoftwareOption.PrimaryName}";
                }
                else
                {
                    // This case should ideally not happen if logic is correct (trying to save an "existing" item that's not in master list)
                    // Potentially add it as new if it's missing, though this implies an issue elsewhere.
                    masterList.Add(EditableSoftwareOption.Clone());
                    _parentViewModel.StatusMessage = $"Saved (and added missing): {EditableSoftwareOption.PrimaryName}";
                }
            }

            _originalSoftwareOption = EditableSoftwareOption.Clone(); // Update the tab's "original" to the saved state
            IsDirty = false;
            IsEditMode = false;
            IsPinned = true; // Automatically pin on save
            // UpdateIconAndHeader() is called by IsEditMode and IsPinned setters
            _parentViewModel.AllRulesheetsView.Refresh(); // Refresh the main list view
        }

        private bool CanPerformCancelEdit() => IsEditMode;
        private void PerformCancelEdit()
        {
            // If it was a new item (based on original ID) AND it's not pinned, closing is more appropriate
            if (WasOriginallyNew && !IsPinned)
            {
                _parentViewModel.CloseTab(this);
                _parentViewModel.StatusMessage = "Add new rulesheet cancelled.";
            }
            else // Revert existing item or a new item that got pinned before saving
            {
                // Detach old event handler before replacing EditableSoftwareOption
                if (EditableSoftwareOption != null)
                {
                    EditableSoftwareOption.PropertyChanged -= EditableSoftwareOption_PropertyChanged;
                }
                EditableSoftwareOption = _originalSoftwareOption.Clone(); // Revert to last saved/loaded state
                if (EditableSoftwareOption != null) // Re-attach event handler
                {
                    EditableSoftwareOption.PropertyChanged += EditableSoftwareOption_PropertyChanged;
                }
                IsDirty = false;
                IsEditMode = false;
                // UpdateIconAndHeader() is called by IsEditMode setter
                _parentViewModel.StatusMessage = $"Cancelled edits for: {TabDisplayName.TrimEnd('*')}";
            }
        }

        private void PerformPinToggle()
        {
            IsPinned = !IsPinned;
        }
    }
}
