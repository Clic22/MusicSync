using App1.Models.Ports;
using App1.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage.Pickers;
using WinUIApp;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
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
            settingsViewModel = new SettingsPageViewModel(saver, fileManager);
        }

        public async void saveSettingsClick(object sender, RoutedEventArgs e)
        {
            settingsViewModel.MusicSyncFolder = MusicSyncFolder.Text;
            settingsViewModel.CheckUpdatesFrequency = CheckUpdatesFrequency.Text;
            settingsViewModel.saveSettings(BandName.Text, BandPassword.Password, BandEmail.Text, Username.Text);
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "Settings Saved";
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            await dialog.ShowAsync();
        }

        private void RevealModeCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            if (revealModeCheckBox.IsChecked == true)
            {
                BandPassword.PasswordRevealMode = PasswordRevealMode.Visible;
            }
            else
            {
                BandPassword.PasswordRevealMode = PasswordRevealMode.Hidden;
            }
        }

        private async void folder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.WindowHandle);
            var folderPicked = await folderPicker.PickSingleFolderAsync();
            if (folderPicked != null)
            {
                MusicSyncFolder.Text = folderPicked.Path;
            }
        }

        private SettingsPageViewModel settingsViewModel;
    }
}
