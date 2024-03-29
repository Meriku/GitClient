﻿using GItClient.Core.Controllers;
using GItClient.Core.Controllers.SettingControllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;

namespace GItClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var userSettinsController = new UserSettingsController();
            var profileCreated = userSettinsController.IsInitialSettingsFilled();

            this.StartupUri = profileCreated ?
                            new Uri("MainWindow.xaml", UriKind.Relative) :
                            new Uri("MVVM/Pages/Welcome.xaml", UriKind.Relative);

        }
    }
}
