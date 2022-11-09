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
        Thread tCuphead, tFps;
        XInputController ds4;
        SerialPort serial = new SerialPort();
        ExpanStickManager esMng;
        double forceThres = 2;
        bool ViGEmCupheadThreadOn = true;
        bool ViGEmFpsThreadOn = true;
        double stickPercent = 0.5f;
        double fpsCursorForceT = 2f;
        double slope = 1;
        static double maxForce = 4.5;

        public MainWindow()
        {
            InitializeComponent();
            ds4 = new XInputController();
        }

        private void ViGEmCuphead()
        {
            uint cnt = 0;
            bool thresPassed = false;
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

            this.Dispatcher.Invoke(DispatcherPriority.Normal,
                                    new Action(
                                        delegate
                                        {
                                            textBox.Text += "\nVigEm Cuphead Start";
                                            textBox.ScrollToEnd();
                                        }));

            // recommended: run this in its own thread
            while (ViGEmCupheadThreadOn)
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

                            /*
                            if (cnt++ % 1000 == 0)
                            {
                                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                                    new Action(
                                        delegate
                                        {
                                            textBox.Text += String.Format("\nLeft: {0}N, Right: {1}N", esMng.es[0].force, esMng.es[1].force);
                                            textBox.ScrollToEnd();
                                        }));
                                Debug.WriteLine(String.Format("left x: {0}, y: {1}", ds4.gamepad.LeftThumbX, ds4.gamepad.LeftThumbY));
                            }
                            */
                            
                            
                            if(!thresPassed && esMng.es[0].force > forceThres && Math.Abs(ds4.gamepad.LeftThumbX) > 27000)
                            {
                                esPad.SetButtonState(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Button.Y, true);
                                ds4.VibTick();
                                thresPassed = true;
                                /*
                                Debug.WriteLine("Y button pressed!");
                                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                                    new Action(
                                        delegate
                                        {
                                            textBox.Text += "\n!!!Y pressed!!!";
                                            textBox.ScrollToEnd();
                                        }));
                                */
                            }
                            else if (esMng.es[0].force < forceThres)
                            {
                                thresPassed = false;
                            }
                        }

                        Thread.Sleep(1000/120);
                    }
                    else
                    {
                        Debug.WriteLine("Not connected");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    //Thread.Sleep(10);
                }
            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal,
                                    new Action(
                                        delegate
                                        {
                                            textBox.Text += "\nVigEm Cuphead Finished";
                                            textBox.ScrollToEnd();
                                        }));
        }

        private void ViGEmFps()
        {
            uint cnt = 0;
            // initializes the SDK instance
            var client = new ViGEmClient();

            // prepares a new Xbox controller
            var esPad = client.CreateXbox360Controller();

            // brings the Xbox controller online
            esPad.Connect();

            if (serial.IsOpen)
            {
                esMng = new ExpanStickManager(serial);
                Debug.WriteLine("esMng created");
            }
            else
            {
                Debug.WriteLine("Serial not connected");
            }

            this.Dispatcher.Invoke(DispatcherPriority.Normal,
                                    new Action(
                                        delegate
                                        {
                                            textBox.Text += "\nVigEm FPS Start";
                                            textBox.ScrollToEnd();
                                        }));

            // recommended: run this in its own thread
            while (ViGEmFpsThreadOn)
            {
                try
                {
                    if (ds4.connected)
                    {
                        ds4.Update();
                        esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Axis.LeftThumbX, ds4.gamepad.LeftThumbX);
                        esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Axis.LeftThumbY, ds4.gamepad.LeftThumbY);
                        esPad.LeftTrigger = (byte)ds4.leftTrigger;
                        esPad.RightTrigger = (byte)ds4.rightTrigger;
                        esPad.SetButtonsFull((ushort)ds4.gamepad.Buttons);

                        if (esMng != null)
                        {
                            esMng.update(ds4.gamepad.LeftThumbX, ds4.gamepad.LeftThumbY, ds4.gamepad.RightThumbX, ds4.gamepad.RightThumbY);

                            short newRx = 0, newRy = 0;
                            double tmpRx = 0, tmpRy = 0;
                            double rF = esMng.es[1].force;
                            double mag = Math.Sqrt(Math.Pow(ds4.gamepad.RightThumbX, 2) + Math.Pow(ds4.gamepad.RightThumbY, 2));
                            if (rF > fpsCursorForceT)
                            {
                                tmpRx = ds4.gamepad.RightThumbX * stickPercent + ds4.gamepad.RightThumbX / mag * short.MaxValue * (1 - stickPercent) / (maxForce - fpsCursorForceT) * (rF - fpsCursorForceT) * slope;
                                tmpRy = ds4.gamepad.RightThumbY * stickPercent + ds4.gamepad.RightThumbY / mag * short.MaxValue * (1 - stickPercent) / (maxForce - fpsCursorForceT) * (rF - fpsCursorForceT) * slope;
                                tmpRx = tmpRx > short.MaxValue ? short.MaxValue : tmpRx;
                                tmpRx = tmpRx < short.MinValue ? short.MinValue : tmpRx;
                                tmpRy = tmpRy > short.MaxValue ? short.MaxValue : tmpRy;
                                tmpRy = tmpRy < short.MinValue ? short.MinValue : tmpRy;
                                newRx = Convert.ToInt16(tmpRx);
                                newRy = Convert.ToInt16(tmpRy);
                            }
                            else
                            {
                                newRx = Convert.ToInt16(ds4.gamepad.RightThumbX * stickPercent);
                                newRy = Convert.ToInt16(ds4.gamepad.RightThumbY * stickPercent);
                            }
                            esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Axis.RightThumbX, newRx);
                            esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Axis.RightThumbY, newRy);
                            if (cnt++ % 200 == 0)
                                Debug.WriteLine(String.Format("x: {0}, y: {1}", newRx, newRy));
                        }
                        else
                        {
                            esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Axis.RightThumbX, ds4.gamepad.RightThumbX);
                            esPad.SetAxisValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Axis.RightThumbY, ds4.gamepad.RightThumbY);
                        }

                        Thread.Sleep(1000 / 120);
                    }
                    else
                    {
                        Debug.WriteLine("Not connected");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal,
                                    new Action(
                                        delegate
                                        {
                                            textBox.Text += "\nVigEm FPS Finished";
                                            textBox.ScrollToEnd();
                                        }));
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            ViGEmCupheadThreadOn = true;
            tCuphead = new Thread(ViGEmCuphead);
            tCuphead.Priority = ThreadPriority.Highest;
            tCuphead.Start();
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

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            forceThres = ((Slider)sender).Value;
            if(textBoxSVal != null)
                textBoxSVal.Text = forceThres.ToString();
            Debug.WriteLine(String.Format("force threshold: {0}N", forceThres));
        }

        private void buttonEnd_Click(object sender, RoutedEventArgs e)
        {
            ViGEmCupheadThreadOn = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(ViGEmCupheadThreadOn || tCuphead.IsAlive)
            {
                ViGEmCupheadThreadOn = false;
                tCuphead.Abort();
            }
            if (ViGEmFpsThreadOn || tFps.IsAlive)
            {
                ViGEmFpsThreadOn = false;
                tFps.Abort();
            }
        }

        private void sliderPrcntg_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(textBoxPercntg != null)
                textBoxPercntg.Text = ((int)e.NewValue).ToString();
            stickPercent = e.NewValue / 100;
        }

        private void buttonFpsStart_Click(object sender, RoutedEventArgs e)
        {
            ViGEmFpsThreadOn = true;
            tFps = new Thread(ViGEmFps);
            tFps.Priority = ThreadPriority.Highest;
            tFps.Start();
        }

        private void buttonFpsEnd_Click(object sender, RoutedEventArgs e)
        {
            ViGEmFpsThreadOn = false;
        }

        private void sliderSlope_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (textBoxSlope != null)
            {
                textBoxSlope.Text = e.NewValue.ToString();
            }
            slope = e.NewValue;
        }

        private void sliderCursorFT_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(textBoxCursorFT != null)
                textBoxCursorFT.Text = e.NewValue.ToString();
            fpsCursorForceT = e.NewValue;
        }
    }
}
