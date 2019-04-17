using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using DirectShowLib;
using System.Threading;
using AForge.Video.DirectShow;

namespace CameraViewer {
    public partial class CameraAdjustments : UserControl {
        public Camera camera;
        int camId = 0;

        bool isLoaded = false;
        public CameraAdjustments() {
            InitializeComponent();
            isLoaded = true;
        }

        public void InitializeVariables(Camera thisCamera, int id) {
            camera = thisCamera;
            camId = id;

            GetCamList();

            cbMirror1.Checked = Program.Settings.Cam[camId].CamMirror;
            cbDrawGrid1.Checked = Program.Settings.Cam[camId].DrawGrid;
            cbGrayscale.Checked = Program.Settings.Cam[camId].grayScale;
            cbInvert.Checked = Program.Settings.Cam[camId].Invert;
            cbEdgeDetect.Checked = Program.Settings.Cam[camId].EdgeDetect;
            cbNoiseReduction.Checked = Program.Settings.Cam[camId].NoiseReduction;
            cbThreshold.Checked = Program.Settings.Cam[camId].Threshold;
            cbContrast.Checked = Program.Settings.Cam[camId].Contrast;
            cbZoom.Checked = Program.Settings.Cam[camId].Zoom;

            nGridInterval.Value = Program.Settings.Cam[camId].GridSpacing < 2 ? 2 : Program.Settings.Cam[camId].GridSpacing;
            cbEdgeDetection.SelectedIndex = Program.Settings.Cam[camId].EdgeDetectVal;
            cbRotationAmount.SelectedIndex = Program.Settings.Cam[camId].CamRotateValue;
            cbResolution.SelectedIndex = Program.Settings.Cam[camId].ResolutionValue;
            cbNoiseReduce.SelectedIndex = Program.Settings.Cam[camId].NoiseReductionVal;
            nThreshold.Value = Program.Settings.Cam[camId].ThresholdVal;
            nZoom.Value = Program.Settings.Cam[camId].ZoomVal;
            lblSaveFileLocation.Text = string.IsNullOrWhiteSpace(Program.Settings.Cam[camId].SaveLocation) ? "Save Location..." : Program.Settings.Cam[camId].SaveLocation.Substring(Program.Settings.Cam[camId].SaveLocation.Length < 50 ? 0 : Program.Settings.Cam[camId].SaveLocation.Length - 50);
        }

        internal void UpdateEyeTracking() {
            if (camera.eyeTracker.recordingPoints) {
                lblInitializeStatus.Text = $"point {camera.eyeTracker.xCal + 1 * camera.eyeTracker.yCal + 1} of {Eyetracking.maxSize}";
            }
            else {
                lblInitializeStatus.Text = $"Eye tracking initialized";
            }

            lblEyeTrackingInfo.Text = $"x = {camera.eyeTracker.TrackedXVal}  y = {camera.eyeTracker.TrackedYVal}";
        }

        // =================================================================================
        // get the devices         
        private void GetCamList() {
            List<string> Devices = camera.GetDeviceList();
            XCam_comboBox.Items.Clear();
            if (Devices.Count != 0)
            {
                for (int i = 0; i < Devices.Count; i++)
                {
                    XCam_comboBox.Items.Add(i.ToString() + ": " + Devices[i]);
                }
            }
            else
            {
                XCam_comboBox.Items.Add("----");
                XCamStatus_label.Text = "No Cam";
            }

            if ((Devices.Count > Program.Settings.Cam[camId].CamIndex) && (Program.Settings.Cam[camId].CamIndex > 0)) {
                XCam_comboBox.SelectedIndex = Program.Settings.Cam[camId].CamIndex;
            }
            else {
                XCam_comboBox.SelectedIndex = 0;  
            }
        }

        private void xCamSelect_Click(object sender, EventArgs e) {
            Program.Settings.Cam[camId].CamIndex = XCam_comboBox.SelectedIndex;

            if (camera.Active) {
                camera.SignalToStop();
                Thread.Sleep(50);
                camera.Active = false;

                xCamSelect.Text = "Go";
            }
            else
            {
                List<string> Monikers = camera.GetMonikerStrings();

                Program.Settings.Cam[camId].CamMoniker = Monikers[XCam_comboBox.SelectedIndex];
                AppSettings<Program.MySettings>.Save(Program.Settings);

                camera.MonikerString = Monikers[XCam_comboBox.SelectedIndex];

                camera.Start("DownCamera", Monikers[XCam_comboBox.SelectedIndex], camId);

                if (!camera.ReceivingFrames)
                {
                    MessageBox.Show("Camera being used by another process");
                }
                else
                {
                    xCamSelect.Text = "Stop";
                    camera.Active = true;
                }
            }
        }

