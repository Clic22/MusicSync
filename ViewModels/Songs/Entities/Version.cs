namespace App1.ViewModels
{
    public class Version : Bindable
    {
        public Version()
        {
            _number = string.Empty;
            _description = string.Empty;
            _author = string.Empty;
            _date = string.Empty;
        }

        private string _number;
        public string Number
        {
            get
            {
                return _number;
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
                 
                SetProperty(ref _number, versionNumber);
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                SetProperty(ref _description, value);
            }
        }

        private string _author;
        public string Author
        {
            get
            {
                return _author;
            }
            set
            {
                SetProperty(ref _author, value);
            }
        }

        private string _date;
        public string Date
        {
            get
            {
                return _date;
            }
            set
            {
                SetProperty(ref _date, value);
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
