﻿#pragma checksum "C:\Users\Pascal\Documents\GitHub\498MA\WIPSA-GPS\voPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "05E76F84779DE84EC1087C9A980D22EC"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18051
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace WIPSA_GPS {
    
    
    public partial class Page2 : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Media.VideoBrush viewfinderBrush;
        
        internal System.Windows.Controls.Button GrayscaleOnButton;
        
        internal System.Windows.Controls.Button GrayscaleOffButton;
        
        internal System.Windows.Controls.Image MainImage;
        
        internal System.Windows.Controls.Canvas TextBoxCanvas;
        
        internal System.Windows.Controls.TextBlock txtDebug;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/WIPSA-GPS;component/voPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.viewfinderBrush = ((System.Windows.Media.VideoBrush)(this.FindName("viewfinderBrush")));
            this.GrayscaleOnButton = ((System.Windows.Controls.Button)(this.FindName("GrayscaleOnButton")));
            this.GrayscaleOffButton = ((System.Windows.Controls.Button)(this.FindName("GrayscaleOffButton")));
            this.MainImage = ((System.Windows.Controls.Image)(this.FindName("MainImage")));
            this.TextBoxCanvas = ((System.Windows.Controls.Canvas)(this.FindName("TextBoxCanvas")));
            this.txtDebug = ((System.Windows.Controls.TextBlock)(this.FindName("txtDebug")));
        }
    }
}

