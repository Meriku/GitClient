﻿using GItClient.Core;
using GItClient.Core.Controllers;
using GItClient.Core.Controllers.SettingControllers;
using GItClient.Core.Convertors;
using GItClient.Core.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Management.Automation.Language;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GItClient.MVVM.View.MainView
{
    /// <summary>
    /// Interaction logic for UserInfo.xaml
    /// </summary>
    public partial class UserInfoView : UserControl
    {
        private UserSettings UserSettings;
        private UserSettings UserSettingsCopy;
        private UserSettingsController _userSettingsController;
        private GitController _gitController;
        private DirectoryController _directoryController;

        public UserInfoView()
        {
            InitializeComponent();

            InputManager.Current.PreProcessInput += (sender, e) =>
            {
                if (e.StagingItem.Input is MouseButtonEventArgs args && args.ClickCount > 0)
                {
                    textBoxEmail_LostFocus(sender, (MouseButtonEventArgs)e.StagingItem.Input);
                }       
            };

            _userSettingsController = new UserSettingsController();
            _gitController = new GitController();
            _directoryController = new DirectoryController();

            UserSettings = _userSettingsController.GetUserSettings().Clone();
            UserSettingsCopy = UserSettings.Clone();

            User_Name_Box.Text = UserSettings.Username;
            User_Email_Box.Text = UserSettings.Email;

            User_Directory_Box.SizeChanged += TextBox_SizeChanged;
            User_Directory_Box.TextChanged += TextBox_SizeChanged;

            User_Directory_Box.Text = UserSettings.Directory;
        }
        
        private async void checkBox_ShowGitResponsesChanged(object sender, RoutedEventArgs args)
        {
            // TODO: async
            // TODO: update git bar
            // TODO: show cheked chekboxes if setting = true
            UserSettings.Optional.ShowGitResponses = ((CheckBox)sender).IsChecked ?? false;
            await _userSettingsController.SetAndSaveUserSettings(UserSettings);
        }
        private void TextBox_SizeChanged(object sender, RoutedEventArgs e)
        {
            User_Directory_Box.Text = TextTrimmer.TrimText((TextBox)sender, UserSettings.Directory);
        }

        private async void button_Save_Click(object sender, RoutedEventArgs e)
        {
            UserSettings.Username = User_Name_Box.Text;
            UserSettingsCopy = UserSettings.Clone();

            await Task.Run(async () =>
            {
                await _userSettingsController.SetAndSaveUserSettings(UserSettings);
            });       
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            User_Name_Box.Text = UserSettingsCopy.Username;
            User_Email_Box.Text = UserSettingsCopy.Email;
            User_Directory_Box.Text = Helper.TrimDirectoryName(UserSettingsCopy.Directory);

            textBoxEmail_LostFocus(sender, e);

            UserSettings = UserSettingsCopy.Clone();
        }

        private void textBoxEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            var email = User_Email_Box.Text;

            if (UserSettings.Email.Equals(email)) 
            {
                email_Error_Icon.Visibility = Visibility.Hidden;
                button_Save.IsEnabled = true;
                return;
            };

            if (email.Length == 0) 
            { 
                UserSettings.Email = email;
                email_Error_Icon.Visibility = Visibility.Hidden;
                button_Save.IsEnabled = true; 
                return;
            };

            if (MailAddress.TryCreate(email, out var result))
            {
                UserSettings.Email = result.Address;
                User_Email_Box.Text = result.Address;

                email_Error_Icon.Visibility = Visibility.Hidden;
                button_Save.IsEnabled = true;
            }
            else
            {
                email_Error_Icon.Visibility = Visibility.Visible;
                button_Save.IsEnabled = false;
            }
        }

        private void onclick_Open_Directory_Dialog(object sender, MouseButtonEventArgs e)
        {
            var dialog = _directoryController.GetDirectoryDialog();

            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                UserSettings.Directory = dialog.FileName;
                ((TextBox)sender).Text = Helper.TrimDirectoryName(dialog.FileName);
            }
        }
    }
}
