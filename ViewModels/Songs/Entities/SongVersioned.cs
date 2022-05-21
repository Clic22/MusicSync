using System.Collections.ObjectModel;

namespace App1.ViewModels
{
    public class SongVersioned : Bindable
    {
        public SongVersioned(string title)
        {
            _title = title;
            _status = string.Empty;
            _isUpdatingSong = false;
            _isUploadingSong = false;
            _isLoading = false;
            _currentVersion = new Version();
            _enableStatus = false;
            UpcomingVersions = new ObservableCollection<Version>();
            Versions = new ObservableCollection<Version>();
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                SetProperty(ref _title, value);
            }
        }

        private string _status;
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                SetProperty(ref _status, value);
                EnableStatus = true;
            }
        }

        private bool _enableStatus;
        public bool EnableStatus
        {
            get
            {
                return _enableStatus;
            }
            set
            {
                SetProperty(ref _enableStatus, value);
            }
        }

        private Version _currentVersion;
        public Version CurrentVersion
        {
            get
            {
                return _currentVersion;
            }
            set
            {
                SetProperty(ref _currentVersion, value);
            }
        }

        public ObservableCollection<Version> UpcomingVersions;

        public ObservableCollection<Version> Versions;

        private bool _isRefreshingSong;
        public bool IsRefreshingSong
        {
            get
            {
                return _isRefreshingSong;
            }
            set
            {
                if (value)
                {
                    Status = "Refreshing...";
                }
                SetProperty(ref _isRefreshingSong, value);
                IsLoading = value;
            }
        }

        private bool _isUpdatingSong;
        public bool IsUpdatingSong
        {
            get
            {
                return _isUpdatingSong;
            }
            set
            {
                if (value)
                {
                    Status = "Updating...";
                }
                SetProperty(ref _isUpdatingSong, value);
                IsLoading = value;
            }
        }

        private bool _isUploadingSong;
        public bool IsUploadingSong
        {
            get
            {
                return _isUploadingSong;
            }
            set
            {
                if (value)
                {
                    Status = "Uploading...";
                }
                SetProperty(ref _isUploadingSong, value);
                IsLoading = value;
            }
        }

        private bool _isRevertingSong;
        public bool IsRevertingSong
        {
            get
            {
                return _isRevertingSong;
            }
            set
            {
                if (value)
                {
                    Status = "Reverting...";
                }
                SetProperty(ref _isRevertingSong, value);
                IsLoading = value;
            }
        }

        private bool _isOpeningSong;
        public bool IsOpeningSong
        {
            get
            {
                return _isOpeningSong;
            }
            set
            {
                if (value)
                {
                    Status = "Opening...";
                }
                SetProperty(ref _isOpeningSong, value);
                IsLoading = value;
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                SetProperty(ref _isLoading, value);
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
