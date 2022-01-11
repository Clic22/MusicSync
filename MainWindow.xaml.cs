﻿using Microsoft.UI.Xaml;
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
            m_versioning = new Versioning();
            m_visualElements = new VisualElements(this);
        }

        private void updateReposClick(object sender, RoutedEventArgs e)
        {
            m_versioning.updateRepos();
            m_visualElements.displayPopUp("Update Repos Done");
        }

        private void commitAllChangeClick(object sender, RoutedEventArgs e)
        {
            m_versioning.commitAllChanges();
            m_visualElements.displayPopUp("Commit All Changes Done");
        }

        private Versioning m_versioning;
        private VisualElements m_visualElements;
    }
}
