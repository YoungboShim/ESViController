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
using System.Windows.Threading;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;

namespace ESViController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread t;
        XInputController ds4;
        SerialPort serial = new SerialPort();
        ExpanStickManager esMng;

        public MainWindow()
        {
            InitializeComponent();
            ds4 = new XInputController();
        }

        private void ViGEmTest()
        {
            // initializes the SDK instance
            var client = new ViGEmClient();

            // prepares a new Xbox controller
            var esPad = client.CreateXbox360Controller();

            // brings the Xbox controller online
            esPad.Connect();

            if(serial.IsOpen)
            {
                esMng = new ExpanStickManager(serial);
                Debug.WriteLine("esMng created");
            }
            else
            {
                Debug.WriteLine("Serial not connected");
            }

            uint cnt = 0;

            // recommended: run this in its own thread
            while (true)
            {
                try
                {
                    if (ds4.connected)
                    {
                        ds4.Update();
                        esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Axis.LeftThumbX, ds4.gamepad.LeftThumbX);
                        esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Axis.LeftThumbY, ds4.gamepad.LeftThumbY);
                        esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Axis.RightThumbX, ds4.gamepad.RightThumbX);
                        esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Axis.RightThumbY, ds4.gamepad.RightThumbY);
                        esPad.LeftTrigger = (byte)ds4.leftTrigger;
                        esPad.RightTrigger = (byte)ds4.rightTrigger;
                        esPad.SetButtonsFull((ushort)ds4.gamepad.Buttons);

                        if (esMng != null)
                        {
                            esMng.update(ds4.gamepad.LeftThumbX, ds4.gamepad.LeftThumbY, ds4.gamepad.RightThumbX, ds4.gamepad.RightThumbY);

                            if (cnt++ % 10 == 0)
                            {
                                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                                    new Action(
                                        delegate
                                        {
                                            textBox.Text += String.Format("\nLeft: {0}N, Right: {1}N", esMng.es[0].force, esMng.es[1].force);
                                            textBox.ScrollToEnd();
                                        }));
                            }
                            if(esMng.es[0].force > 2)
                            {
                                esPad.SetButtonState(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Button.Y, true);
                            }
                        }

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
            t = new Thread(ViGEmTest);
            t.Start();
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBoxPortList.Items.Add(port);
            }
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (serial.IsOpen)
            {
                serial.Close();
            }

            buttonConnect.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ffdddddd"));
            serial.PortName = comboBoxPortList.SelectedItem.ToString();
            serial.BaudRate = 115200;
            serial.DtrEnable = true;
            serial.RtsEnable = true;

            try
            {
                serial.Open();
                string line = serial.ReadExisting();
                Debug.WriteLine(line);
                buttonConnect.Background = Brushes.Orange;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Serial port open failed");
            }
        }
    }
}
