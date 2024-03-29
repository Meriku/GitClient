﻿using GItClient.Core;
using GItClient.Core.Controllers;
using GItClient.Core.Controllers.SettingControllers;
using GItClient.Core.Convertors;
using GItClient.Core.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GItClient.MVVM.View.MainView
{
    /// <summary>
    /// Interaction logic for InitRepoView.xaml
    /// </summary>
    public partial class InitRepoView : UserControl
    {
        private UserSettings UserSettings;
        private UserSettingsController _userSettingsController;
        private GitController _gitController;
        private DirectoryController _directoryController;

        public InitRepoView()
        {
            InitializeComponent();

            _userSettingsController = new UserSettingsController();
            _gitController = new GitController();
            _directoryController = new DirectoryController();

            UserSettings = _userSettingsController.GetUserSettings();

            User_Directory_Box.SizeChanged += TextBox_SizeChanged;
            User_Directory_Box.TextChanged += TextBox_SizeChanged;

            User_Directory_Box.Text = UserSettings.Directory;
        }

        private void TextBox_SizeChanged(object sender, RoutedEventArgs e)
        {
            User_Directory_Box.Text = TextTrimmer.TrimText((TextBox)sender, UserSettings.Directory);
        }

        private void onclick_Open_Directory_Dialog(object sender, MouseButtonEventArgs e)
        {
            var dialog = _directoryController.GetDirectoryDialog();

            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                var textBox = ((TextBox)sender);
                UserSettings.Directory = dialog.FileName;
                textBox.Text = dialog.FileName;
            }
        }

        private void button_Create_Click(object sender, RoutedEventArgs e)
        {
            _gitController.InitRepositoryAsync(UserSettings.Directory);
        }
    }
}
