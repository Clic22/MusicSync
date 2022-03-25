namespace App1.ViewModels
{
    public class Version : Bindable
    {
        public Version()
        {
            number_ = string.Empty;
            description_ = string.Empty;
            author_ = string.Empty;
        }

        private string number_;
        public string Number
        {
            get
            {
                return number_;
            }
            set
            {
                SetProperty(ref number_, value);
            }
        }

        private string description_;
        public string Description
        {
            get
            {
                return description_;
            }
            set
            {
                SetProperty(ref description_, value);
            }
        }

        private string author_;
        public string Author
        {
            get
            {
                return author_;
            }
            set
            {
                SetProperty(ref author_, value);
            }
        }
    }
}
