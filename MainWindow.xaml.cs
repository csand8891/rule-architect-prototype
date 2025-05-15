using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RuleArchitectPrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var errorBrush = Application.Current.TryFindResource("MaterialDesignErrorBrush");
            if (errorBrush != null)
            {
                if (errorBrush is System.Windows.Media.SolidColorBrush scb)
                {
                    System.Diagnostics.Debug.WriteLine($"RuleArchitectPrototype: MaterialDesignErrorBrush found. Color: {scb.Color}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"RuleArchitectPrototype: MaterialDesignErrorBrush found, but it's not a SolidColorBrush. Type: {errorBrush.GetType()}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("RuleArchitectPrototype: MaterialDesignErrorBrush was NOT FOUND in application resources.");
            }

            var primaryBrush = Application.Current.TryFindResource("PrimaryHueMidBrush");
            if (primaryBrush != null && primaryBrush is System.Windows.Media.SolidColorBrush pscb)
            {
                System.Diagnostics.Debug.WriteLine($"RuleArchitectPrototype: PrimaryHueMidBrush found. Color: {pscb.Color}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("RuleArchitectPrototype: PrimaryHueMidBrush was NOT FOUND.");
            }

            var validationErrorBrush = Application.Current.TryFindResource("MaterialDesignValidationErrorBrush");
            if (validationErrorBrush != null)
            {
                if (validationErrorBrush is System.Windows.Media.SolidColorBrush scb)
                {
                    System.Diagnostics.Debug.WriteLine($"RuleArchitectPrototype: MaterialDesignValidationErrorBrush found. Color: {scb.Color}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"RuleArchitectPrototype: MaterialDesignValidationErrorBrush found, but it's not a SolidColorBrush. Type: {validationErrorBrush.GetType()}. ToString: {validationErrorBrush.ToString()}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("RuleArchitectPrototype: MaterialDesignValidationErrorBrush was NOT FOUND in application resources.");
            }
        }
    }
}
