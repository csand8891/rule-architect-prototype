using RuleArchitectPrototype.ViewModels;
using System;
using System.Windows;

namespace RuleArchitectPrototype.Views
{
    public partial class AddEditSpecCodeDialogView : Window
    {
        public AddEditSpecCodeDialogView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddEditSpecCodeDialogViewModel vm)
            {
                vm.RequestCloseDialog += (dialogResult) =>
                {
                    try
                    {
                        // This check is important to prevent issues if already closing or not shown modally.
                        if (this.IsVisible && PresentationSource.FromVisual(this) != null)
                        {
                            this.DialogResult = dialogResult;
                            // No explicit this.Close() here if DialogResult is set on a modal window.
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // This can happen if DialogResult is set multiple times 
                        // or if the window is not being shown as a dialog.
                        // If not modal (should not be the case for this dialog), then just close.
                        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                        {
                            // For a non-modal window, or if DialogResult fails, explicitly close.
                            // However, this dialog is intended to be modal.
                            this.Close();
                        }
                    }
                };
            }
        }
    }
}
