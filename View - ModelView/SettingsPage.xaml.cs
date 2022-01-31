using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

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
            user_ = User.Instance;
            gitLabUsername.Text = Windows.Storage.ApplicationData.Current.LocalSettings.Values["gitLabUsername"] as string;
            gitLabPassword.Password = Windows.Storage.ApplicationData.Current.LocalSettings.Values["gitLabPassword"] as string;
            gitUsername.Text = Windows.Storage.ApplicationData.Current.LocalSettings.Values["gitUsername"] as string;
            gitEmail.Text = Windows.Storage.ApplicationData.Current.LocalSettings.Values["gitEmail"] as string;
        }

        public async void saveSettingsClick(object sender, RoutedEventArgs e)
        {
            user_.saveSettings(gitLabUsername.Text, gitLabPassword.Password, gitUsername.Text, gitEmail.Text);
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
                gitLabPassword.PasswordRevealMode = PasswordRevealMode.Visible;
            }
            else
            {
                gitLabPassword.PasswordRevealMode = PasswordRevealMode.Hidden;
            }
        }

        private User user_;
    }
}
