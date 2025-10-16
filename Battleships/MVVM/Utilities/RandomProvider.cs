using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleships.MVVM.Utilities
{
    public static class RandomProvider
    {
        public static readonly Random Instance = new();
    }
}
