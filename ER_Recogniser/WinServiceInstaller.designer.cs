using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace ER_Recogniser
{
    /// <summary>
    /// A windows service installer class
    /// </summary>
    /// <seealso cref="System.Configuration.Install.Installer" />
    partial class WinServiceInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller
            // 
            this.serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstaller.Password = null;
            this.serviceProcessInstaller.Username = null;
            // 
            // serviceInstaller
            // 
            this.serviceInstaller.Description = "ER_Recogniser - ServiceStack Windows Service.";
            this.serviceInstaller.DisplayName = "ER_Recogniser";
            this.serviceInstaller.ServiceName = "ER_Recogniser";
            this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // WinServiceInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller,
            this.serviceInstaller});

        }

        #endregion

        /// <summary>
        /// The service process installer
        /// </summary>
        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
        /// <summary>
        /// The service installer
        /// </summary>
        private System.ServiceProcess.ServiceInstaller serviceInstaller;
    }
}
