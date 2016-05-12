using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// for the interface
using System.ComponentModel;

namespace csharp_kinect_info
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        // required implementation for the interface
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnNotifyPropertyChange(string propertyName)
        {
            if(this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int frameNumberValue;
        public int FrameNumber
        {
            get
            {
                return this.frameNumberValue;
            }
            set
            {
                if(frameNumberValue != value)
                {
                    this.frameNumberValue = value;
                    this.OnNotifyPropertyChange("FrameNumber");
                }
            }
        }

        // register properties that need notification
        private string connectionIDValue;
        public string ConnectionID
        {
            get
            {
                return this.connectionIDValue;
            }
            set
            {
                if(connectionIDValue != value)
                {
                    this.connectionIDValue = value;
                    this.OnNotifyPropertyChange("ConnectionID");
                }
            }
        }

        private string deviceIDValue;
        public string DeviceID
        {
            get
            {
                return this.deviceIDValue;
            }
            set
            {
                if(deviceIDValue != value)
                {
                    this.deviceIDValue = value;
                    this.OnNotifyPropertyChange("DeviceID");
                }
            }
        }

        private string canStartValue;
        public string CanStart
        {
            get
            {
                return this.canStartValue;
            }
            set
            {
                if(canStartValue != value)
                {
                    this.canStartValue = value;
                    this.OnNotifyPropertyChange("CanStart");
                }
            }
        }

        private string canStopValue;
        public string CanStop
        {
            get
            {
                return this.canStopValue;
            }
            set
            {
                if(canStopValue != value)
                {
                    this.canStopValue = value;
                    this.OnNotifyPropertyChange("CanStop");
                }
            }
        }

        private string isColorStreamEnabledValue;
        public string IsColorStreamEnabled
        {
            get
            {
                return this.isColorStreamEnabledValue;
            }
            set
            {
                if(isColorStreamEnabledValue != value)
                {
                    this.isColorStreamEnabledValue = value;
                    this.OnNotifyPropertyChange("IsColorStreamEnabled");
                }
            }
        }

        private string isDepthStreamEnabledValue;
        public string IsDepthStreamEnabled
        {
            get
            {
                return this.isDepthStreamEnabledValue;
            }
            set
            {
                if(isDepthStreamEnabledValue != value)
                {
                    this.isDepthStreamEnabledValue = value;
                    this.OnNotifyPropertyChange("IsDepthStreamEnabled");
                }
            }
        }

        private int sensorAngleValue;
        public int SensorAngle
        {
            get
            {
                return this.sensorAngleValue;
            }
            set
            {
                if(sensorAngleValue != value)
                {
                    this.sensorAngleValue = value;
                    this.OnNotifyPropertyChange("SensorAngle");
                }
            }
        }

        private string sensorStatusValue;
        public string SensorStatus
        {
            get
            {
                return this.sensorStatusValue;
            }
            set
            {
                if(sensorStatusValue != value)
                {
                    this.sensorStatusValue = value;
                    this.OnNotifyPropertyChange("SensorStatus");
                }
            }
        }
    }
}
