// MIT License
// 
// Copyright (c) 2018 Chris Miller
// See https://github.com/anotherlab/FocusUF
// Copyright (c) 2020 Mike Ormond
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DirectShowLib;

namespace FocusGUI
{
    class CameraProperties
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Step { get; set; }
        public int Default { get; set; }
        public CameraControlFlags PossFlags { get; set; }
    }

    class ViewModel : DependencyObject
    {
        private static IAMCameraControl camera;
        private static DsDevice[] devs = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        public CameraProperties FocusProperties { get; set; }
        public CameraProperties ExposureProperties { get; set; }

        public ViewModel()
        {
            if (devs.Length > 0)
            {
                string _cameraName = (string)Application.Current.Resources["CameraName"];
                DsDevice cam = (DsDevice)devs.Where(d => d.Name.ToLower().Contains(_cameraName.ToLower())).FirstOrDefault();

                if (cam != null)
                {
                    camera = GetCamera(cam);

                    FocusProperties = CameraGetRange(camera, CameraControlProperty.Focus);
                    Tuple<int, CameraControlFlags> focusSetting = CameraGetSettings(camera, CameraControlProperty.Focus);
                    this.FocusValue = focusSetting.Item1;
                    this.AutoFocusEnabled = focusSetting.Item2 == CameraControlFlags.Auto;

                    ExposureProperties = CameraGetRange(camera, CameraControlProperty.Exposure);
                    Tuple<int, CameraControlFlags> exposureSetting = CameraGetSettings(camera, CameraControlProperty.Exposure);
                    this.ExposureValue = exposureSetting.Item1;
                    this.AutoExposureEnabled = exposureSetting.Item2 == CameraControlFlags.Auto;
                }
                else
                {
                    // No matching camera found
                    throw new ArgumentException("Matching camera not found - check the camera name in App Resources");
                }
            }
        }

        private Tuple<int, CameraControlFlags> CameraGetSettings(IAMCameraControl camera, CameraControlProperty cameraProperty)
        {
            int _value;
            CameraControlFlags _setting;

            camera.Get(cameraProperty,
                out _value,
                out _setting);

            return new Tuple<int, CameraControlFlags>(_value, _setting);
        }

        private CameraProperties CameraGetRange(IAMCameraControl camera, CameraControlProperty cameraProperty)
        {
            int _min;
            int _max;
            int _step;
            int _default;
            CameraControlFlags _possFlags;

            camera.GetRange(cameraProperty,
                        out _min,
                        out _max,
                        out _step,
                        out _default,
                        out _possFlags);

            return new CameraProperties
            {
                Min = _min,
                Max = _max,
                Step = _step,
                Default = _default,
                PossFlags = _possFlags
            };
        }

        private static IAMCameraControl GetCamera(DsDevice dev)
        {
            IAMCameraControl _camera = null;
            if (dev != null)
            {
                // DirectShow uses a module system called filters to exposure the functionality
                // We create a new object that implements the IFilterGraph2 interface so that we can
                // new filters to exposure the functionality that we need.
                if (new FilterGraph() is IFilterGraph2 graphBuilder)
                {
                    // Create a video capture filter for the device
                    graphBuilder.AddSourceFilterForMoniker(dev.Mon, null, dev.Name, out IBaseFilter capFilter);

                    // Cast that filter to IAMCameraControl from the DirectShowLib
                    _camera = capFilter as IAMCameraControl;
                }
            }
            return _camera;
        }

        private static void SetCameraFlag(CameraControlProperty camProperty, CameraControlFlags flagVal)
        {
            if (camera != null)
            {
                // Get the current settings from the webcam
                camera.Get(camProperty, out int v, out CameraControlFlags f);

                // If the camera property differs from the desired value, adjust it leaving value the same.
                if (f != flagVal)
                {
                    camera.Set(camProperty, v, flagVal);
                    //Console.WriteLine($"{cameraName} {camProperty} set to {flagVal}");
                }
                else
                {
                    //Console.WriteLine($"{cameraName} {camProperty} already {flagVal}");
                }
            }
            else
            {
                //Console.WriteLine($"No physical camera matching \"{cameraName}\" found");
            }
        }

        private static void SetCameraValue(CameraControlProperty camProperty, int val)
        {
            if (camera != null)
            {
                // Get the current settings from the webcam
                camera.Get(camProperty, out int v, out CameraControlFlags f);

                // If the camera value differs from the desired value, adjust it leaving flag the same.
                if (v != val)
                {
                    camera.Set(camProperty, val, f);
                    //Console.WriteLine($"{cameraName} {camProperty} value set to {val}");
                }
                else
                {
                    //Console.WriteLine($"{cameraName} {camProperty} value already {val}");
                }
            }
            else
            {
                //Console.WriteLine($"No physical camera matching \"{cameraName}\" found");
            }
        }

        public bool AutoFocusEnabled
        {
            get { return (bool)GetValue(AutoFocusEnabledProperty); }
            set { SetValue(AutoFocusEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoFocusEnabledProperty =
            DependencyProperty.Register("AutoFocusEnabled", typeof(bool), typeof(ViewModel),
                new PropertyMetadata(new PropertyChangedCallback(OnAutoFocusEnabledChanged)));

        private static void OnAutoFocusEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool newVal = (bool)e.NewValue;
            CameraControlFlags flagSetting = newVal ? CameraControlFlags.Auto : CameraControlFlags.Manual;
            SetCameraFlag(CameraControlProperty.Focus, flagSetting);
        }

        public int FocusValue
        {
            get { return (int)GetValue(FocusValueProperty); }
            set { SetValue(FocusValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusValueProperty =
            DependencyProperty.Register("FocusValue", typeof(int), typeof(ViewModel),
                new PropertyMetadata(new PropertyChangedCallback(OnFocusValueChanged)));

        private static void OnFocusValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int newVal = (int)e.NewValue;
            SetCameraValue(CameraControlProperty.Focus, newVal);
        }

        public bool AutoExposureEnabled
        {
            get { return (bool)GetValue(AutoExposureEnabledProperty); }
            set { SetValue(AutoExposureEnabledProperty, value); }
        }
        public static readonly DependencyProperty AutoExposureEnabledProperty =
            DependencyProperty.Register("AutoExposureEnabled", typeof(bool), typeof(ViewModel),
                new PropertyMetadata(new PropertyChangedCallback(OnAutoExposureEnabledChanged)));

        private static void OnAutoExposureEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool newVal = (bool)e.NewValue;
            CameraControlFlags flagSetting = newVal ? CameraControlFlags.Auto : CameraControlFlags.Manual;
            SetCameraFlag(CameraControlProperty.Exposure, flagSetting);
        }

        public int ExposureValue
        {
            get { return (int)GetValue(ExposureValueProperty); }
            set { SetValue(ExposureValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExposureValueProperty =
            DependencyProperty.Register("ExposureValue", typeof(int), typeof(ViewModel),
                new PropertyMetadata(new PropertyChangedCallback(OnExposureValueChanged)));

        private static void OnExposureValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int newVal = (int)e.NewValue;
            SetCameraValue(CameraControlProperty.Exposure, newVal);
        }
    }
}
