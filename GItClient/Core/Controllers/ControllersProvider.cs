﻿using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace GItClient.Core.Controllers
{
    internal static class ControllersProvider
    {

        private static UserSettingsController? _userSettingsController;
        private static AppSettingsController? _appSettingsController;
        private static AnimationController? _animationController;
        private static GitController? _gitController;

        public static UserSettingsController GetUserSettingsController()
        {
            _userSettingsController ??= new UserSettingsController();
            return _userSettingsController;
        }

        public static AppSettingsController GetAppSettingsController()
        {
            _appSettingsController ??= new AppSettingsController();
            return _appSettingsController;
        }
        public static AnimationController GetAnimationController()
        {
            _animationController ??= new AnimationController();
            return _animationController;
        }

        public static GitController GetGitController()
        {
            _gitController ??= new GitController();
            return _gitController;
        }
    }
}
