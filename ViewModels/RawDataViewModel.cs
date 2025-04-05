﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace SeiveIT.ViewModels
{
    public partial class RawDataViewModel : ObservableObject
    {
        [ObservableProperty]
        float _phiScale;
        [ObservableProperty]
        string _weight;
        [ObservableProperty]
        double _indWeight;
        [ObservableProperty]
        double _cummWeight;
        [ObservableProperty]
        double _cummPassing;
        [ObservableProperty]
        double _rowNumber;
        public long Id { get; set; }

    }
}
