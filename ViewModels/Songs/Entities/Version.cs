namespace App1.ViewModels
{
    public class Version : Bindable
    {
        public Version()
        {
            number_ = string.Empty;
            description_ = string.Empty;
            author_ = string.Empty;
            date_ = string.Empty;
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
                var numbers = value.Split('.').ToList();
                string compoVersion = numbers[0];
                string mixVersion = numbers[1];
                string versionNumber = string.Empty;
                if (mixVersion == "0")
                {
                    versionNumber = "Compo v" + compoVersion;
                }
                else
                {
                    versionNumber = "Compo v" + compoVersion + " / Mix v" + mixVersion;
                }
                 
                SetProperty(ref number_, versionNumber);
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

        private string date_;
        public string Date
        {
            get
            {
                return date_;
            }
            set
            {
                SetProperty(ref date_, value);
            }
        }

        public override bool Equals(object? obj)
        {
            var version = obj as Version;
            if (version == null)
                return false;
            if (this.Number != version.Number || 
                this.Author != version.Author ||
                this.Description != version.Description ||
                this.Date != version.Date)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Number, this.Author, this.Description, this.Date);
        }
    }
}
