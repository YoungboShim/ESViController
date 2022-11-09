using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Ports;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ESViController
{
    class ExpanStickManager
    {
        public class ExpanStick
        {
            public double posX, posY = 0;
            public int rawVal = 0;
            public double force = 0;
            public double thres = 0;
        }

        public class RegParams
        {
            public double[] N;
            public double[] NE;
            public double[] E;
            public double[] SE;
            public double[] S;
            public double[] SW;
            public double[] W;
            public double[] NW;
        }

        public bool modeChange = false;
        public ExpanStick[] es = new ExpanStick[2]; // 0: left, 1: right
        private SerialPort serial;
        private Dictionary<string, double[]>[] regParams = new Dictionary<string, double[]>[2];
        private string[] dir_list = new string[] { "W", "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

        public ExpanStickManager(SerialPort port)
        {
            serial = port;
            es[0] = new ExpanStick();
            es[1] = new ExpanStick();
            SetRegParams();
        }

        public void update(short lx, short ly, short rx, short ry)
        {
            es[0].posX = (double)lx / short.MaxValue;
            es[0].posY = (double)ly / short.MaxValue;
            es[1].posX = (double)rx / short.MaxValue;
            es[1].posY = (double)ry / short.MaxValue;

            if (SetRawValue())
            {
                rawToForce(0);
                rawToForce(1);
            }
        }

        private bool SetRawValue()
        {
            string[] parsedStr = new string[2];

            if (serial.IsOpen)
            {
                string lines = serial.ReadExisting();
                string[] line = lines.Split('\n');

                if (line.Length > 2)
                {
                    parsedStr = line[line.Length - 2].Split(',');
                }
                else return false;

                int.TryParse(parsedStr[0], out es[0].rawVal);
                int.TryParse(parsedStr[1], out es[1].rawVal);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void rawToForce(int stickLR)
        {
            
            if (magnitude(es[stickLR].posX, es[stickLR].posY) < 0.95f)
            {
                es[stickLR].force = 0;
                es[stickLR].thres = 0;
                return;
            }

            double x = es[stickLR].posX;
            double y = es[stickLR].posY;
            double raw = es[stickLR].rawVal;

            double phi = Math.Atan2(y, x);
            double CW_f = 0, CW_t = 0, CCW_f = 0, CCW_t = 0;

            int mod_phi = (int)Math.Floor(((float)phi + Math.PI) / (Math.PI / 4));
            double rest_phi = ((float)phi + Math.PI) % (Math.PI / 4);
            if (mod_phi > 7)
                mod_phi = 0;
            double fraction = rest_phi / (Math.PI / 4);
            double[] p_CW = regParams[stickLR][dir_list[mod_phi]];
            double[] p_CCW = regParams[stickLR][dir_list[mod_phi + 1]];
            double low_thres = p_CW[2] * (1 - fraction) + p_CCW[2] * fraction;

            if (raw < low_thres)
                es[stickLR].force = 0;
            else
            {
                CW_f = p_CW[0] * Math.Log(raw) + p_CW[1];
                CCW_f = p_CCW[0] * Math.Log(raw) + p_CCW[1];
                CW_t = p_CW[0] * Math.Log(low_thres) + p_CW[1];
                CCW_t = p_CCW[0] * Math.Log(low_thres) + p_CCW[1];

                es[stickLR].force = CW_f * (1 - fraction) + CCW_f * fraction;
                es[stickLR].thres = CW_t * (1 - fraction) + CCW_t * fraction;

                if (es[stickLR].force < 0)
                {
                    es[stickLR].force = 0;
                    es[stickLR].thres = 0;
                }
            }
        }
        public double magnitude(double x, double y)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        private void SetRegParams()
        {
            RegParams[] preParams = new RegParams[2];
            preParams[0] = new RegParams();
            preParams[1] = new RegParams();

            var pathL = @".\\L_params_v2_1.json";
            var pathR = @".\\R_params_v2_2.json";
            string paramJsonL = System.IO.File.ReadAllText(pathL);
            string paramJsonR = System.IO.File.ReadAllText(pathR);

            Debug.WriteLine(pathL);
            preParams[0] = JsonConvert.DeserializeObject<RegParams>(paramJsonL);
            preParams[1] = JsonConvert.DeserializeObject<RegParams>(paramJsonR);

            for (int i = 0; i < 2; i++)
            {
                regParams[i] = new Dictionary<string, double[]>();
                regParams[i]["N"] = preParams[i].N;
                regParams[i]["NW"] = preParams[i].NW;
                regParams[i]["W"] = preParams[i].W;
                regParams[i]["SW"] = preParams[i].SW;
                regParams[i]["S"] = preParams[i].S;
                regParams[i]["SE"] = preParams[i].SE;
                regParams[i]["E"] = preParams[i].E;
                regParams[i]["NE"] = preParams[i].NE;
            }
        }
    }
}