        private void cbRotationAmount_SelectedIndexChanged(object sender, EventArgs e) {
            try {camera.rotateAmount = cbRotationAmount.SelectedIndex; } catch { }

            Program.Settings.Cam[camId].CamRotateValue = cbRotationAmount.SelectedIndex;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.Settings.Cam[camId].ResolutionValue = cbResolution.SelectedIndex;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbMirror1_CheckedChanged(object sender, EventArgs e) {
            try { camera.Mirror = cbMirror1.Checked; } catch { }
            Program.Settings.Cam[camId].CamMirror = cbMirror1.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbDrawGrid1_CheckedChanged(object sender, EventArgs e) {
            try { camera.DrawGrid = cbDrawGrid1.Checked; } catch { }
            Program.Settings.Cam[camId].DrawGrid = cbDrawGrid1.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbGrayscale_CheckedChanged(object sender, EventArgs e) {
            try { camera.GrayScale = cbGrayscale.Checked; } catch { }
            Program.Settings.Cam[camId].grayScale = cbGrayscale.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbInvert_CheckedChanged(object sender, EventArgs e) {
            try { camera.Invert = cbInvert.Checked; } catch { }
            Program.Settings.Cam[camId].Invert = cbInvert.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbEdgeDetect_CheckedChanged(object sender, EventArgs e) {
            try { camera.EdgeDetect = cbEdgeDetect.Checked; } catch { }

            Program.Settings.Cam[camId].EdgeDetect = cbEdgeDetect.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbNoiseReduction_CheckedChanged(object sender, EventArgs e) {
            try { camera.NoiseReduce = cbNoiseReduction.Checked; } catch { }
            Program.Settings.Cam[camId].NoiseReduction = cbNoiseReduction.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbThreshold_CheckedChanged(object sender, EventArgs e) {
            try { camera.Threshold = cbThreshold.Checked; } catch { }
            Program.Settings.Cam[camId].Threshold = cbThreshold.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbContrast_CheckedChanged(object sender, EventArgs e) {
            try { camera.Contrast = cbContrast.Checked; } catch { }
            Program.Settings.Cam[camId].Contrast = cbContrast.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbZoom_CheckedChanged(object sender, EventArgs e) {
            try { camera.Zoom = cbZoom.Checked; } catch { }
            Program.Settings.Cam[camId].Zoom = cbZoom.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        // values
        private void nGridInterval_ValueChanged(object sender, EventArgs e) {
            try { camera.GridIncrement = (int)nGridInterval.Value; } catch { }
            Program.Settings.Cam[camId].GridSpacing = camera.GridIncrement;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbEdgeDetection_SelectedIndexChanged(object sender, EventArgs e) {
            try { camera.EdgeDetectValue = (int)cbEdgeDetection.SelectedIndex; } catch { }
            Program.Settings.Cam[camId].EdgeDetectVal = camera.EdgeDetectValue;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbNoiseReduce_SelectedIndexChanged(object sender, EventArgs e) {
            try { camera.NoiseReduceValue = (int)cbNoiseReduce.SelectedIndex; } catch { }
            Program.Settings.Cam[camId].NoiseReductionVal = camera.NoiseReduceValue;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void nThreshold_ValueChanged(object sender, EventArgs e) {
            try { camera.ThresholdValue = (int)nThreshold.Value; } catch { }
            Program.Settings.Cam[camId].ThresholdVal = camera.ThresholdValue;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void nZoom_ValueChanged(object sender, EventArgs e) {
            try { camera.ZoomValue = (int)nZoom.Value; } catch { }
            Program.Settings.Cam[camId].GridSpacing = camera.ZoomValue;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void bSetSaveLocation_Click(object sender, EventArgs e) {
            var directoryDialog = new CommonOpenFileDialog {
                IsFolderPicker = true,
                Title = "Select Folder"
            };

            directoryDialog.ShowDialog();
            try {
                if (!string.IsNullOrWhiteSpace(directoryDialog.FileName)) {
                    Program.Settings.Cam[camId].SaveLocation = directoryDialog.FileName;
                    lblSaveFileLocation.Text = string.IsNullOrWhiteSpace(Program.Settings.Cam[camId].SaveLocation) ? "Save Location..." : Program.Settings.Cam[camId].SaveLocation.Substring(Program.Settings.Cam[camId].SaveLocation.Length < 50 ? 0 : Program.Settings.Cam[camId].SaveLocation.Length - 50);
                    AppSettings<Program.MySettings>.Save(Program.Settings);
                }
            }
            catch { }
        }


        private void cbLockExposure_CheckedChanged(object sender, EventArgs e) {
            if (!camera.Active) {
                return;
            }

            if (!isLoaded) {
                return;
            }

            if (cbLockExposure.Checked) {
                int g = 0;
                AForge.Video.DirectShow.CameraControlFlags controlFlags;
                // max -9 min 0
                camera.VideoSource.GetCameraProperty(AForge.Video.DirectShow.CameraControlProperty.Exposure, out g, out controlFlags);
                camera.VideoSource.SetCameraProperty(AForge.Video.DirectShow.CameraControlProperty.Exposure, g, AForge.Video.DirectShow.CameraControlFlags.Manual);
                camera.VideoSource.GetCameraProperty(AForge.Video.DirectShow.CameraControlProperty.Exposure, out g, out controlFlags);
                trackBarExposure.Value = g * -1 + 1;
                trackBarExposure.Visible = true;
            }
            else {
                camera.VideoSource.SetCameraProperty(AForge.Video.DirectShow.CameraControlProperty.Exposure, 10, AForge.Video.DirectShow.CameraControlFlags.Auto);
                trackBarExposure.Visible = false;
            }
        }

        private void trackBarExposure_Scroll(object sender, EventArgs e) {
            if (!isLoaded) {
                return;
            }
            camera.VideoSource.SetCameraProperty(AForge.Video.DirectShow.CameraControlProperty.Exposure, (trackBarExposure.Value - 1) * -1, AForge.Video.DirectShow.CameraControlFlags.Manual);
        }

        private void trackBar1_Scroll(object sender, EventArgs e) {
            camera.targetFramesSecond = trackBar1.Value;
            lblFrameRate.Text = $"Framerate tuning: {camera.targetFramesSecond}";
        }

        private void cbLineDetection_CheckedChanged(object sender, EventArgs e) {
            camera.ShapeVariables.calcLines = cbLineDetection.Checked;
        }

        private void cbCircleDetection_CheckedChanged(object sender, EventArgs e) {
            camera.ShapeVariables.calcCircles = cbCircleDetection.Checked;
        }

        private void cbRectangleTriDetection_CheckedChanged(object sender, EventArgs e) {
            camera.ShapeVariables.calcRectTri = cbRectangleTriDetection.Checked;
        }

        private void nlineCanny_ValueChanged(object sender, EventArgs e) {
            camera.ShapeVariables.lineCannyThreshold = (double)nlineCanny.Value;
        }

        private void nLineThreshold_ValueChanged(object sender, EventArgs e) {
            camera.ShapeVariables.lineThreshold = (int)nLineThreshold.Value;
        }

        private void nThresholdLinking_ValueChanged(object sender, EventArgs e) {
            camera.ShapeVariables.cannyThresholdLinking = (double)nThresholdLinking.Value;
        }

        private void nMinLineWidth_ValueChanged(object sender, EventArgs e) {
            camera.ShapeVariables.minLineWidth = (double)nMinLineWidth.Value;
        }

        private void nMinRadius_ValueChanged(object sender, EventArgs e) {
            camera.ShapeVariables.minradius = (int)nMinRadius.Value;
        }

        private void nMaxRadius_ValueChanged(object sender, EventArgs e) {
            camera.ShapeVariables.maxRadius = (int)nMaxRadius.Value;
        }

        private void nCircleCanny_ValueChanged(object sender, EventArgs e) {
            camera.ShapeVariables.circleCannyThreshold = (int)nCircleCanny.Value;
        }

        private void nCircleAccumulator_ValueChanged(object sender, EventArgs e) {
            camera.ShapeVariables.circleAccumulatorThreshold = (int)nCircleAccumulator.Value;
        }

        private void cbVisualFlow_CheckedChanged(object sender, EventArgs e) {
            camera.optiVariables.calcOpticalFlow = cbVisualFlow.Checked;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e) {
            camera.optiVariables.stepRate = (int)nFlowDensity.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e) {
            camera.optiVariables.frameReduction = (int)((int)nResolutionReduction.Value % 2 == 1 ? nResolutionReduction.Value - 1 : nResolutionReduction.Value);
        }

        private void numericUpDown1_ValueChanged_1(object sender, EventArgs e) {
            camera.optiVariables.shiftThatCounts = (int)numericUpDown1.Value;
        }

        private void cbEyeTracking_CheckedChanged(object sender, EventArgs e) {

        }

        // start the initialization
        private void bInitialize_Click(object sender, EventArgs e) {
            if (!camera.eyeTracker.Initialize()) {
                MessageBox.Show("please set eye tracking camera");
                return;
            }

        }

        private void cbIsEyeCam_CheckedChanged(object sender, EventArgs e) {
            camera.eyeTracker.SetCameraPosition(camera);
        }

        private void bReset_Click(object sender, EventArgs e) {
            camera.eyeTracker.Initialize(true);
        }

        private void cbR_CheckedChanged(object sender, EventArgs e) {
            try { camera.R = cbR.Checked; } catch { }

            Program.Settings.Cam[camId].R = cbR.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbG_CheckedChanged(object sender, EventArgs e) {
            try { camera.G = cbG.Checked; } catch { }
            Program.Settings.Cam[camId].G = cbG.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void cbB_CheckedChanged(object sender, EventArgs e) {
            try { camera.B = cbB.Checked; } catch { }
            Program.Settings.Cam[camId].B = cbB.Checked;
            AppSettings<Program.MySettings>.Save(Program.Settings);
        }

        private void UpdateResolutionsCombobox()
        {
            // Set available resolutions
            cbResolution.Items.Clear();

            List<string> Monikers = camera.GetMonikerStrings();
            var Moniker = Monikers[XCam_comboBox.SelectedIndex];
            var VideoSource = new VideoCaptureDevice(Moniker);

            foreach (var cap in VideoSource.VideoCapabilities)
            {
                cbResolution.Items.Add(cap.FrameSize.Width.ToString() + " x " + cap.FrameSize.Height.ToString());
            }

            try
            {
                cbResolution.SelectedIndex = Program.Settings.Cam[camId].ResolutionValue;
            }
            catch { }
        }

        private void XCam_comboBox_SelectedIndexChanged(object sender, EventArgs e) {
            Program.Settings.Cam[camId].CamIndex = XCam_comboBox.SelectedIndex;
            AppSettings<Program.MySettings>.Save(Program.Settings);

            UpdateResolutionsCombobox();
        }

        private void tbEyeRectX_KeyPress(object sender, KeyPressEventArgs e) {
            if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back)))
                e.Handled = true;
        }

        private void tbEyeRectY_KeyPress(object sender, KeyPressEventArgs e) {
            if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back)))
                e.Handled = true;
        }

        private void tbEyeSX_KeyPress(object sender, KeyPressEventArgs e) {
            if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back)))
                e.Handled = true;
        }

        private void tbEyeSY_KeyPress(object sender, KeyPressEventArgs e) {
            if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back)))
                e.Handled = true;
        }

        private void nEyeTrackingTuningX_ValueChanged(object sender, EventArgs e) {
            camera.eyeTracker.xTune = (int)nEyeTrackingTuningX.Value;
        }

        private void nEyeTrackingTuningY_ValueChanged(object sender, EventArgs e) {
            camera.eyeTracker.yTune = (int)nEyeTrackingTuningY.Value;
        }

        private void tbEyeRectX_TextChanged(object sender, EventArgs e) {
            try {
                camera.eyeTracker.rectX = int.Parse(tbEyeRectX.Text);
            }
            catch { }
        }

        private void tbEyeRectY_TextChanged(object sender, EventArgs e) {
            try {
                camera.eyeTracker.rectY = int.Parse(tbEyeRectY.Text);
            }
            catch { }
        }

        private void tbEyeSX_TextChanged(object sender, EventArgs e) {
            try {
                camera.eyeTracker.rectHeight = int.Parse(tbEyeSX.Text);
            }
            catch { }
        }

        private void tbEyeSY_TextChanged(object sender, EventArgs e) {
            try {
                camera.eyeTracker.rectWidth = int.Parse(tbEyeSY.Text);
            }
            catch { }
        }
    }
}
