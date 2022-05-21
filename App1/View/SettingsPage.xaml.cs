using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage.Pickers;
using App1.ViewModels;
using App1.Models.Ports;
using WinUIApp;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            ISaver saver = new LocalSettingsSaver();
            IFileManager fileManager = new FileManager();
            SettingsViewModel = new SettingsPageViewModel(saver, fileManager);
        }

        public async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsViewModel.SaveSettings();
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "Settings Saved";
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            await dialog.ShowAsync();
        }

        public void RevealModeCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            if (RevealModeCheckBox.IsChecked == true)
            {
                BandPassword.PasswordRevealMode = PasswordRevealMode.Visible;
            }
            else
            {
                BandPassword.PasswordRevealMode = PasswordRevealMode.Hidden;
            }
        }

        public async void Folder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.WindowHandle);
            var folderPicked = await folderPicker.PickSingleFolderAsync();
            if (folderPicked != null)
            {
                SettingsViewModel.MusicSyncFolder = folderPicked.Path;
            }
        }

        public SettingsPageViewModel SettingsViewModel;
    }
}
