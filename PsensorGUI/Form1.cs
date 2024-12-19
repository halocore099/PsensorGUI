using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using LibreHardwareMonitor.Hardware;

namespace PsensorGUI
{
    public partial class Form1 : Form
    {
        // UI Controls
        private Timer _timer;
        private int _timeElapsed;
        private Label _timeLabel;
        private GroupBox _performanceGroup;
        private GroupBox _temperatureGroup;
        private ColoredProgressBar _cpuProgressBar;
        private ColoredProgressBar _memoryProgressBar;
        private ColoredProgressBar _gpuProgressBar;
        private ColoredProgressBar _diskProgressBar;
        private Label _cpuTempLabel;
        private Label _gpuTempLabel;
        private Button _startButton;

        // Performance Monitoring
        private PerformanceCounter _cpuCounter;
        private Computer _computer;

        public Form1()
        {
            InitializeComponent();
            _timer = new Timer(); // Initialize _timer
            _timeLabel = new Label(); // Initialize _timeLabel
            _performanceGroup = new GroupBox(); // Initialize _performanceGroup
            _temperatureGroup = new GroupBox(); // Initialize _temperatureGroup
            _cpuProgressBar = new ColoredProgressBar(); // Initialize _cpuProgressBar
            _memoryProgressBar = new ColoredProgressBar(); // Initialize _memoryProgressBar
            _gpuProgressBar = new ColoredProgressBar(); // Initialize _gpuProgressBar
            _diskProgressBar = new ColoredProgressBar(); // Initialize _diskProgressBar
            _cpuTempLabel = new Label(); // Initialize _cpuTempLabel
            _gpuTempLabel = new Label(); // Initialize _gpuTempLabel
            _startButton = new Button(); // Initialize _startButton
            _cpuCounter = new PerformanceCounter(); // Initialize _cpuCounter
            _computer = new Computer(); // Initialize _computer
            SetupMainForm();
            InitializeControls();
            InitializePerformanceMonitoring();
        }

        private void SetupMainForm()
        {
            this.Text = "System Performance Monitor";
            this.Size = new Size(800, 500); // Increased width to 800 and height to 500
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }
        

        #region Controls Initialization
        private void InitializeControls()
        {
            InitializeGroups();
            InitializeLabels();
            InitializeProgressBars();
            InitializeTimer();
            InitializeStartButton();

            // Add main controls to form
            this.Controls.AddRange(new Control[] { _performanceGroup, _temperatureGroup, _startButton });
        }

