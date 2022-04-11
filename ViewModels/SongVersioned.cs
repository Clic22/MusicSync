using System.Collections.ObjectModel;

namespace App1.ViewModels
{
    public class SongVersioned : Bindable
    {
        public SongVersioned(string title)
        {
            title_ = title;
            status_ = string.Empty;
            isUpdatingSong_ = false;
            isUploadingSong_ = false;
            isLoading_ = false;
            currentVersion_ = new Version();
            Versions = new ObservableCollection<Version>();
        }

        private readonly string title_;
        public string Title
        {
            get
            {
                return title_;
            }
        }

        private string status_;
        public string Status
        {
            get
            {
                return status_;
            }
            set
            {
                SetProperty(ref status_, value);
            }
        }

        private Version currentVersion_;
        public Version CurrentVersion
        {
            get
            {
                return currentVersion_;
            }
            set
            {
                SetProperty(ref currentVersion_, value);
            }
        }

        public ObservableCollection<Version> Versions;

        private bool isRefreshingSong_;
        public bool IsRefreshingSong
        {
            get
            {
                return isRefreshingSong_;
            }
            set
            {
                if (value)
                {
                    Status = "Refreshing...";
                }
                else
                {
                    Status = string.Empty;
                }
                SetProperty(ref isRefreshingSong_, value);
                IsLoading = value;
            }
        }

        private bool isUpdatingSong_;
        public bool IsUpdatingSong
        {
            get
            {
                return isUpdatingSong_;
            }
            set
            {
                if (value)
                {
                    Status = "Updating...";
                }
                else
                {
                    Status = string.Empty;
                }
                SetProperty(ref isUpdatingSong_, value);
                IsLoading = value;
            }
        }

        private bool isUploadingSong_;
        public bool IsUploadingSong
        {
            get
            {
                return isUploadingSong_;
            }
            set
            {
                if (value)
                {
                    Status = "Uploading...";
                }
                else
                {
                    Status = string.Empty;
                }
                SetProperty(ref isUploadingSong_, value);
                IsLoading = value;
            }
        }

        private bool isRevertingSong_;
        public bool IsRevertingSong
        {
            get
            {
                return isRevertingSong_;
            }
            set
            {
                if (value)
                {
                    Status = "Reverting...";
                }
                else
                {
                    Status = string.Empty;
                }
                SetProperty(ref isRevertingSong_, value);
                IsLoading = value;
            }
        }

        private bool isOpeningSong_;
        public bool IsOpeningSong
        {
            get
            {
                return isOpeningSong_;
            }
            set
            {
                if (value)
                {
                    Status = "Opening...";
                }
                else
                {
                    Status = string.Empty;
                }
                SetProperty(ref isOpeningSong_, value);
                IsLoading = value;
            }
        }

        private bool isLoading_;
        public bool IsLoading
        {
            get
            {
                return isLoading_;
            }
            set
            {
                isLoading_ = value;
                SetProperty(ref isLoading_, value);
            }
        }


        public override bool Equals(object? obj)
        {
            var song = obj as SongVersioned;
            if (song == null)
                return false;
            if (this.Title != song.Title)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Title);
        }
    }
}
