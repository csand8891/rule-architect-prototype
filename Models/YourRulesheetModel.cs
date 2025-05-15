using System.ComponentModel;
using System.Runtime.CompilerServices; // For CallerMemberName

// Suggested Namespace (adjust if your folder structure/project name is different)
namespace RuleArchitectPrototype.Models
{
    public class YourRulesheetModel : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); } // CallerMemberName will infer "Name"
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        // Add any other properties your rulesheet needs for the prototype here
        // For example:
        // private DateTime _lastModified;
        // public DateTime LastModified
        // {
        //     get => _lastModified;
        //     set { _lastModified = value; OnPropertyChanged(); }
        // }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // This Clone method is used by the ViewModel to create a copy for editing,
        // so you can cancel edits without affecting the original object in the list.
        public YourRulesheetModel Clone()
        {
            return (YourRulesheetModel)this.MemberwiseClone();
        }
    }
}