        private void InitializeGroups()
        {
            _performanceGroup = new GroupBox
            {
                Text = "Performance Metrics",
                Location = new Point(20, 20),
                Size = new Size(740, 200), // Increased width to 740
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            _temperatureGroup = new GroupBox
            {
                Text = "Temperature Metrics",
                Location = new Point(20, 230),
                Size = new Size(740, 80), // Increased width to 740
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
        }

        private void InitializeLabels()
        {
            _timeLabel = CreateLabel("Time Elapsed: 0s", 20, 20, _performanceGroup);
            _cpuTempLabel = CreateLabel("CPU: 0°C", 30, 30, _temperatureGroup);
            _gpuTempLabel = CreateLabel("GPU: 0°C", 280, 30, _temperatureGroup);
        }

        private Label CreateLabel(string text, int x, int y, Control parent)
        {
            var label = new Label
            {
                Location = new Point(x, y),
                Size = new Size(200, 20),
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            parent.Controls.Add(label);
            return label;
        }

        private void InitializeProgressBars()
        {
            _cpuProgressBar = CreateProgressBar(20, 50, "CPU Usage", _performanceGroup);
            _memoryProgressBar = CreateProgressBar(20, 90, "Memory Usage", _performanceGroup);
            _gpuProgressBar = CreateProgressBar(20, 130, "GPU Usage", _performanceGroup);
            _diskProgressBar = CreateProgressBar(20, 170, "Disk Usage", _performanceGroup);
        }

        public class ColoredProgressBar : ProgressBar
        {
            public ColoredProgressBar()
            {
                this.SetStyle(ControlStyles.UserPaint, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Rectangle rec = e.ClipRectangle;
                rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4;
                if (ProgressBarRenderer.IsSupported)
                    ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);
                rec.Height = rec.Height - 4;
                e.Graphics.FillRectangle(new SolidBrush(ForeColor), 2, 2, rec.Width, rec.Height);
            }
        }

        private ColoredProgressBar CreateProgressBar(int x, int y, string labelText, Control parent)
        {
            var progressBar = new ColoredProgressBar
            {
                Location = new Point(x, y),
                Size = new Size(500, 20), // Fixed width to 500
                Maximum = 100,
                ForeColor = Color.FromArgb(0, 164, 0),
                Tag = CreateLabel(labelText, x + 510, y, parent) // Adjusted label position
            };
            parent.Controls.Add(progressBar);
            return progressBar;
        }

        private void InitializeTimer()
        {
            _timer = new Timer { Interval = 1000 };
            _timer.Tick += Timer_Tick;
        }

        private void InitializeStartButton()
        {
            _startButton = new Button
            {
                Text = "Start Monitoring",
                Location = new Point(20, 320),
                Size = new Size(540, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _startButton.Click += StartButton_Click;
        }
        #endregion

        #region Performance Monitoring
        private void InitializePerformanceMonitoring()
        {
            InitializePerformanceCounters();
            InitializeHardwareMonitor();
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor Information", "% Processor Time", "_Total", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing performance counters: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeHardwareMonitor()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsStorageEnabled = true
            };
            _computer.Open();
        }

        private void UpdateProgressBar(ColoredProgressBar progressBar, float value)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(() => UpdateProgressBar(progressBar, value)));
                return;
            }

            progressBar.Value = (int)Math.Min(100, Math.Max(0, value));
            var label = progressBar.Tag as Label;
            if (label != null)
            {
                label.Text = $"{progressBar.Value}%";
            }

            // Update color based on value
            progressBar.ForeColor = value switch
            {
                < 60 => Color.FromArgb(0, 164, 0),    // Green
                < 85 => Color.FromArgb(255, 164, 0),  // Orange
                _ => Color.FromArgb(232, 17, 35)      // Red
            };
        }
        #endregion

        #region Event Handlers
        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timeElapsed++;
            _timeLabel.Text = $"Time Elapsed: {_timeElapsed}s";

            try
            {
                // Update CPU Usage
                float cpuUsage = _cpuCounter.NextValue();
                UpdateProgressBar(_cpuProgressBar, cpuUsage);

                // Update Memory Usage
                var totalPhys = new PerformanceCounter("Memory", "Committed Bytes");
                var availPhys = new PerformanceCounter("Memory", "Available Bytes");
                float memoryUsage = ((totalPhys.NextValue() - availPhys.NextValue()) / totalPhys.NextValue()) * 100;
                UpdateProgressBar(_memoryProgressBar, memoryUsage);

                // Update Hardware Info
                foreach (var hardware in _computer.Hardware)
                {
                    hardware.Update();
                    switch (hardware.HardwareType)
                    {
                        case HardwareType.Cpu:
                            var cpuTemp = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
                            if (cpuTemp?.Value.HasValue == true)
                                _cpuTempLabel.Text = $"CPU: {cpuTemp.Value:F1}°C";
                            break;

                        case HardwareType.GpuNvidia:
                        case HardwareType.GpuAmd:
                            var gpuLoad = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);
                            var gpuTemp = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);

                            if (gpuLoad?.Value.HasValue == true)
                                UpdateProgressBar(_gpuProgressBar, gpuLoad.Value.Value);
                            if (gpuTemp?.Value.HasValue == true)
                                _gpuTempLabel.Text = $"GPU: {gpuTemp.Value:F1}°C";
                            break;

                        case HardwareType.Storage:
                            var diskLoad = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);
                            if (diskLoad?.Value.HasValue == true)
                                UpdateProgressBar(_diskProgressBar, diskLoad.Value.Value);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _timer.Stop();
                MessageBox.Show($"Error updating sensors: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _startButton.Text = "Start Monitoring";
            }
        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
                _startButton.Text = "Start Monitoring";
                _startButton.BackColor = Color.FromArgb(0, 120, 212);
            }
            else
            {
                _timer.Start();
                _startButton.Text = "Stop Monitoring";
                _startButton.BackColor = Color.FromArgb(232, 17, 35);
            }
        }
        #endregion

        #region Cleanup
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _computer?.Close();
            _cpuCounter?.Dispose();
        }
        #endregion
    }

    public class ColoredProgressBar : ProgressBar
    {
        public ColoredProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rec = e.ClipRectangle;
            rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4;
            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);
            rec.Height = rec.Height - 4;
            e.Graphics.FillRectangle(new SolidBrush(ForeColor), 2, 2, rec.Width, rec.Height);
        }
    }
}
