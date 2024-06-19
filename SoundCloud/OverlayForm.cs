using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SoundCloudPlayer // Ensure this matches your project's namespace
{
    public class OverlayForm : Form
    {
        private Form1 mainForm;
        private Button playPauseButton;
        private Button nextButton; // Add button for next track
        private TrackBar volumeTrackBar; // Add track bar for volume control

        public OverlayForm(string imageUrl, string author, string title, Form1 form1)
        {
            mainForm = form1; // Store reference to Form1

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.ShowInTaskbar = false; // Hide from taskbar
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;

            // Resize the form to 0.8x its original size
            float scaleFactor = 0.6f;
            this.Size = new Size((int)(400 * scaleFactor), (int)(150 * scaleFactor));

            // Create a panel for the image
            Panel imagePanel = CreateRoundedPanel((int)(120 * scaleFactor), (int)(120 * scaleFactor)); // Increased size by 4px
            imagePanel.Location = new Point((int)(10 * scaleFactor), (int)(10 * scaleFactor));

            PictureBox pictureBox = new PictureBox
            {
                ImageLocation = imageUrl,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Black,
                Size = new Size((int)(120 * scaleFactor), (int)(120 * scaleFactor)), // Increased size by 4px
                Location = new Point(0, 0)
            };

            imagePanel.Controls.Add(pictureBox);

            int xOffset = (int)(120 * scaleFactor); // Adjust this value to change the horizontal offset from the image
            int xDistanceFromImage = (int)(15 * scaleFactor); // Adjust this value to change the distance between image and panels

            // Create a panel for the author
            Panel authorPanel = CreateRoundedPanel((int)(250 * scaleFactor), (int)(30 * scaleFactor));
            authorPanel.Location = new Point(xOffset + xDistanceFromImage, (int)(10 * scaleFactor)); // Adjust the x-coordinate here

            Label authorLabel = new Label
            {
                Text = $"{author}",
                Font = new Font("Segoe UI", 10 * scaleFactor, FontStyle.Bold), // Scale the font size
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point((int)(10 * scaleFactor), (int)(5 * scaleFactor)) // Position inside the panel
            };

            authorPanel.Controls.Add(authorLabel);

            // Create a panel for the title
            Panel titlePanel = CreateRoundedPanel((int)(250 * scaleFactor), (int)(30 * scaleFactor));
            titlePanel.Location = new Point(xOffset + xDistanceFromImage, (int)(50 * scaleFactor)); // Adjust the x-coordinate here

            Label titleLabel = new Label
            {
                Text = $"{title}",
                Font = new Font("Segoe UI", 10 * scaleFactor, FontStyle.Regular), // Scale the font size
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point((int)(10 * scaleFactor), (int)(5 * scaleFactor)) // Position inside the panel
            };

            titlePanel.Controls.Add(titleLabel);

            // Create a panel for the controls
            Panel controlsPanel = CreateRoundedPanel((int)(250 * scaleFactor), (int)(40 * scaleFactor));
            controlsPanel.Location = new Point(xOffset + xDistanceFromImage, (int)(90 * scaleFactor)); // Adjust the x-coordinate here

            playPauseButton = new Button
            {
                Size = new Size((int)(30 * scaleFactor), (int)(30 * scaleFactor)),
                Location = new Point((int)(10 * scaleFactor), (int)(5 * scaleFactor)), // Position inside the panel
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Image = ResizeImage(global::SoundCloud.Properties.Resources.pause_icon, (int)(25 * scaleFactor), (int)(25 * scaleFactor)), // Resize image to fit within the button
                ImageAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0)
            };
            playPauseButton.FlatAppearance.BorderSize = 0;
            playPauseButton.FlatAppearance.MouseOverBackColor = Color.Transparent; // Remove hover effect
            playPauseButton.FlatAppearance.MouseDownBackColor = Color.Transparent; // Remove click effect
            playPauseButton.Click += PlayPauseButton_Click; // Attach event handler

            nextButton = new Button
            {
                Size = new Size((int)(30 * scaleFactor), (int)(30 * scaleFactor)),
                Location = new Point((int)(50 * scaleFactor), (int)(5 * scaleFactor)), // Position to the right of the play/pause button
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Image = ResizeImage(global::SoundCloud.Properties.Resources.next1_icon, (int)(25 * scaleFactor), (int)(25 * scaleFactor)), // Resize image to fit within the button
                ImageAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0)
            };
            nextButton.FlatAppearance.BorderSize = 0;
            nextButton.FlatAppearance.MouseOverBackColor = Color.Transparent; // Remove hover effect
            nextButton.FlatAppearance.MouseDownBackColor = Color.Transparent; // Remove click effect
            nextButton.Click += NextButton_Click; // Attach event handler

            volumeTrackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 100, // Default volume to 100%
                TickFrequency = 10,
                Size = new Size((int)(150 * scaleFactor), (int)(30 * scaleFactor)), // Make volume bar smaller
                Location = new Point((int)(90 * scaleFactor), (int)(5 * scaleFactor)) // Position to the right of the next button
            };
            volumeTrackBar.Scroll += VolumeTrackBar_Scroll; // Attach event handler

            controlsPanel.Controls.Add(playPauseButton);
            controlsPanel.Controls.Add(nextButton);
            controlsPanel.Controls.Add(volumeTrackBar);

            this.Controls.Add(imagePanel);
            this.Controls.Add(authorPanel);
            this.Controls.Add(titlePanel);
            this.Controls.Add(controlsPanel); // Add controls panel to the form
        }

        private Panel CreateRoundedPanel(int width, int height)
        {
            Panel panel = new Panel
            {
                Size = new Size(width, height),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.None
            };
            panel.Paint += (sender, e) =>
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(0, 0, 20, 20, 180, 90);
                    path.AddArc(width - 20, 0, 20, 20, 270, 90);
                    path.AddArc(width - 20, height - 20, 20, 20, 0, 90);
                    path.AddArc(0, height - 20, 20, 20, 90, 90);
                    path.CloseAllFigures();

                    panel.Region = new Region(path);

                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(new Pen(Color.Black, 1), path); // Remove the white border by setting it to the panel's background color
                }
            };
            return panel;
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }

        private void PlayPauseButton_Click(object sender, EventArgs e)
        {
            mainForm.TogglePlayPause(); // Call method in Form1
            UpdatePlayPauseIcon();
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            mainForm.buttonNext_Click(sender, e); // Call method in Form1
        }

        private void VolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            mainForm.UpdateVolume(volumeTrackBar.Value); // Call method in Form1
        }

        public void UpdatePlayPauseIcon()
        {
            if (mainForm.IsPlaying)
            {
                playPauseButton.Image = ResizeImage(global::SoundCloud.Properties.Resources.pause_icon, playPauseButton.Width - 5, playPauseButton.Height - 5); // Resize image to fit within the button
            }
            else
            {
                playPauseButton.Image = ResizeImage(global::SoundCloud.Properties.Resources.play_icon, playPauseButton.Width - 5, playPauseButton.Height - 5); // Resize image to fit within the button
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Position the overlay 20px from the left side and at the bottom of the screen
            this.Location = new Point(2, Screen.PrimaryScreen.WorkingArea.Height - this.Height);
        }
    }
}
