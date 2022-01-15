using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using System.IO;

namespace App1
{
    public sealed partial class UploadNewVersionPopUp : ContentDialog
    {
        public UploadNewVersionPopUp(Window window, SongsManager songsManager, Song song)
        {
            this.InitializeComponent();
            this.XamlRoot = window.Content.XamlRoot;
            window_ = window;
            songsManager_ = songsManager;
            song_ = song;
        }

        public void uploadNewVersionClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            songsManager_.uploadNewSongVersion(song_, commitMessageTitle.Text, commitMessageDescription.Text );
        }

        private SongsManager songsManager_;
        private Song song_;
        private Window window_;
    }
}