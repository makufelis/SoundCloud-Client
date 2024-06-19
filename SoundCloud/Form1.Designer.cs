using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SoundCloudPlayer
{
    partial class Form1
    {
        private System.Windows.Forms.PictureBox pictureBoxThumb;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelCurrentTime; // Added label for current track time
        private System.Windows.Forms.Label labelTotalTime; // Added label for total track time
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox listBoxTracks;
        private System.Windows.Forms.TextBox textBoxUrl;
        private System.Windows.Forms.TrackBar trackBarVolume;
        private System.Windows.Forms.Button buttonLoadCsv;
        private Panel seekBarPanel; // Panel control for the seek bar
        private int seekBarPosition = 0; // Position of the seek bar
        private Panel progressPanel;
        private NoHorizontalScrollPanel playlistPanel; // Replace Panel with NoHorizontalScrollPanel
        private NoHorizontalScrollPanel historyPanel; // Use the custom panel here
        private Button buttonCreatePlaylist; // Add this line
        private Button buttonToggleOverlay; // Add this line
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.listBoxTracks = new System.Windows.Forms.ListBox();
            this.textBoxUrl = new System.Windows.Forms.TextBox();
            this.trackBarVolume = new System.Windows.Forms.TrackBar();
            this.buttonLoadCsv = new System.Windows.Forms.Button();
            this.labelAuthor = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelCurrentTime = new System.Windows.Forms.Label();
            this.labelTotalTime = new System.Windows.Forms.Label();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonLoop = new System.Windows.Forms.Button();
            this.buttonPrevious = new System.Windows.Forms.Button();
            this.buttonRandom = new System.Windows.Forms.Button();
            this.buttonTogglePlayPause = new System.Windows.Forms.Button();
            this.seekBarPanel = new System.Windows.Forms.Panel();
            this.progressPanel = new System.Windows.Forms.Panel();
            this.buttonCreatePlaylist = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.ButtonOpenSearchForm = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonPlaylistPage = new System.Windows.Forms.Button();
            this.buttonToggleOverlay = new System.Windows.Forms.Button(); // Add this line
            this.pictureBoxThumb = new System.Windows.Forms.PictureBox();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).BeginInit();
            this.tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // listBoxTracks
            // 
            this.listBoxTracks.BackColor = System.Drawing.Color.Black;
            this.listBoxTracks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBoxTracks.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxTracks.FormattingEnabled = true;
            this.listBoxTracks.Location = new System.Drawing.Point(444, 13);
            this.listBoxTracks.Name = "listBoxTracks";
            this.listBoxTracks.Size = new System.Drawing.Size(439, 390);
            this.listBoxTracks.TabIndex = 0;
            this.listBoxTracks.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBoxTracks_DrawItem);
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.textBoxUrl.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxUrl.ForeColor = System.Drawing.Color.White;
            this.textBoxUrl.Location = new System.Drawing.Point(519, 415);
            this.textBoxUrl.Multiline = true;
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(364, 44);
            this.textBoxUrl.TabIndex = 7;
            this.textBoxUrl.Visible = false;
            // 
            // trackBarVolume
            // 
            this.trackBarVolume.Location = new System.Drawing.Point(444, 415);
            this.trackBarVolume.Maximum = 100;
            this.trackBarVolume.Name = "trackBarVolume";
            this.trackBarVolume.Size = new System.Drawing.Size(437, 45);
            this.trackBarVolume.TabIndex = 8;
            this.trackBarVolume.TickFrequency = 10;
            this.trackBarVolume.Scroll += new System.EventHandler(this.trackBarVolume_Scroll);
            // 
            // buttonLoadCsv
            // 
            this.buttonLoadCsv.BackColor = System.Drawing.Color.Black;
            this.buttonLoadCsv.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonLoadCsv.ForeColor = System.Drawing.Color.White;
            this.buttonLoadCsv.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonLoadCsv.Location = new System.Drawing.Point(783, 363);
            this.buttonLoadCsv.Name = "buttonLoadCsv";
            this.buttonLoadCsv.Size = new System.Drawing.Size(100, 40);
            this.buttonLoadCsv.TabIndex = 6;
            this.buttonLoadCsv.Text = "Load CSV";
            this.buttonLoadCsv.UseVisualStyleBackColor = false;
            this.buttonLoadCsv.Visible = false;
            this.buttonLoadCsv.Click += new System.EventHandler(this.buttonLoadCsv_Click);
            // 
            // labelAuthor
            // 
            this.labelAuthor.AutoSize = true;
            this.labelAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAuthor.ForeColor = System.Drawing.Color.White;
            this.labelAuthor.Location = new System.Drawing.Point(72, 344);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new System.Drawing.Size(52, 13);
            this.labelAuthor.TabIndex = 11;
            this.labelAuthor.Text = "Author: ";
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(72, 357);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(33, 13);
            this.labelTitle.TabIndex = 12;
            this.labelTitle.Text = "Title: ";
            // 
            // labelCurrentTime
            // 
            this.labelCurrentTime.AutoSize = true;
            this.labelCurrentTime.ForeColor = System.Drawing.Color.White;
            this.labelCurrentTime.Location = new System.Drawing.Point(57, 372);
            this.labelCurrentTime.Name = "labelCurrentTime";
            this.labelCurrentTime.Size = new System.Drawing.Size(0, 13);
            this.labelCurrentTime.TabIndex = 13;
            // 
            // labelTotalTime
            // 
            this.labelTotalTime.AutoSize = true;
            this.labelTotalTime.ForeColor = System.Drawing.Color.White;
            this.labelTotalTime.Location = new System.Drawing.Point(393, 372);
            this.labelTotalTime.Name = "labelTotalTime";
            this.labelTotalTime.Size = new System.Drawing.Size(0, 13);
            this.labelTotalTime.TabIndex = 14;
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 5;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel.Controls.Add(this.buttonNext, 3, 0);
            this.tableLayoutPanel.Controls.Add(this.buttonLoop, 4, 0);
            this.tableLayoutPanel.Controls.Add(this.buttonPrevious, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.buttonRandom, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.buttonTogglePlayPause, 2, 0);
            this.tableLayoutPanel.Location = new System.Drawing.Point(57, 405);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 1;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(362, 64);
            this.tableLayoutPanel.TabIndex = 9;
            // 
            // buttonNext
            // 
            this.buttonNext.Cursor = System.Windows.Forms.Cursors.Default;
            this.buttonNext.FlatAppearance.BorderSize = 0;
            this.buttonNext.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonNext.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonNext.Image = global::SoundCloud.Properties.Resources.next1_icon;
            this.buttonNext.Location = new System.Drawing.Point(219, 3);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(64, 58);
            this.buttonNext.TabIndex = 4;
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonLoop
            // 
            this.buttonLoop.FlatAppearance.BorderSize = 0;
            this.buttonLoop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonLoop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonLoop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonLoop.Image = ((System.Drawing.Image)(resources.GetObject("buttonLoop.Image")));
            this.buttonLoop.Location = new System.Drawing.Point(291, 3);
            this.buttonLoop.Name = "buttonLoop";
            this.buttonLoop.Size = new System.Drawing.Size(64, 58);
            this.buttonLoop.TabIndex = 5;
            this.buttonLoop.UseVisualStyleBackColor = true;
            this.buttonLoop.Click += new System.EventHandler(this.buttonLoop_Click);
            // 
            // buttonPrevious
            // 
            this.buttonPrevious.FlatAppearance.BorderSize = 0;
            this.buttonPrevious.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonPrevious.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPrevious.ForeColor = System.Drawing.Color.Transparent;
            this.buttonPrevious.Image = global::SoundCloud.Properties.Resources.previous1_icon;
            this.buttonPrevious.Location = new System.Drawing.Point(75, 3);
            this.buttonPrevious.Name = "buttonPrevious";
            this.buttonPrevious.Size = new System.Drawing.Size(64, 58);
            this.buttonPrevious.TabIndex = 3;
            this.buttonPrevious.UseVisualStyleBackColor = true;
            this.buttonPrevious.Click += new System.EventHandler(this.buttonPrevious_Click);
            // 
            // buttonRandom
            // 
            this.buttonRandom.FlatAppearance.BorderSize = 0;
            this.buttonRandom.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonRandom.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonRandom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRandom.Image = ((System.Drawing.Image)(resources.GetObject("buttonRandom.Image")));
            this.buttonRandom.Location = new System.Drawing.Point(3, 3);
            this.buttonRandom.Name = "buttonRandom";
            this.buttonRandom.Size = new System.Drawing.Size(64, 58);
            this.buttonRandom.TabIndex = 1;
            this.buttonRandom.UseVisualStyleBackColor = true;
            this.buttonRandom.Click += new System.EventHandler(this.buttonRandom_Click);
            // 
            // buttonTogglePlayPause
            // 
            this.buttonTogglePlayPause.FlatAppearance.BorderSize = 0;
            this.buttonTogglePlayPause.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonTogglePlayPause.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonTogglePlayPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonTogglePlayPause.Image = ((System.Drawing.Image)(resources.GetObject("buttonTogglePlayPause.Image")));
            this.buttonTogglePlayPause.Location = new System.Drawing.Point(147, 3);
            this.buttonTogglePlayPause.Name = "buttonTogglePlayPause";
            this.buttonTogglePlayPause.Size = new System.Drawing.Size(66, 58);
            this.buttonTogglePlayPause.TabIndex = 1;
            this.buttonTogglePlayPause.UseVisualStyleBackColor = true;
            this.buttonTogglePlayPause.Click += new System.EventHandler(this.buttonTogglePlayPause_Click);
            // 
            // seekBarPanel
            // 
            this.seekBarPanel.BackColor = System.Drawing.Color.DimGray;
            this.seekBarPanel.Location = new System.Drawing.Point(57, 392);
            this.seekBarPanel.Name = "seekBarPanel";
            this.seekBarPanel.Size = new System.Drawing.Size(362, 10);
            this.seekBarPanel.TabIndex = 0;
            // 
            // progressPanel
            // 
            this.progressPanel.BackColor = System.Drawing.Color.White;
            this.progressPanel.Enabled = false;
            this.progressPanel.Location = new System.Drawing.Point(57, 392);
            this.progressPanel.Name = "progressPanel";
            this.progressPanel.Size = new System.Drawing.Size(362, 10);
            this.progressPanel.TabIndex = 0;
            // 
            // historyPanel
            // 
            this.historyPanel = new NoHorizontalScrollPanel();
            this.historyPanel.AutoScroll = true;
            this.historyPanel.BackColor = System.Drawing.Color.Black;
            this.historyPanel.Location = new System.Drawing.Point(444, 12);
            this.historyPanel.Name = "historyPanel";
            this.historyPanel.Size = new System.Drawing.Size(439, 459);
            this.historyPanel.TabIndex = 17;
            this.historyPanel.Visible = false;
            // 
            // playlistPanel
            // 
            this.playlistPanel = new NoHorizontalScrollPanel();
            this.playlistPanel.AutoScroll = true;
            this.playlistPanel.BackColor = System.Drawing.Color.Black;
            this.playlistPanel.Location = new System.Drawing.Point(444, 12);
            this.playlistPanel.Name = "playlistPanel";
            this.playlistPanel.Size = new System.Drawing.Size(439, 459);
            this.playlistPanel.TabIndex = 22;
            this.playlistPanel.Visible = false;
            // 
            // buttonCreatePlaylist
            // 
            this.buttonCreatePlaylist.BackColor = System.Drawing.Color.Black;
            this.buttonCreatePlaylist.ForeColor = System.Drawing.Color.White;
            this.buttonCreatePlaylist.Location = new System.Drawing.Point(454, 420);
            this.buttonCreatePlaylist.Name = "buttonCreatePlaylist";
            this.buttonCreatePlaylist.Size = new System.Drawing.Size(120, 30);
            this.buttonCreatePlaylist.TabIndex = 0;
            this.buttonCreatePlaylist.Text = "Create Playlist";
            this.buttonCreatePlaylist.UseVisualStyleBackColor = false;
            this.buttonCreatePlaylist.Visible = false;
            this.buttonCreatePlaylist.Click += new System.EventHandler(this.buttonCreatePlaylist_Click);
            // 
            // button3
            // 
            this.button3.FlatAppearance.BorderSize = 0;
            this.button3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.button3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Image = global::SoundCloud.Properties.Resources.history;
            this.button3.Location = new System.Drawing.Point(0, 276);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(57, 50);
            this.button3.TabIndex = 20;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // ButtonOpenSearchForm
            // 
            this.ButtonOpenSearchForm.AccessibleName = "ButtonOpenSearchForm";
            this.ButtonOpenSearchForm.FlatAppearance.BorderSize = 0;
            this.ButtonOpenSearchForm.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.ButtonOpenSearchForm.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.ButtonOpenSearchForm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonOpenSearchForm.Image = global::SoundCloud.Properties.Resources.search;
            this.ButtonOpenSearchForm.Location = new System.Drawing.Point(0, 220);
            this.ButtonOpenSearchForm.Name = "ButtonOpenSearchForm";
            this.ButtonOpenSearchForm.Size = new System.Drawing.Size(57, 50);
            this.ButtonOpenSearchForm.TabIndex = 19;
            this.ButtonOpenSearchForm.UseVisualStyleBackColor = true;
            this.ButtonOpenSearchForm.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Image = global::SoundCloud.Properties.Resources.home;
            this.button1.Location = new System.Drawing.Point(0, 164);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(57, 50);
            this.button1.TabIndex = 18;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonPlaylistPage
            // 
            this.buttonPlaylistPage.FlatAppearance.BorderSize = 0;
            this.buttonPlaylistPage.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonPlaylistPage.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonPlaylistPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPlaylistPage.Image = global::SoundCloud.Properties.Resources.playlist_icon;
            this.buttonPlaylistPage.Location = new System.Drawing.Point(0, 332);
            this.buttonPlaylistPage.Name = "buttonPlaylistPage";
            this.buttonPlaylistPage.Size = new System.Drawing.Size(57, 50);
            this.buttonPlaylistPage.TabIndex = 21;
            this.buttonPlaylistPage.UseVisualStyleBackColor = true;
            this.buttonPlaylistPage.Click += new System.EventHandler(this.buttonPlaylistPage_Click);
            // 
            // buttonToggleOverlay
            // 
            this.buttonToggleOverlay.FlatAppearance.BorderSize = 0;
            this.buttonToggleOverlay.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonToggleOverlay.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonToggleOverlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonToggleOverlay.Image = global::SoundCloud.Properties.Resources.overlay_icon;
            this.buttonToggleOverlay.Location = new System.Drawing.Point(0, 388);
            this.buttonToggleOverlay.Name = "buttonToggleOverlay";
            this.buttonToggleOverlay.Size = new System.Drawing.Size(57, 50);
            this.buttonToggleOverlay.TabIndex = 23;
            this.buttonToggleOverlay.UseVisualStyleBackColor = true;
            this.buttonToggleOverlay.Click += new System.EventHandler(this.buttonToggleOverlay_Click);
            // 
            // pictureBoxThumb
            // 
            this.pictureBoxThumb.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxThumb.Location = new System.Drawing.Point(57, 12);
            this.pictureBoxThumb.Name = "pictureBoxThumb";
            this.pictureBoxThumb.Size = new System.Drawing.Size(362, 329);
            this.pictureBoxThumb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxThumb.TabIndex = 10;
            this.pictureBoxThumb.TabStop = false;
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.Image = global::SoundCloud.Properties.Resources.logo;
            this.pictureBoxLogo.Location = new System.Drawing.Point(-17, -35);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(93, 117);
            this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxLogo.TabIndex = 21;
            this.pictureBoxLogo.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(895, 483);
            this.Controls.Add(this.buttonToggleOverlay); // Add this line
            this.Controls.Add(this.buttonCreatePlaylist);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.ButtonOpenSearchForm);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonPlaylistPage);
            this.Controls.Add(this.historyPanel);
            this.Controls.Add(this.playlistPanel);
            this.Controls.Add(this.progressPanel);
            this.Controls.Add(this.seekBarPanel);
            this.Controls.Add(this.tableLayoutPanel);
            this.Controls.Add(this.trackBarVolume);
            this.Controls.Add(this.textBoxUrl);
            this.Controls.Add(this.buttonLoadCsv);
            this.Controls.Add(this.listBoxTracks);
            this.Controls.Add(this.labelAuthor);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.labelCurrentTime);
            this.Controls.Add(this.labelTotalTime);
            this.Controls.Add(this.pictureBoxThumb);
            this.Controls.Add(this.pictureBoxLogo);
            this.Name = "Form1";
            this.Text = "SoundCloud Player";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).EndInit();
            this.tableLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button buttonTogglePlayPause;
        private System.Windows.Forms.Button buttonRandom;
        private System.Windows.Forms.Button buttonPrevious;
        private System.Windows.Forms.Button buttonLoop;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.PictureBox pictureBoxLogo; // Add this line
        private Button button1;
        private Button ButtonOpenSearchForm;
        private Button button3;
        private Button buttonPlaylistPage; // Add this line
       
    }
}
