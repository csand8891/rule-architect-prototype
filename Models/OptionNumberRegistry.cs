namespace RuleArchitectPrototype.Models
{
    public class OptionNumberRegistry : BaseModel // Assuming BaseModel handles INotifyPropertyChanged
    {
        private int _optionNumberRegistryId;
        public int OptionNumberRegistryId
        {
            get => _optionNumberRegistryId;
            set => SetField(ref _optionNumberRegistryId, value);
        }

        private string _optionNumber;
        public string OptionNumber
        {
            get => _optionNumber;
            set => SetField(ref _optionNumber, value);
        }

        private int _softwareOptionId; // Backing field for the foreign key
        public int SoftwareOptionId    // Foreign key property
        {
            get => _softwareOptionId;
            set => SetField(ref _softwareOptionId, value);
        }

        // Optional: Navigation property back to the SoftwareOption.
        // This would typically be populated by your data access layer or ORM.
        // For static sample data, setting the SoftwareOptionId is sufficient.
        // private SoftwareOption _softwareOption;
        // public SoftwareOption SoftwareOption 
        // {
        //     get => _softwareOption;
        //     set => SetField(ref _softwareOption, value);
        // }
    }
}