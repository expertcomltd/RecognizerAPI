using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ER_ImageRecogniserWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// <seealso cref="System.Windows.Application" />
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            this.Dispatcher.UnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Dispatcher_UnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }
        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            //MessageBox.Show(ex.Message, "Uncaught Thread Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            //Common.Log.Error("CurrentDomain_UnhandledException:" + ex.ToString(), ex);
        }

        /// <summary>
        /// Handles the UnhandledException event of the Dispatcher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="ex">The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs ex)
        {
            //Common.Log.Error("Dispatcher_UnhandledException:" + ex.Exception.ToString());
            ex.Handled = true;
        }
    }
}
