﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using GItClient.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GItClient.MVVM.ViewModel
{
    internal class CommitsHistoryViewModel : ObservableObject, IViewModel
    {
        public double MinWidth { get => 330; }
        public double MinHeight { get => 250; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public CommitsHistoryViewModel()
        {
            // TODO: implemeted custom views now
            // will be needed in future
            CurrentView = new CommitsHistoryPartialViewModel();

            WeakReferenceMessenger.Default.Register<CommitsPartialViewChangedMessage>(this, (r, m) =>
            { CurrentView = m.Value; });
        }

    }
}
