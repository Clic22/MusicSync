using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using System.IO;

namespace App1
{
    public sealed partial class AddNewSong : ContentDialog
    {
        public AddNewSong(Window window, SongsStorage songsList)
        {
            this.InitializeComponent();
            this.XamlRoot = window.Content.XamlRoot;
            songsList_ = songsList;
        }

        public void AddNewSongAddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Song song = new Song();
            song.title = songTitle.Text;
            song.localPath = songlocalPath.Text;
            songsList_.Add(song);
        }

        private SongsStorage songsList_;
    }
}