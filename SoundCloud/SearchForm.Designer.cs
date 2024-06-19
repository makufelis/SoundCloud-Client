using System.Drawing;
using System.Windows.Forms;

namespace SoundCloud
{
    partial class SearchForm : Form
    {
        private TextBox searchTextBox;
        private Button searchButton;
        private CheckBox userCheckBox;
        private CheckBox trackCheckBox;
        private CheckBox playlistCheckBox; // New checkbox for playlists

        private void InitializeComponent()
        {
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.userCheckBox = new System.Windows.Forms.CheckBox();
            this.trackCheckBox = new System.Windows.Forms.CheckBox();
            this.playlistCheckBox = new System.Windows.Forms.CheckBox(); // Initialize the new checkbox

            this.SuspendLayout();
            // 
            // searchTextBox
            // 
            this.searchTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.searchTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.searchTextBox.ForeColor = System.Drawing.Color.White;
            this.searchTextBox.Location = new System.Drawing.Point(12, 15);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(200, 13);
            this.searchTextBox.TabIndex = 0;
            // 
            // searchButton
            // 
            this.searchButton.BackColor = System.Drawing.Color.Black;
            this.searchButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.searchButton.Location = new System.Drawing.Point(218, 10);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(75, 23);
            this.searchButton.TabIndex = 1;
            this.searchButton.Text = "Search";
            this.searchButton.UseVisualStyleBackColor = false;
            this.searchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // userCheckBox
            // 
            this.userCheckBox.AutoSize = true;
            this.userCheckBox.Location = new System.Drawing.Point(299, 14);
            this.userCheckBox.Name = "userCheckBox";
            this.userCheckBox.Size = new System.Drawing.Size(167, 17);
            this.userCheckBox.TabIndex = 2;
            this.userCheckBox.Text = "Search Users (Liked/Playlists)";
            this.userCheckBox.UseVisualStyleBackColor = true;
            // 
            // trackCheckBox
            // 
            this.trackCheckBox.AutoSize = true;
            this.trackCheckBox.Location = new System.Drawing.Point(481, 14);
            this.trackCheckBox.Name = "trackCheckBox";
            this.trackCheckBox.Size = new System.Drawing.Size(96, 17);
            this.trackCheckBox.TabIndex = 3;
            this.trackCheckBox.Text = "Search Tracks";
            this.trackCheckBox.UseVisualStyleBackColor = true;
            // 
            // playlistCheckBox
            // 
            this.playlistCheckBox.AutoSize = true;
            this.playlistCheckBox.Location = new System.Drawing.Point(583, 14);
            this.playlistCheckBox.Name = "playlistCheckBox";
            this.playlistCheckBox.Size = new System.Drawing.Size(106, 17);
            this.playlistCheckBox.TabIndex = 4;
            this.playlistCheckBox.Text = "Search Playlists";
            this.playlistCheckBox.UseVisualStyleBackColor = true;
            // 
            // SearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(700, 495);
            this.Controls.Add(this.playlistCheckBox); // Add the new checkbox to the form
            this.Controls.Add(this.trackCheckBox);
            this.Controls.Add(this.userCheckBox);
            this.Controls.Add(this.searchButton);
            this.Controls.Add(this.searchTextBox);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "SearchForm";
            this.Text = "Search";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
