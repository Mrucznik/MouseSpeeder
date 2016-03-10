using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Gma.System.MouseKeyHook;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace MouseSpeeder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IKeyboardMouseEvents m_GlobalHook;
        private const UInt32 SPI_SETMOUSESPEED = 0x0071;
        private static bool applicationState = true;
        private Keys hotkey;
        private double lol;

        [DllImport("User32.dll")]
        static extern Boolean SystemParametersInfo(
            UInt32 uiAction,
            UInt32 uiParam,
            UInt32 pvParam,
            UInt32 fWinIni);

        public MainWindow()
        {
            InitializeComponent();
            Subscribe();
            
            foreach (string key in Enum.GetNames(typeof (Keys)))
            {
                comboBox_hotkey.Items.Add(key);
            }
            comboBox_hotkey.SelectedIndex = Properties.Settings.Default.HotkeyIdx;
            Enum.TryParse(Properties.Settings.Default.HotkeyName, out hotkey);

            changedslider.Value = Properties.Settings.Default.ChangedSpeed;
            defaultslider.Value = Properties.Settings.Default.DefaultSpeed;
        }

        public static void SetMouseSpeed(uint speed)
        {
            SystemParametersInfo(SPI_SETMOUSESPEED, 0, speed, 0);
        }

        #region Keyboard Hook
        public void Subscribe()
        {
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyDown += OnKeyDown;
            m_GlobalHook.KeyUp += OnKeyUp;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (applicationState)
            {
                if (e.KeyCode == hotkey)
                {
                    rect.Fill = new SolidColorBrush(Colors.Red);
                    SetMouseSpeed((uint)changedslider.Value);
                }
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (applicationState)
            {
                if (e.KeyCode == hotkey)
                {
                    rect.Fill = new SolidColorBrush(Colors.White);
                    SetMouseSpeed((uint)defaultslider.Value);
                }
            }
        }

        public void Unsubscribe()
        {
            m_GlobalHook.KeyDown -= OnKeyDown;
            m_GlobalHook.KeyUp -= OnKeyUp;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }
        #endregion

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            applicationState = true;
            if (comboBox_hotkey != null) comboBox_hotkey.IsEnabled = true;
            if (comboBox_hotkey != null) textBlock.Text = "" + lol;
        }

        private void checkBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            applicationState = false;
            if (comboBox_hotkey != null) comboBox_hotkey.IsEnabled = false;
        }

        private void defaultslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetMouseSpeed((uint)defaultslider.Value);
        }

        private void ComboBox_hotkey_OnDropDownClosed(object sender, EventArgs e)
        {
            Properties.Settings.Default.HotkeyIdx = comboBox_hotkey.SelectedIndex;
            Properties.Settings.Default.HotkeyName = comboBox_hotkey.SelectedItem.ToString();
            Properties.Settings.Default.Save();
            Enum.TryParse(comboBox_hotkey.SelectionBoxItem.ToString(), out hotkey);
        }

        private void Defaultslider_OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            Properties.Settings.Default.DefaultSpeed = (uint)defaultslider.Value;
            Properties.Settings.Default.Save();
        }

        private void Changedslider_OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            Properties.Settings.Default.ChangedSpeed = (uint)changedslider.Value;
            Properties.Settings.Default.Save();
        }
    }
}