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

        public override bool Equals(object? obj)
        {
            var song = obj as Version;
            if (song == null)
                return false;
            if (this.Number != song.Number && 
                this.Author != song.Author &&
                this.Description != song.Description)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Number, this.Author, this.Description);
        }
    }
}
