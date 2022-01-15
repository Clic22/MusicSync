using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using System.IO;

namespace App1
{
    public sealed partial class AddNewSongPopUp : ContentDialog
    {
        public AddNewSongPopUp(Window window, SongsManager songsManager)
        {
            this.InitializeComponent();
            this.XamlRoot = window.Content.XamlRoot;
            songsManager_ = songsManager;
        }

        public void AddNewSongAddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            songsManager_.addSong(songTitle.Text, songlocalPath.Text);
        }

        private SongsManager songsManager_;
    }
}