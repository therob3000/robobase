﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using Microsoft.Kinect;
using System.IO;

namespace SerialPortDotNET
{
    public partial class Form1 : Form
    {
        private KinectSensor sensor;
        int BaseVal = 1500;
        public SerialPort sport;
        public string str_powerOffRobot = "#0L #1L #2L #3L #4L #5L #6L #7L #8L #9L #10L #11L #12L #13L  #14 P1500 #15 P1500\r";
        public string str_centerRobot = "#0 P1500 #1 P1500 #2 P1500 #3 P1500 #4 P1500 #5 P1500 #6 P1500 #7 P1500 #8 P1500 #9 P1500 #10 P1500 #11 P1500 #12 P1500 #13 P1500 #14 P1500 #15 P1500\r";
        public Form1()
        {
            InitializeComponent();
            //Initialization of ComPort 1
            sport = new SerialPort("COM1");
            //Must match baud rate of SSC-32
            sport.BaudRate = 115200;
            //Not sure why, but this has to be here
            sport.Parity = System.IO.Ports.Parity.None;
            sport.DataBits = 8;
            sport.StopBits = System.IO.Ports.StopBits.One;
            //Open Port
            sport.Open();
            //Center Everything
            sport.Write(str_centerRobot);

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
             foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
               MessageBox.Show( "No Kinect");
            }
        }

        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            if (skeletons.Length != 0)
            {
                foreach (Skeleton skel in skeletons)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {

                        tb_ElbowPos.Text = skel.Joints[JointType.ElbowLeft].Position.X.ToString();
                        tb_ElbowPos2.Text = skel.Joints[JointType.ElbowLeft].Position.Y.ToString();
                    }
                }
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Power Down Robot and Port
            sport.Write(str_powerOffRobot);
            sport.Close();
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Set Base to specific angle
            double angle = (double)numericUpDown1.Value;
            int pulse = (int)(angle * (10)) + 600; 
            sport.Write("#0 P" + pulse +"\r");
            
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                    //Rotate Base CCW
                case Keys.Q:
                    if (!(BaseVal <= 500))
                        BaseVal -= 20;
                    sport.Write("#0 P" + BaseVal + "\r");
                    break;
                    //Rotate Base CW
                case Keys.E:
                    if (!(BaseVal >= 2500))
                        BaseVal += 20;
                    sport.Write("#0 P" + BaseVal + "\r");
                    break;

                    //Tracks Reverse
                case Keys.S:
                    sport.Write("#15 P1600 #14 P1400\r");
                    break;

                    //Tracks Forward
                case Keys.W:
                    sport.Write("#15 P1400 #14 P1600\r");
                    break;
                    //Tracks Turn Left
                case Keys.A:
                    sport.Write("#15 P1400 #14 P1400\r");
                    break;
                    //Tracks Turn Right
                case Keys.D:
                    sport.Write("#15 P1600 #14 P1600\r");
                    break;
                    //Power Down Code
                case Keys.K:
                    sport.Write(str_powerOffRobot);
                    break;
                    //Power Up and Center
                case Keys.L:
                    sport.Write(str_centerRobot);
                    break;

            }
            
        }

    }
}
