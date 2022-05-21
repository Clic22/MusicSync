using App1.Models;
using App1.Models.Ports;
using App1.ViewModels;
using GitVersionTool;
using Microsoft.UI.Xaml;
using System;
using WinUIApp;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            FileManager = new FileManager();
            Saver = new LocalSettingsSaver();
            string gitServerUrl = "https://gitlab.com";
            Transport = new GitTransport(gitServerUrl, Saver, FileManager);
            SongsManager = new SongsManager(Transport, Saver, FileManager);
            SongsViewModel = new SongsPageViewModel(SongsManager);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            window_ = new MainWindow();
            WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window_);
            window_.Title = "MusicSync";
            window_.Activate();
        }

        public static Window window_ { get; set; }
        public static IntPtr WindowHandle { get; private set; }
        public static IFileManager FileManager;
        public static ISaver Saver;
        public static ITransport Transport;
        public static ISongsManager SongsManager;
        public static SongsPageViewModel SongsViewModel;
    }
}
