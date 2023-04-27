using Http.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Http
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static DataContext dataContext = null!;
        public static DataContext DataContext { 
            get
            {
                if (dataContext == null)
                {
                    dataContext = new DataContext();
                }
                return dataContext;
            }
        }

        public static NpUser? AuthUser { get; set; }
    }
}
