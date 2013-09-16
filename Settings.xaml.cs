using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PIEHidDotNet;
using System.Collections;

namespace VisionMixer
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window, PIEDataHandler, PIEErrorHandler
    {
        public static PIEDevice[] devices;
        public static string device_type;

        public static int[] cbotodevice = null; //for each item in the CboDevice list maps this index to the device index.  Max devices =100 
        public static byte[] wData = null; //write data buffer
        public static int selecteddevice = -1; //set to the index of CboDevice

        public Settings()
        {
            InitializeComponent();
        }

        public void HandlePIEHidError(Int32 error, PIEDevice sourceDevice)
        {
            XkeysStatusLabel.Text = "Error: " + error.ToString();
        }

        public static String BinToHex(Byte value)
        {
            StringBuilder sb = new StringBuilder("");
            sb.Append(value.ToString("X2"));  //the 2 means 2 digits
            return sb.ToString();
        }

        public void HandlePIEHidData(byte[] data, PIEDevice sourceDevice)
        {
            //check the sourceDevice and make sure it is the same device as selected in CboDevice   
            if (sourceDevice == devices[selecteddevice])
            {
                byte[] rdata = null;
                while (0 == sourceDevice.ReadData(ref rdata)) //do this so don't ever miss any data
                {
                    //write raw data to listbox1 in HEX
                    //String output = string.Format("Callback: {0}, ID: {1}, UnitID: {2}, data=", sourceDevice.Pid, selecteddevice.ToString(), rdata[1].ToString());
                    //for (int i = 0; i < sourceDevice.ReadLength; i++)
                    //{
                        //output = output + BinToHex(rdata[i]) + " ";
                    //}

                    String output = string.Format("Callback: {0}, ID: {1}, UnitID: {2}, data=", sourceDevice.Pid, selecteddevice.ToString(), rdata[1].ToString());
                    BitArray col1 = new BitArray(new byte[] { rdata[3] });
                    BitArray col2 = new BitArray(new byte[] { rdata[4] });
                    BitArray col3 = new BitArray(new byte[] { rdata[5] });
                    BitArray col4 = new BitArray(new byte[] { rdata[6] });
                    switch (device_type)
                    {
                        case "XK-24":
                        case "XK-60":
                        case "XK-80":
                            for (int i = 0; i < col1.Length; i++)
                            {
                                if (col1[i])
                                {
                                    int key = i;
                                    output = output + " Key pressed: " + key + " " + col1[i].ToString();
                                }
                            }
                            for (int i = 0; i < col2.Length; i++)
                            {
                                if (col2[i])
                                {
                                    int key = i + 8;
                                    output = output + " Key pressed: " + key + " " + col2[i].ToString();
                                }
                            }
                            for (int i = 0; i < col3.Length; i++)
                            {
                                if (col3[i])
                                {
                                    int key = i + 16;
                                    output = output + " Key pressed: " + key + " " + col3[i].ToString();
                                }
                            }
                            for (int i = 0; i < col4.Length; i++)
                            {
                                if (col4[i])
                                {
                                    int key = i + 24;
                                    output = output + " Key pressed: " + key + " " + col4[i].ToString();
                                }
                            }
                            if (device_type == "XK-24")
                            {
                                // XK-24 only has 24 keys
                                break;
                            }
                            BitArray col5 = new BitArray(new byte[] { rdata[7] });
                            BitArray col6 = new BitArray(new byte[] { rdata[8] });
                            BitArray col7 = new BitArray(new byte[] { rdata[9] });
                            BitArray col8 = new BitArray(new byte[] { rdata[10] });
                            BitArray col9 = new BitArray(new byte[] { rdata[11] });
                            BitArray col10 = new BitArray(new byte[] { rdata[12] });
                            for (int i = 0; i < col5.Length; i++)
                            {
                                if (col1[i])
                                {
                                    int key = i + 32;
                                    output = output + " Key pressed: " + key + " " + col1[i].ToString();
                                }
                            }
                            for (int i = 0; i < col6.Length; i++)
                            {
                                if (col2[i])
                                {
                                    int key = i + 40;
                                    output = output + " Key pressed: " + key + " " + col2[i].ToString();
                                }
                            }
                            for (int i = 0; i < col7.Length; i++)
                            {
                                if (col3[i])
                                {
                                    int key = i + 48;
                                    output = output + " Key pressed: " + key + " " + col3[i].ToString();
                                }
                            }
                            for (int i = 0; i < col8.Length; i++)
                            {
                                if (col4[i])
                                {
                                    int key = i + 56;
                                    output = output + " Key pressed: " + key + " " + col4[i].ToString();
                                }
                            }
                            for (int i = 0; i < col9.Length; i++)
                            {
                                if (col3[i])
                                {
                                    int key = i + 64;
                                    output = output + " Key pressed: " + key + " " + col3[i].ToString();
                                }
                            }
                            for (int i = 0; i < col10.Length; i++)
                            {
                                if (col4[i])
                                {
                                    int key = i + 72;
                                    output = output + " Key pressed: " + key + " " + col4[i].ToString();
                                }
                            }
                            break;
                        default:
                            for (int i = 0; i < sourceDevice.ReadLength; i++)
                            {
                                output = output + BinToHex(rdata[i]) + " ";
                            }
                            break;
                    }
                    
                    OnUIThread(() => Diagnostics.Instance.DiagnosticListbox.Items.Add(output));
                }//end while
            }
        }

        private void OnUIThread(Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                // if you don't want to block the current thread while action is
                // executed, you can also call Dispatcher.BeginInvoke(action);
                Dispatcher.Invoke(action);
            }
        }

        private void DetectXKeysButton_Click(object sender, RoutedEventArgs e)
        {
            DevicesCombo.Items.Clear();
            cbotodevice = new int[128]; //128=max # of devices
            //enumerate and setupinterfaces for all devices
            devices = PIEHidDotNet.PIEDevice.EnumeratePIE();
            int cbocount = 0; //keeps track of how many valid devices were added to the CboDevice box
            if (devices.Length == 0)
            {
                XkeysStatusLabel.Text = "No Devices Found";
            }
            else
            {
                DevicesCombo.IsEnabled = true;
                SetupXKeysButton.IsEnabled = true;
                //System.Media.SystemSounds.Beep.Play(); 
                for (int i = 0; i < devices.Length; i++)
                {
                    //information about device
                    //PID = devices[i].Pid);
                    //HID Usage = devices[i].HidUsage);
                    //HID Usage Page = devices[i].HidUsagePage);
                    //HID Version = devices[i].Version);
                    int hidusagepg = devices[i].HidUsagePage;
                    int hidusage = devices[i].HidUsage;
                    if (devices[i].HidUsagePage == 0xc)
                    {
                        switch (devices[i].Pid)
                        {
                            case 1027:
                                //Device 2 Keyboard, Joystick, Input and Output endpoints
                                DevicesCombo.Items.Add("X-keys XK-24 (" + devices[i].Pid + "=PID #2), ID: " + i);
                                cbotodevice[cbocount] = i;
                                cbocount++;
                                device_type = "XK-24";
                                break;
                            case 1028:
                                //Device 1 Keyboard, Joystick, Mouse and Output endpoints
                                DevicesCombo.Items.Add("X-keys XK-24 (" + devices[i].Pid + "), ID: " + i);
                                cbotodevice[cbocount] = i;
                                cbocount++;
                                device_type = "XK-24";
                                break;
                            case 1029:
                                //Device 0 Keyboard, Mouse, Input and Output endpoints (factory default)
                                DevicesCombo.Items.Add("X-keys XK-24 (" + devices[i].Pid + "=PID #1), ID: " + i);
                                cbotodevice[cbocount] = i;
                                cbocount++;
                                device_type = "XK-24";
                                break;
                            case 1089:
                                //Device 0 Keyboard, Mouse, Input and Output endpoints
                                DevicesCombo.Items.Add("X-keys XK-80 (" + devices[i].Pid + "=PID #1), ID: " + i);
                                cbotodevice[cbocount] = i;
                                cbocount++;
                                device_type = "XK-80";
                                break;
                            case 1090:
                                //Device 1 Keyboard, Joystick, Mouse and Output endpoints
                                DevicesCombo.Items.Add("X-keys XK-80 (" + devices[i].Pid + "), ID: " + i);
                                cbotodevice[cbocount] = i;
                                cbocount++;
                                device_type = "XK-80";
                                break;
                            case 1091:
                                //Device 2 Keyboard, Joystick, Input and Output endpoints (factory default)
                                DevicesCombo.Items.Add("X-keys XK-80 (" + devices[i].Pid + "=PID #2), ID: " + i);
                                cbotodevice[cbocount] = i;
                                cbocount++;
                                device_type = "XK-80";
                                break;
                            case 1121:
                                //Device 0 Keyboard, Mouse, Input and Output endpoints
                                DevicesCombo.Items.Add("X-keys XK-60 (" + devices[i].Pid + "=PID #1), ID: " + i);
                                cbotodevice[cbocount] = i;
                                cbocount++;
                                device_type = "XK-60";
                                break;
                            case 1122:
                                //Device 1 Keyboard, Joystick, Mouse and Output endpoints
                                DevicesCombo.Items.Add("X-keys XK-60 (" + devices[i].Pid + "), ID: " + i);
                                cbotodevice[cbocount] = i;
                                cbocount++;
                                device_type = "XK-60";
                                break;
                            case 1123:
                                //Device 2 Keyboard, Joystick, Input and Output endpoints (factory default)
                                DevicesCombo.Items.Add("X-keys XK-60 (" + devices[i].Pid + "=PID #2), ID: " + i);
                                cbotodevice[cbocount] = i;
                                cbocount++;
                                device_type = "XK-60";
                                break;
                            default:
                                DevicesCombo.Items.Add("Unknown Device (" + devices[i].Pid + "), ID: " + i);
                                cbotodevice[cbocount] = i;
                                cbocount++;
                                device_type = "Unknown";
                                break;
                        }
                        devices[i].SetupInterface(false);
                    }
                }
            }
            if (DevicesCombo.Items.Count > 0)
            {
                XkeysStatusLabel.Text = string.Format("Found {0} {1}.", cbocount.ToString(), cbocount == 1 ? "device" : "devices");
                DevicesCombo.SelectedIndex = 0;
                selecteddevice = cbotodevice[DevicesCombo.SelectedIndex];
                wData = new byte[devices[selecteddevice].WriteLength];//go ahead and setup for write
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (DevicesCombo.SelectedIndex != -1)
            {
                for (int i = 0; i < DevicesCombo.Items.Count; i++)
                {
                    //use the cbotodevice array which contains the mapping of the devices in the DevicesCombo to the actual device IDs
                    devices[cbotodevice[i]].SetErrorCallback(this);
                    devices[cbotodevice[i]].SetDataCallback(this, DataCallbackFilterType.callOnNewData);
                }
                XkeysStatusLabel.Text = "Ready to use.";
            }
        }

        private void DevicesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selecteddevice = cbotodevice[DevicesCombo.SelectedIndex];
            wData = new byte[devices[selecteddevice].WriteLength];//size write array 
        }
    }
}
