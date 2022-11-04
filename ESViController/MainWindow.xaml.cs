using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nefarius.ViGEm.Client;
using System.Threading;
using System.Diagnostics;

namespace ESViController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread t;
        XInputController ds4;

        public MainWindow()
        {
            InitializeComponent();
            ds4 = new XInputController();
        }

        private void ViGEmTest()
        {
            // initializes the SDK instance
            var client = new ViGEmClient();

            // prepares a new DS4
            var esPad = client.CreateDualShock4Controller();

            // brings the DS4 online
            esPad.Connect();

            Debug.WriteLine("Start button clicked - ViGEm thread");

            // recommended: run this in its own thread
            while (true)
            {
                try
                {
                    if (ds4.connected)
                    {
                        ds4.Update();
                        esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.DualShock4.DualShock4Axis.LeftThumbX, ds4.leftThumbX);
                        esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.DualShock4.DualShock4Axis.LeftThumbY, ds4.leftThumbY);
                        esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.DualShock4.DualShock4Axis.RightThumbX, ds4.rightThumbX);
                        esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.DualShock4.DualShock4Axis.RightThumbY, ds4.rightThumbY);
                        esPad.LeftTrigger = (byte)ds4.leftTrigger;
                        esPad.RightTrigger = (byte)ds4.rightTrigger;
                        Thread.Sleep(10);
                    }
                    else
                    {
                        Debug.WriteLine("Not connected");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Thread.Sleep(1000);
                }
            }
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Start button clicked - main thread");
            t = new Thread(ViGEmTest);
            t.Start();
        }

        private void buttonEnd_Click(object sender, RoutedEventArgs e)
        {
            if (t.IsAlive)
            {
                // Terminate thread somehow.
            }
        }
    }
}
