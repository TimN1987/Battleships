using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Battleships.MVVM.ViewModel.Base;

namespace Battleships.MVVM.ViewModel
{
    public class LoadingViewModel : ViewModelBase
    {
        private int _progress;
        public int Progress
        {
            get => _progress;
            set
            {
                Debug.WriteLine($"Progress updating: {value}");
                SetProperty(ref _progress, value);
            }
        }
        public LoadingViewModel()
        {
            _progress = 0;
        }

        public void UpdateProgress(int value)
        {
            Debug.WriteLine($"Value received: {value}");
            Progress = value;
        }
    }
}
