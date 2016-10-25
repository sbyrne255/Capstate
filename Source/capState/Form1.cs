using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading;
using Gma.System.MouseKeyHook;

namespace capState
{
    public partial class Form1 : Form
    {
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
        const int WS_EX_NOACTIVATE = 0x08000000;
        const int WS_EX_TOPMOST = 0x00000008;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams param = base.CreateParams;
                param.ExStyle |= WS_EX_TOPMOST; // make the form topmost
                param.ExStyle |= WS_EX_NOACTIVATE; // prevent the form from being activated
                return param;
            }
        }
        [DllImport("user32.dll")]
        private extern static IntPtr SetActiveWindow(IntPtr handle);
        private const int WM_ACTIVATE = 6;
        private const int WA_INACTIVE = 0;
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int MA_NOACTIVATEANDEAT = 0x0004;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEACTIVATE)
            {
                m.Result = (IntPtr)MA_NOACTIVATEANDEAT; // prevent the form from being clicked and gaining focus
                return;
            }
            if (m.Msg == WM_ACTIVATE) // if a message gets through to activate the form somehow
            {
                if (((int)m.WParam & 0xFFFF) != WA_INACTIVE)
                {

                    if (m.LParam != IntPtr.Zero)
                    {
                        SetActiveWindow(m.LParam);
                    }
                    else
                    {
                        // Could not find sender, just in-activate it.
                        SetActiveWindow(IntPtr.Zero);
                    }

                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);
        public System.Timers.Timer myTimer = new System.Timers.Timer();
        public System.Timers.Timer hideForm = new System.Timers.Timer();

        public bool capOn() {
            return (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
        }
        public bool numOn()
        {
            return (((ushort)GetKeyState(0x90)) & 0xffff) != 0;
        }
        public bool StartingC = false;
        public bool StartingN = false;
        public bool hide = true;

        private void Form1_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.TransparencyKey = Color.Turquoise;
            this.BackColor = Color.Turquoise;

            PlaceLowerRight();
            checkLocks(null, null);
            StartingC = capOn();
            StartingN = numOn();

            myTimer.Elapsed += new ElapsedEventHandler(checkLocks);
            myTimer.Interval = (200); // 1000 ms is one second//200
            myTimer.AutoReset = true;
            myTimer.SynchronizingObject = this;
            myTimer.Start();

        }
        public void checkLocks(object source, ElapsedEventArgs e)
        {
            if (StartingC != capOn())
            {
                if (capOn()) { button1.BackColor = Color.Green; }
                else { button1.BackColor = Color.Transparent; }
                StartingC = capOn();
                this.Show();
                StartCloseTimer();
                //Start second timer for re-hiding...
            }
            if (StartingN != numOn())
            {
                if (numOn()) { button2.BackColor = Color.Green; }
                else { button2.BackColor = Color.Transparent; }
                StartingN = numOn();
                this.Show();
                StartCloseTimer();
            }

        }
        private void StartCloseTimer()
        {
            hideForm.Elapsed += new ElapsedEventHandler(hideMe);
            hideForm.Interval = (2000); // 1000 ms is one second//200
            hideForm.AutoReset = false;
            hideForm.SynchronizingObject = this;
            hideForm.Start();
        }

        private void hideMe(object source, ElapsedEventArgs e)
        {
            this.Hide();
        }
        public void PlaceLowerRight()
        {
            //Determine "rightmost" screen
            Screen rightmost = Screen.AllScreens[0];
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
                {
                    rightmost = screen;
                }
                break;
            }

            this.Left = rightmost.WorkingArea.Right - this.Width;
            this.Top = rightmost.WorkingArea.Bottom - this.Height;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (hide)
            {
                this.Hide();
                hide = false;
            }
        }
    }
}
