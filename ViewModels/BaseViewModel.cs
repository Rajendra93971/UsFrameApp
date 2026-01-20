using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UsFrameApp.ViewModels
{
    // base class for all viewmodels
    public partial class BaseViewModel : ObservableObject
    {
        // indicates loading or busy state
        [ObservableProperty]
        private bool isBusy;
    }
}