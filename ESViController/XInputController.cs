using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.XInput;
using System.Drawing;
using System.Diagnostics;
using Nefarius.ViGEm.Client.Utilities;
using System.Threading;

namespace ESViController
{
    class XInputController
    {
        Controller controller;
        public Gamepad gamepad;
        public bool connected = false;
        public int deadband = 2500;
        public byte leftThumbX, leftThumbY, rightThumbX, rightThumbY = 0;
        public float leftTrigger, rightTrigger;
        private bool vibrating = false;

        public XInputController()
        {
            var controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

            foreach (var selectControler in controllers)
            {
                if (selectControler.IsConnected)
                {
                    controller = selectControler;
                    break;
                }
            }

            if (controller == null)
                Debug.WriteLine("No controller installed");
            else
                connected = controller.IsConnected;
        }

        public void Update()
        {
            if (!connected)
                return;

            gamepad = controller.GetState().Gamepad;

            leftThumbX = short2byte(gamepad.LeftThumbX);
            leftThumbY = short2byte(-gamepad.LeftThumbY);
            rightThumbX = short2byte(gamepad.RightThumbX);
            rightThumbY = short2byte(-gamepad.RightThumbY);

            leftTrigger = gamepad.LeftTrigger;
            rightTrigger = gamepad.RightTrigger;
        }

        public void VibTick()
        {
            if (!vibrating)
            {
                vibrating = true;
                Thread t = new Thread(VibrateBoth);
                t.Start();
            }
        }

        void VibrateBoth()
        {
            Vibration v = new Vibration();
            v.LeftMotorSpeed = 65535;
            v.RightMotorSpeed = 65535;
            controller.SetVibration(v);
            Thread.Sleep(100);
            v.LeftMotorSpeed = 0;
            v.RightMotorSpeed = 0;
            controller.SetVibration(v);
            vibrating = false;
        }

        private byte short2byte(float val)
        {
            return (byte)(val / short.MaxValue * byte.MaxValue / 2f + byte.MaxValue / 2f);
        }
    }
}
