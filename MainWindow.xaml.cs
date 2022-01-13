using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            versioning_ = new SongVersioning();
            visualElements_ = new VisualElements(this);
        }

        public void SongsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            songSelected_ = (sender as ListBox).SelectedItem as Song;
        }

        private void updateLocalSongClick(object sender, RoutedEventArgs e)
        {
            versioning_.updateLocalSong(songSelected_);
            visualElements_.displayPopUp($"Song '{songSelected_.title}' Updated");
        }

        private void updateLocalSongsClick(object sender, RoutedEventArgs e)
        {
            versioning_.updateLocalSongs();
            visualElements_.displayPopUp("All Songs Updated");
        }

        private void pushNewSongVersionClick(object sender, RoutedEventArgs e)
        {
            versioning_.pushNewSongVersion(songSelected_, commitMessageTitle.Text, commitMessageDescription.Text);
            clearTitleAndDescritpion();
            visualElements_.displayPopUp($"New Version for '{songSelected_.title}' Pushed");
        }

        private void clearTitleAndDescritpion()
        {
            commitMessageTitle.Text = string.Empty;
            commitMessageDescription.Text = string.Empty;
        }

        private SongVersioning versioning_;
        private VisualElements visualElements_;
        private Song songSelected_;
    }
}
