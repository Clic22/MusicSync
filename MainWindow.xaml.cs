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

        private void updateLocalSongsClick(object sender, RoutedEventArgs e)
        {
            versioning_.updateLocalSongs();
            visualElements_.displayPopUp("Songs Updated");
        }

        private void pushNewSongsVersionsClick(object sender, RoutedEventArgs e)
        {
            versioning_.pushNewSongsVersions();
            visualElements_.displayPopUp("New Songs Versions Pushed");
        }

        private SongVersioning versioning_;
        private VisualElements visualElements_;
    }
}
