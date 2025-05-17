using RuleArchitectPrototype.ViewModels; // Ensure this is the correct namespace for your ViewModel
using System;
using System.Windows;

namespace RuleArchitectPrototype.Views // Ensure this is the correct namespace for your View
{
    public partial class AddEditSpecCodeDialogView : Window
    {
        public AddEditSpecCodeDialogView()
        {
            InitializeComponent();
            // It's often better to subscribe to events after the DataContext is set, 
            // or ensure DataContext is set before Loaded if subscribing in Loaded.
            // Subscribing in constructor if DataContext is set immediately by caller is also an option.
            // For robustness, let's ensure DataContext is available.
            this.DataContextChanged += AddEditSpecCodeDialogView_DataContextChanged;
        }

        private void AddEditSpecCodeDialogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is AddEditSpecCodeDialogViewModel oldVm)
            {
                oldVm.RequestCloseDialog -= ViewModel_RequestCloseDialog;
            }
            if (e.NewValue is AddEditSpecCodeDialogViewModel newVm)
            {
                newVm.RequestCloseDialog += ViewModel_RequestCloseDialog;
            }
        }

        private void ViewModel_RequestCloseDialog(bool? dialogResult)
        {
            // This event is raised by the ViewModel when it wants to close the dialog.
            // Set the DialogResult of the window. This will close the window if it was shown with ShowDialog().
            try
            {
                // Check if the window is still loaded/visible to avoid issues if called multiple times
                // or after the window has started closing for other reasons.
                if (this.IsLoaded && this.IsVisible)
                {
                    this.DialogResult = dialogResult;
                    // No explicit this.Close() needed here if DialogResult is set on a modal window.
                    // If DialogResult is set, and the window was shown with ShowDialog(), it will close.
                }
            }
            catch (InvalidOperationException ex)
            {
                // This exception can occur if DialogResult is set multiple times,
                // or if the window was not shown as a modal dialog (i.e., using Show() instead of ShowDialog()).
                // For a dialog, it should always be shown with ShowDialog().
                MessageBox.Show($"Error closing dialog: {ex.Message}\nEnsure the dialog was shown using ShowDialog().", "Dialog Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // As a fallback if not modal or DialogResult fails, try to close.
                if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this) && this.IsVisible)
                {
                    this.Close();
                }
            }
        }

        // Clean up event subscription when the window is unloaded
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AddEditSpecCodeDialogViewModel vm)
            {
                vm.RequestCloseDialog -= ViewModel_RequestCloseDialog;
            }
            this.DataContextChanged -= AddEditSpecCodeDialogView_DataContextChanged;
        }
    }
}
