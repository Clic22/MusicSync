using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
            CurrentVersion = new Version();
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

        public Version CurrentVersion;
        public ObservableCollection<Version> Versions;

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
                    SetProperty(ref isUpdatingSong_, value);
                }
                isUpdatingSong_ = value;
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
                    SetProperty(ref isUploadingSong_, value);
                }
                isUploadingSong_ = value;
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
