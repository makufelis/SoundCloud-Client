using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Linq; // Add this line
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using SoundCloud;
using SoundCloud.Models;
using SoundCloud.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Newtonsoft.Json;

namespace SoundCloudPlayer
{
    public partial class Form1 : Form
    {
        private List<SoundCloudTrack> tracks = new List<SoundCloudTrack>();
        private WaveOutEvent waveOut;
        private MediaFoundationReader mediaReader;
        private string currentTempFilePath;
        private bool manualStop = false;
        private int currentPlayingIndex = -1; // Index of currently playing track
        private bool isPaused = false;
        private bool isLooped = false; // Flag to indicate if loop is enabled
        private bool nextTrack = false;
        private bool isRandomModeEnabled = false;
        private Timer timer; // Timer for updating current time label
        private ToolTip toolTip1; // Declare the ToolTip object at the class level
        private static bool useFallbackApi = false;
        private string soundCloudClientId;
        private SearchForm searchForm; // Instance of SearchForm
        private bool isOverlayEnabled = true;
        private string historyFilePath = "history.txt"; // File path for history
        private bool isDownloading = false;
        private ContextMenuStrip trackContextMenu; // Add this line
        private OverlayForm overlayForm;
        private string currentImageUrl;
        private string currentAuthor;
        private string currentTitle;
        
        public Form1(string soundCloudClientId)
        {
            InitializeComponent();
            this.soundCloudClientId = soundCloudClientId;
            waveOut = new WaveOutEvent();
            waveOut.PlaybackStopped += OnPlaybackStopped; // Hook into the PlaybackStopped event

            // Set ListBox DrawMode to OwnerDrawFixed
            listBoxTracks.DrawMode = DrawMode.OwnerDrawFixed;
            listBoxTracks.DrawItem += listBoxTracks_DrawItem;

            // Double-click event for track selection
            listBoxTracks.DoubleClick += listBoxTracks_DoubleClick;

            // Right-click event for track context menu
            listBoxTracks.MouseUp += ListBoxTracks_MouseUp;

            // SelectedIndexChanged event for track selection
            listBoxTracks.SelectedIndexChanged += listBoxTracks_SelectedIndexChanged;

            // Initialize the timer
            timer = new Timer();
            timer.Interval = 1000; // Update every second
            timer.Tick += Timer_Tick;

            // Create and configure the tooltip
            toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;

            // Attach the tooltip to the seek bar panel
            toolTip1.SetToolTip(seekBarPanel, "");

            seekBarPanel.MouseMove += SeekBarPanel_MouseMove;
            seekBarPanel.MouseClick += SeekBarPanel_MouseClick;

            // Initialize the SearchForm instance
            searchForm = new SearchForm(soundCloudClientId, this);
            LoadHistory();
            this.buttonPlaylistPage.Click += new System.EventHandler(this.buttonPlaylistPage_Click);

            // Initialize the context menu
            InitializeTrackContextMenu();
            isOverlayEnabled = false;
            LoadPlaylists();
        }

        private void buttonToggleOverlay_Click(object sender, EventArgs e)
        {
            
            isOverlayEnabled = !isOverlayEnabled;

            if (isOverlayEnabled)
            {
                if (overlayForm == null)
                {
                    // If no overlay form exists, create a new one
                    ShowOverlayImage(currentImageUrl, currentAuthor, currentTitle);
                }
            }
            else
            {
                // If overlay is disabled, close the existing overlay form if it exists
                overlayForm?.Close();
                overlayForm = null;
            }
        }



        private void InitializeTrackContextMenu()
        {
            trackContextMenu = new ContextMenuStrip();
            trackContextMenu.Opening += TrackContextMenu_Opening;
        }

        private void TrackContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            trackContextMenu.Items.Clear();

            if (listBoxTracks.SelectedIndex == -1)
            {
                e.Cancel = true;
                return;
            }

            // Get the selected track
            var selectedTrack = tracks[listBoxTracks.SelectedIndex];
            string selectedTrackTitle = selectedTrack.Name; // Assuming Name is the correct property name
            string selectedTrackUrl = selectedTrack.Url; // Assuming Url is the correct property name

            // Add the track title as a disabled item
            ToolStripMenuItem trackTitleItem = new ToolStripMenuItem($"Track: {selectedTrackTitle}");
            trackTitleItem.Enabled = false;
            trackContextMenu.Items.Add(trackTitleItem);

            // Add the "Copy Link" option
            ToolStripMenuItem copyLinkItem = new ToolStripMenuItem("Copy Link");
            copyLinkItem.Click += (s, ev) =>
            {
                Clipboard.SetText(selectedTrackUrl);
            };
            trackContextMenu.Items.Add(copyLinkItem);

            // Add the "Add To Playlist" submenu
            ToolStripMenuItem addToPlaylistItem = new ToolStripMenuItem("Add To Playlist");

            string playlistsFilePath = "playlists.json";
            if (File.Exists(playlistsFilePath))
            {
                string json = File.ReadAllText(playlistsFilePath);
                var playlists = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

                foreach (var playlistEntry in playlists)
                {
                    var playlistName = playlistEntry.Key;
                    ToolStripMenuItem playlistMenuItem = new ToolStripMenuItem(playlistName);
                    playlistMenuItem.Click += (s, ev) => AddTrackToPlaylist(playlistName);
                    addToPlaylistItem.DropDownItems.Add(playlistMenuItem);
                }
            }

            trackContextMenu.Items.Add(addToPlaylistItem);
        }


        private void ListBoxTracks_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = listBoxTracks.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                {
                    listBoxTracks.SelectedIndex = index;
                    trackContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void AddTrackToPlaylist(string playlistName)
        {
            if (listBoxTracks.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a track first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get the selected track
            var selectedTrack = tracks[listBoxTracks.SelectedIndex];
            string selectedTrackUrl = selectedTrack.Url; // Assuming Url is the correct property name
            string selectedTrackTitle = selectedTrack.Name; // Assuming Name is the correct property name

            try
            {
                string playlistsFilePath = "playlists.json";
                var playlists = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(playlistsFilePath));

                if (playlists.ContainsKey(playlistName))
                {
                    var playlist = playlists[playlistName];
                    List<Dictionary<string, string>> tracksList;

                    if (playlist.ContainsKey("tracks"))
                    {
                        tracksList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(playlist["tracks"].ToString());
                    }
                    else
                    {
                        tracksList = new List<Dictionary<string, string>>();
                    }

                    // Check if the track is already in the playlist
                    bool trackExists = tracksList.Any(track => track["url"] == selectedTrackUrl);
                    if (trackExists)
                    {
                        MessageBox.Show($"Track is already in the {playlistName} playlist.", "Duplicate Track", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    tracksList.Add(new Dictionary<string, string> { { "title", selectedTrackTitle }, { "url", selectedTrackUrl } });

                    // Update the playlist's tracks
                    playlist["tracks"] = tracksList;

                    // Save the updated playlists to the JSON file
                    string updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(playlists, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(playlistsFilePath, updatedJson);

                    MessageBox.Show($"Track added to {playlistName} playlist.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Playlist {playlistName} does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding track to playlist: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPlaylists()
        {
            string playlistsFilePath = "playlists.json";

            if (!File.Exists(playlistsFilePath))
            {
                return; // If the file doesn't exist, there's nothing to load
            }

            string json = File.ReadAllText(playlistsFilePath);
            var playlists = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

            int top = 10; // Starting position for the first playlist

            foreach (var playlistEntry in playlists)
            {
                var playlist = playlistEntry.Value;
                string playlistName = playlist["name"].ToString();
                string imageUrl = playlist.ContainsKey("image") && playlist["image"] != null ? playlist["image"].ToString() : null;

                AddPlaylistToPanel(playlistName, imageUrl, ref top);
            }
        }

        private void AddPlaylistToPanel(string playlistName, string imageUrl, ref int top)
        {
            Panel playlistItemPanel = new Panel();
            playlistItemPanel.AutoSize = true;
            playlistItemPanel.Width = playlistPanel.Width - 20; // Adjust for padding
            playlistItemPanel.Height = 100;
            playlistItemPanel.Location = new Point(10, top);
            playlistItemPanel.BorderStyle = BorderStyle.FixedSingle;

            PictureBox pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Width = 80;
            pictureBox.Height = 80;
            pictureBox.Location = new Point(10, 10);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                pictureBox.ImageLocation = imageUrl;
            }
            else
            {
                pictureBox.BackColor = Color.Gray; // Default background color if no image is provided
            }

            Label titleLabel = new Label();
            titleLabel.AutoSize = true;
            titleLabel.Text = playlistName;
            titleLabel.Font = new Font(titleLabel.Font, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Location = new Point(pictureBox.Right + 10, 10);

            Button viewButton = new Button();
            viewButton.Text = "View";
            viewButton.ForeColor = Color.White;
            viewButton.Location = new Point(pictureBox.Right + 10, titleLabel.Bottom + 10);
            viewButton.Click += (sender, e) =>
            {
                // Handle view playlist tracks logic here
                ViewPlaylist(playlistName);
                this.buttonCreatePlaylist.Visible = false;
            };

            Button loadButton = new Button();
            loadButton.Text = "Load";
            loadButton.ForeColor = Color.White;
            loadButton.Location = new Point(viewButton.Right + 10, titleLabel.Bottom + 10);
            loadButton.Click += (sender, e) =>
            {
                // Handle load playlist tracks logic here
                LoadPlaylistTracks(playlistName);
            };

            playlistItemPanel.Controls.Add(pictureBox);
            playlistItemPanel.Controls.Add(titleLabel);
            playlistItemPanel.Controls.Add(viewButton);
            playlistItemPanel.Controls.Add(loadButton);

            playlistPanel.Controls.Add(playlistItemPanel);

            top += 100 + 10; // Increment top position for the next playlist item
        }

        private void LoadPlaylistTracks(string playlistName)
        {
            string playlistsFilePath = "playlists.json";

            if (!File.Exists(playlistsFilePath))
            {
                MessageBox.Show("No playlists found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string json = File.ReadAllText(playlistsFilePath);
            var playlists = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

            if (playlists.ContainsKey(playlistName))
            {
                var playlist = playlists[playlistName];
                List<Dictionary<string, string>> tracksList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(playlist["tracks"].ToString());

                if (tracksList.Count == 0)
                {
                    MessageBox.Show("No tracks in the playlist.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                List<string> trackInfoList = tracksList.Select(track => $"{track["url"]}, {track["title"]}").ToList();
                string tempCsvFilePath = SaveTempCsv(trackInfoList);

                ReceiveTracklist(tempCsvFilePath);
            }
            else
            {
                MessageBox.Show($"Playlist {playlistName} does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public string SaveTempCsv(List<string> trackInfoList)
        {
            string tempCsvFilePath = null;

            try
            {
                // Generate a unique temporary file path
                tempCsvFilePath = System.IO.Path.GetTempFileName();

                // Process each track info to replace unwanted characters
                List<string> cleanedTrackInfoList = trackInfoList.Select(trackInfo =>
                    trackInfo.Replace("`", " ").Replace("\"", " ")
                ).ToList();

                // Write the cleaned tracklist data to the temporary CSV file
                System.IO.File.WriteAllLines(tempCsvFilePath, cleanedTrackInfoList);
            }
            catch (Exception ex)
            {
                // Handle the exception as needed
                Debug.WriteLine($"Error saving CSV file: {ex.Message}");
            }

            return tempCsvFilePath;
        }

        private void PlaylistListBox_DragDrop(object sender, DragEventArgs e, string playlistName, List<Dictionary<string, string>> tracksList)
        {
            ListBox listBox = sender as ListBox;
            Point point = listBox.PointToClient(new Point(e.X, e.Y));
            int index = listBox.IndexFromPoint(point);

            if (index < 0) index = listBox.Items.Count - 1;
            var data = e.Data.GetData(typeof(string));

            Debug.WriteLine($"Dragging item: {data}");
            Debug.WriteLine($"Dropped at index: {index}");

            // Temporarily remove the item and insert it at the new location
            listBox.Items.Remove(data);
            listBox.Items.Insert(index, data);

            // Rebuild the tracks list based on the new order in the ListBox
            var newTracksList = new List<Dictionary<string, string>>();
            foreach (var item in listBox.Items)
            {
                var trackData = tracksList.FirstOrDefault(track => $"{track["title"]} - {track["url"]}" == item.ToString());
                if (trackData != null)
                {
                    newTracksList.Add(new Dictionary<string, string>
            {
                { "title", trackData["title"] },
                { "url", trackData["url"] }
            });
                }
            }

            Debug.WriteLine("New tracks list:");
            foreach (var track in newTracksList)
            {
                Debug.WriteLine($"Track: {track["title"]} - {track["url"]}");
            }

            // Update the playlist with the new order of tracks
            UpdatePlaylistTracks(playlistName, newTracksList);
        }

        private void UpdatePlaylistTracks(string playlistName, List<Dictionary<string, string>> newTracksList)
        {
            string playlistsFilePath = "playlists.json";

            if (!File.Exists(playlistsFilePath))
            {
                MessageBox.Show("No playlists found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string json = File.ReadAllText(playlistsFilePath);
            var playlists = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

            if (playlists.ContainsKey(playlistName))
            {
                var playlist = playlists[playlistName];
                playlist["tracks"] = newTracksList;

                // Save the updated playlists to the JSON file
                string updatedJson = JsonConvert.SerializeObject(playlists, Formatting.Indented);
                File.WriteAllText(playlistsFilePath, updatedJson);

                Debug.WriteLine("Updated tracks list in JSON:");
                foreach (var track in newTracksList)
                {
                    Debug.WriteLine($"Track: {track["title"]} - {track["url"]}");
                }
            }
            else
            {
                MessageBox.Show($"Playlist {playlistName} does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void PlaylistListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void PlaylistListBox_MouseDown(object sender, MouseEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (e.Button == MouseButtons.Left && listBox.SelectedItem != null)
            {
                listBox.DoDragDrop(listBox.SelectedItem, DragDropEffects.Move);
            }
        }


        private void ViewPlaylist(string playlistName)
        {
            string playlistsFilePath = "playlists.json";

            if (!File.Exists(playlistsFilePath))
            {
                MessageBox.Show("No playlists found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string json = File.ReadAllText(playlistsFilePath);
            var playlists = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

            if (playlists.ContainsKey(playlistName))
            {
                var playlist = playlists[playlistName];
                List<Dictionary<string, string>> tracksList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(playlist["tracks"].ToString());

                playlistPanel.Controls.Clear(); // Clear the current playlist panel content

                // Create and configure the ListBox
                ListBox playlistListBox = new ListBox();
                playlistListBox.DrawMode = DrawMode.OwnerDrawFixed;
                playlistListBox.BackColor = Color.Black;
                playlistListBox.ForeColor = Color.White;
                playlistListBox.BorderStyle = BorderStyle.None;
                playlistListBox.Width = playlistPanel.Width - 20;
                playlistListBox.Location = new Point(10, 40); // Adjust location to make space for the back button
                playlistListBox.Height = playlistPanel.Height - 60; // Adjust height to fit the panel
                playlistListBox.DrawItem += PlaylistListBox_DrawItem; // Add DrawItem event handler

                // Enable drag-and-drop
                playlistListBox.AllowDrop = true;
                playlistListBox.MouseDown += PlaylistListBox_MouseDown;
                playlistListBox.DragOver += PlaylistListBox_DragOver;
                playlistListBox.DragDrop += (sender, e) => PlaylistListBox_DragDrop(sender, e, playlistName, tracksList);

                // Create and configure the context menu
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                ToolStripMenuItem trackTitleItem = new ToolStripMenuItem();
                trackTitleItem.Enabled = false;
                contextMenu.Items.Add(trackTitleItem);

                ToolStripMenuItem copyLinkItem = new ToolStripMenuItem("Copy Link");
                copyLinkItem.Click += (s, e) =>
                {
                    if (playlistListBox.SelectedItem != null)
                    {
                        var selectedItem = playlistListBox.SelectedItem.ToString();
                        var selectedTrack = tracksList.FirstOrDefault(track => $"{track["title"]} - {track["url"]}" == selectedItem);
                        if (selectedTrack != null)
                        {
                            Clipboard.SetText(selectedTrack["url"]);
                        }
                    }
                };
                contextMenu.Items.Add(copyLinkItem);

                ToolStripMenuItem deleteTrackItem = new ToolStripMenuItem("Delete Track");
                deleteTrackItem.Click += (s, e) =>
                {
                    if (playlistListBox.SelectedItem != null)
                    {
                        var selectedItem = playlistListBox.SelectedItem.ToString();
                        var selectedTrack = tracksList.FirstOrDefault(track => $"{track["title"]} - {track["url"]}" == selectedItem);
                        if (selectedTrack != null)
                        {
                            tracksList.Remove(selectedTrack);
                            playlistListBox.Items.Remove(selectedItem);
                            UpdatePlaylistTracks(playlistName, tracksList);
                        }
                    }
                };
                contextMenu.Items.Add(deleteTrackItem);

                playlistListBox.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        int index = playlistListBox.IndexFromPoint(e.Location);
                        if (index != ListBox.NoMatches)
                        {
                            playlistListBox.SelectedIndex = index;
                            var selectedItem = playlistListBox.SelectedItem.ToString();
                            var selectedTrack = tracksList.FirstOrDefault(track => $"{track["title"]} - {track["url"]}" == selectedItem);
                            if (selectedTrack != null)
                            {
                                trackTitleItem.Text = $"Track: {selectedTrack["title"]}";
                            }
                            contextMenu.Show(Cursor.Position);
                        }
                    }
                };

                // Add tracks to the ListBox
                foreach (var track in tracksList)
                {
                    playlistListBox.Items.Add($"{track["title"]} - {track["url"]}");
                }

                // Create and configure the Back button
                Button backButton = new Button();
                backButton.Text = "Back";
                backButton.ForeColor = Color.White;
                backButton.BackColor = Color.Black;
                backButton.Location = new Point(10, 10);
                backButton.Click += BackButton_Click; // Add event handler for the back button

                // Create and configure the Load button
                Button loadButton = new Button();
                loadButton.Text = "Load";
                loadButton.ForeColor = Color.White;
                loadButton.BackColor = Color.Black;
                loadButton.Location = new Point(100, 10); // Adjust position next to the back button
                loadButton.Click += (sender, e) =>
                {
                    // Handle load playlist tracks logic here
                    LoadPlaylistTracks(playlistName);
                };

                // Create and configure the Save button
                Button saveButton = new Button();
                saveButton.Text = "Save";
                saveButton.ForeColor = Color.White;
                saveButton.BackColor = Color.Black;
                saveButton.Location = new Point(190, 10); // Adjust position next to the load button
                saveButton.Click += (sender, e) =>
                {
                    // Handle save playlist tracks logic here
                    SavePlaylistTracks(playlistName, playlistListBox);
                };

                // Add the Back button, Load button, Save button, and ListBox to the playlist panel
                playlistPanel.Controls.Add(backButton);
                playlistPanel.Controls.Add(loadButton);
                playlistPanel.Controls.Add(saveButton);
                playlistPanel.Controls.Add(playlistListBox);
            }
            else
            {
                MessageBox.Show($"Playlist {playlistName} does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void SavePlaylistTracks(string playlistName, ListBox listBox)
        {
            // Rebuild the tracks list based on the new order in the ListBox
            var newTracksList = new List<Dictionary<string, string>>();
            foreach (var item in listBox.Items)
            {
                var parts = item.ToString().Split(new[] { "https" }, 2, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    string title = parts[0].Trim();
                    if (title.EndsWith(" -"))
                    {
                        title = title.Substring(0, title.Length - 2).Trim();
                    }
                    newTracksList.Add(new Dictionary<string, string>
            {
                { "title", title },
                { "url", "https" + parts[1].Trim() }
            });
                }
            }

            // Update the playlist with the new order of tracks
            UpdatePlaylistTracks(playlistName, newTracksList);
        }





        private void BackButton_Click(object sender, EventArgs e)
        {
            // Clear the playlist panel and reload the playlists
            playlistPanel.Controls.Clear();
            LoadPlaylists();
            this.buttonCreatePlaylist.Visible = true;
        }

        private void PlaylistListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ListBox listBox = sender as ListBox;
            e.DrawBackground();
            e.Graphics.DrawString(listBox.Items[e.Index].ToString(), e.Font, Brushes.White, e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }


        private void buttonCreatePlaylist_Click(object sender, EventArgs e)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Create Playlist";
                dialog.Width = 300;
                dialog.Height = 200;

                Label nameLabel = new Label { Text = "Playlist Name:", Left = 10, Top = 20, Width = 100 };
                TextBox nameTextBox = new TextBox { Left = 120, Top = 20, Width = 150 };

                CheckBox addImageCheckBox = new CheckBox { Text = "Add Image", Left = 10, Top = 60, Width = 100 };
                Label imageLinkLabel = new Label { Text = "Image Link:", Left = 10, Top = 100, Width = 100, Visible = false };
                TextBox imageLinkTextBox = new TextBox { Left = 120, Top = 100, Width = 150, Visible = false };

                addImageCheckBox.CheckedChanged += (s, ev) =>
                {
                    imageLinkLabel.Visible = addImageCheckBox.Checked;
                    imageLinkTextBox.Visible = addImageCheckBox.Checked;
                };

                Button submitButton = new Button { Text = "Submit", Left = 120, Top = 140, Width = 100 };
                submitButton.Click += (s, ev) =>
                {
                    string playlistName = nameTextBox.Text;
                    string imageLink = addImageCheckBox.Checked ? imageLinkTextBox.Text : null;

                    CreatePlaylist(playlistName, imageLink);
                    dialog.Close();
                };

                dialog.Controls.Add(nameLabel);
                dialog.Controls.Add(nameTextBox);
                dialog.Controls.Add(addImageCheckBox);
                dialog.Controls.Add(imageLinkLabel);
                dialog.Controls.Add(imageLinkTextBox);
                dialog.Controls.Add(submitButton);

                dialog.ShowDialog();
            }
        }




        private void CreatePlaylist(string playlistName, string imageLink)
        {
            try
            {
                string playlistsFilePath = "playlists.json";
                Dictionary<string, object> playlists;

                // Load existing playlists from JSON file if it exists
                if (File.Exists(playlistsFilePath))
                {
                    string json = File.ReadAllText(playlistsFilePath);
                    playlists = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                }
                else
                {
                    playlists = new Dictionary<string, object>();
                }

                // Create new playlist entry
                var newPlaylist = new Dictionary<string, object>
        {
            { "name", playlistName },
            { "image", imageLink },
            { "tracks", new List<object>() }
        };

                // Add the new playlist to the playlists dictionary
                playlists[playlistName] = newPlaylist;

                // Save the updated playlists to the JSON file
                string updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(playlists, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(playlistsFilePath, updatedJson);
                LoadPlaylists();
                MessageBox.Show("Playlist created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating playlist: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ButtonHome_Click(object sender, EventArgs e)
        {
            this.historyPanel.Visible = false;
            this.listBoxTracks.Visible = true;
            this.trackBarVolume.Visible = true;
            this.textBoxUrl.Visible = true;
        }

        private void ButtonHistory_Click(object sender, EventArgs e)
        {
            this.historyPanel.Visible = true;
            this.listBoxTracks.Visible = false;
            this.trackBarVolume.Visible = false;
            this.textBoxUrl.Visible = false;
        }
        public void LoadHistory()
        {
            Debug.WriteLine("LoadHistory called");
            try
            {
                if (File.Exists(historyFilePath))
                {
                    Debug.WriteLine("History file found.");

                    // Clear any existing controls in the historyPanel
                    historyPanel.Controls.Clear();

                    var historyLines = File.ReadAllLines(historyFilePath);
                    int originalLength = historyLines.Length;
                    Array.Reverse(historyLines); // Reverse the order of the lines to read from bottom to top
                    Debug.WriteLine($"Number of lines in history file: {originalLength}");
                    int top = 0;

                    for (int i = 0; i < originalLength; i++)
                    {
                        var line = historyLines[i];
                        int originalIndex = originalLength - 1 - i; // Calculate the original index
                        if (line.Contains("Track:"))
                        {
                            var parts = line.Split(new string[] { "Track:", ", " }, StringSplitOptions.None);
                            if (parts.Length >= 3)
                            {
                                string title = parts[1].Trim();
                                string url = parts[2].Trim();
                                string artworkUrl = parts.Length > 3 ? parts[3].Trim() : "";
                                Debug.WriteLine($"Loading track: {title}, {url}, {artworkUrl}");
                                AddTrackToHistoryPanel(title, url, artworkUrl, ref top, originalIndex);
                            }
                        }
                        else if (line.Contains("Likes:"))
                        {
                            var parts = line.Split(new string[] { "Likes:", ", " }, StringSplitOptions.None);
                            if (parts.Length >= 3)
                            {
                                string username = parts[1].Trim();
                                string id = parts[2].Trim();
                                string avatarUrl = parts.Length > 3 ? parts[3].Trim() : "";
                                Debug.WriteLine($"Loading likes: {username}, {id}, {avatarUrl}");
                                AddLikesToHistoryPanel(username, id, avatarUrl, ref top, originalIndex);
                            }
                        }
                        else if (line.Contains("Playlist:"))
                        {
                            var parts = line.Split(new string[] { "Playlist:", ", " }, StringSplitOptions.None);
                            if (parts.Length >= 4)
                            {
                                string title = parts[1].Trim();
                                string id = parts[2].Trim();
                                string permalink = parts[3].Trim();
                                string artworkUrl = parts.Length > 4 ? parts[4].Trim() : "";
                                Debug.WriteLine($"Loading playlist: {title}, {id}, {permalink}, {artworkUrl}");
                                AddPlaylistToHistoryPanel(title, id, permalink, artworkUrl, ref top, originalIndex);
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("History file not found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading history: {ex.Message}");
            }
        }



        private void AddTrackToHistoryPanel(string title, string url, string artworkUrl, ref int top, int lineIndex)
        {
            try
            {
                // Create a panel for the track
                Panel trackPanelItem = new Panel();
                trackPanelItem.AutoSize = true;
                trackPanelItem.Width = historyPanel.Width - 20; // Adjust for padding
                trackPanelItem.Height = 80;
                trackPanelItem.Location = new Point(0, top);
                trackPanelItem.BorderStyle = BorderStyle.FixedSingle;

                // Create a PictureBox for the track's artwork
                PictureBox pictureBox = new PictureBox();
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Width = 60;
                pictureBox.Height = 60;
                pictureBox.Location = new Point(10, 10);
                pictureBox.ImageLocation = string.IsNullOrEmpty(artworkUrl) ? null : artworkUrl;

                // Create a Label for the track's title
                Label titleLabel = new Label();
                titleLabel.AutoSize = true;
                titleLabel.Text = title;
                titleLabel.Font = new Font(titleLabel.Font, FontStyle.Bold);
                titleLabel.ForeColor = Color.White; // Set text color to white
                titleLabel.Location = new Point(pictureBox.Right + 10, 10);

                // Create a Label for the track's URL
                Label urlLabel = new Label();
                urlLabel.AutoSize = true;
                urlLabel.Text = $"URL: {url}";
                urlLabel.ForeColor = Color.White; // Set text color to white
                urlLabel.Location = new Point(pictureBox.Right + 10, titleLabel.Bottom + 5);

                // Create the "Play" button
                Button playButton = new Button();
                playButton.Text = "Play";
                playButton.ForeColor = Color.White; // Set the button text color to white
                playButton.Location = new Point(pictureBox.Right + 10, urlLabel.Bottom + 10);
                playButton.Click += (sender, e) =>
                {
                    // Save the track URL and title to a temporary CSV file
                    string tempCsvFilePath = searchForm.SaveTempCsv(new List<string> { $"{url}, {title}" });

                    // Directly call the method in Form1 to use the tracklist
                    ReceiveTracklist(tempCsvFilePath);
                };

                // Create the "Delete" button
                Button deleteButton = new Button();
                deleteButton.Text = "Delete";
                deleteButton.ForeColor = Color.Red; // Set the button text color to red
                deleteButton.Location = new Point(playButton.Right + 10, urlLabel.Bottom + 10);
                deleteButton.Click += (sender, e) =>
                {
                    DeleteLineFromHistory($"Track: {title}, {url}, {artworkUrl}", lineIndex);
                    LoadHistory();
                };

                // Add PictureBox, Labels, "Play", and "Delete" button to the track panel
                trackPanelItem.Controls.Add(pictureBox);
                trackPanelItem.Controls.Add(titleLabel);
                trackPanelItem.Controls.Add(urlLabel);
                trackPanelItem.Controls.Add(playButton);
                trackPanelItem.Controls.Add(deleteButton);

                // Add the track panel to the history panel
                historyPanel.Controls.Add(trackPanelItem);

                // Increment top position for the next track item
                top += 80 + 10; // Add some spacing between track items

                Debug.WriteLine($"Added track to history panel: {title}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding track to history panel: {ex.Message}");
            }
        }

        private void AddLikesToHistoryPanel(string username, string id, string avatarUrl, ref int top, int lineIndex)
        {
            try
            {
                // Create a panel for the liked user
                Panel likesPanelItem = new Panel();
                likesPanelItem.AutoSize = true;
                likesPanelItem.Width = historyPanel.Width - 20; // Adjust for padding
                likesPanelItem.Height = 80;
                likesPanelItem.Location = new Point(0, top);
                likesPanelItem.BorderStyle = BorderStyle.FixedSingle;

                // Create a PictureBox for the user's avatar
                PictureBox pictureBox = new PictureBox();
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Width = 60;
                pictureBox.Height = 60;
                pictureBox.Location = new Point(10, 10);
                pictureBox.ImageLocation = string.IsNullOrEmpty(avatarUrl) ? null : avatarUrl;

                // Create a Label for the user's name
                Label usernameLabel = new Label();
                usernameLabel.AutoSize = true;
                usernameLabel.Text = username;
                usernameLabel.Font = new Font(usernameLabel.Font, FontStyle.Bold);
                usernameLabel.ForeColor = Color.White; // Set text color to white
                usernameLabel.Location = new Point(pictureBox.Right + 10, 10);

                // Create a Label for the user's ID
                Label idLabel = new Label();
                idLabel.AutoSize = true;
                idLabel.Text = $"ID: {id}";
                idLabel.ForeColor = Color.White; // Set text color to white
                idLabel.Location = new Point(pictureBox.Right + 10, usernameLabel.Bottom + 5);

                // Create the "Fetch Likes" button
                Button fetchLikesButton = new Button();
                fetchLikesButton.Text = "Get Liked";
                fetchLikesButton.ForeColor = Color.White; // Set the button text color to white
                fetchLikesButton.Location = new Point(pictureBox.Right + 10, idLabel.Bottom + 10);
                fetchLikesButton.Click += async (sender, e) =>
                {
                    await searchForm.FetchUserLikes(id);
                };

                // Create the "Delete" button
                Button deleteButton = new Button();
                deleteButton.Text = "Delete";
                deleteButton.ForeColor = Color.Red; // Set the button text color to red
                deleteButton.Location = new Point(fetchLikesButton.Right + 10, idLabel.Bottom + 10);
                deleteButton.Click += (sender, e) =>
                {
                    DeleteLineFromHistory($"Likes: {username}, {id}, {avatarUrl}", lineIndex);
                    LoadHistory();
                };

                // Add PictureBox, Labels, "Fetch Likes", and "Delete" button to the likes panel
                likesPanelItem.Controls.Add(pictureBox);
                likesPanelItem.Controls.Add(usernameLabel);
                likesPanelItem.Controls.Add(idLabel);
                likesPanelItem.Controls.Add(fetchLikesButton);
                likesPanelItem.Controls.Add(deleteButton);

                // Add the likes panel to the history panel
                historyPanel.Controls.Add(likesPanelItem);

                // Increment top position for the next likes item
                top += 80 + 10; // Add some spacing between likes items

                Debug.WriteLine($"Added likes to history panel: {username}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding likes to history panel: {ex.Message}");
            }
        }

        private void AddPlaylistToHistoryPanel(string title, string id, string permalink, string artworkUrl, ref int top, int lineIndex)
        {
            try
            {
                // Create a panel for the playlist
                Panel playlistPanelItem = new Panel();
                playlistPanelItem.AutoSize = true;
                playlistPanelItem.Width = historyPanel.Width - 20; // Adjust for padding
                playlistPanelItem.Height = 80;
                playlistPanelItem.Location = new Point(0, top);
                playlistPanelItem.BorderStyle = BorderStyle.FixedSingle;

                // Create a PictureBox for the playlist's artwork
                PictureBox pictureBox = new PictureBox();
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Width = 60;
                pictureBox.Height = 60;
                pictureBox.Location = new Point(10, 10);
                pictureBox.ImageLocation = string.IsNullOrEmpty(artworkUrl) ? null : artworkUrl;

                // Create a Label for the playlist's title
                Label titleLabel = new Label();
                titleLabel.AutoSize = true;
                titleLabel.Text = title;
                titleLabel.Font = new Font(titleLabel.Font, FontStyle.Bold);
                titleLabel.ForeColor = Color.White; // Set text color to white
                titleLabel.Location = new Point(pictureBox.Right + 10, 10);

                // Create a Label for the playlist's permalink
                Label permalinkLabel = new Label();
                permalinkLabel.AutoSize = true;
                permalinkLabel.Text = $"Permalink: {permalink}";
                permalinkLabel.ForeColor = Color.White; // Set text color to white
                permalinkLabel.Location = new Point(pictureBox.Right + 10, titleLabel.Bottom + 5);

                // Create the "Get Playlist Tracks" button
                Button playButton = new Button();
                playButton.Text = "Get Playlist Tracks";
                playButton.ForeColor = Color.White; // Set the button text color to white
                playButton.Location = new Point(pictureBox.Right + 10, permalinkLabel.Bottom + 10);
                playButton.Click += async (sender, e) =>
                {
                    await searchForm.FetchPlaylistTracks(id, title, permalink, artworkUrl);
                };

                // Create the "Delete" button
                Button deleteButton = new Button();
                deleteButton.Text = "Delete";
                deleteButton.ForeColor = Color.Red; // Set the button text color to red
                deleteButton.Location = new Point(playButton.Right + 10, permalinkLabel.Bottom + 10);
                deleteButton.Click += (sender, e) =>
                {
                    DeleteLineFromHistory($"Playlist: {title}, {id}, {permalink}, {artworkUrl}", lineIndex);
                    LoadHistory();
                };

                // Add PictureBox, Labels, "Get Playlist Tracks", and "Delete" button to the playlist panel
                playlistPanelItem.Controls.Add(pictureBox);
                playlistPanelItem.Controls.Add(titleLabel);
                playlistPanelItem.Controls.Add(permalinkLabel);
                playlistPanelItem.Controls.Add(playButton);
                playlistPanelItem.Controls.Add(deleteButton);

                // Add the playlist panel to the history panel
                historyPanel.Controls.Add(playlistPanelItem);

                // Increment top position for the next playlist item
                top += 80 + 10; // Add some spacing between playlist items

                Debug.WriteLine($"Added playlist to history panel: {title}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding playlist to history panel: {ex.Message}");
            }
        }

        private void DeleteLineFromHistory(string lineToDelete, int lineIndex)
        {
            try
            {
                // Read all lines from the history file
                List<string> lines = new List<string>(File.ReadAllLines(historyFilePath));

                // Remove the specified line by index
                if (lineIndex >= 0 && lineIndex < lines.Count)
                {
                    lines.RemoveAt(lineIndex);
                }

                // Write the remaining lines back to the history file
                File.WriteAllLines(historyFilePath, lines);

                Debug.WriteLine($"Deleted line from history: {lineToDelete}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting line from history: {ex.Message}");
            }
        }



        // Method to receive the tracklist file path from SearchForm
        public void ReceiveTracklist(string tracklistFilePath)
        {
            // Debug message to confirm method invocation


            // Handle the tracklist file path
            UseTracklist(tracklistFilePath);


            listBoxTracks.SelectedIndex = 0; // Select the first track
            listBoxTracks_DoubleClick(this, EventArgs.Empty); // Simulate double-click on the first track

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update the current time label
            if (mediaReader != null && waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
            {
                labelCurrentTime.Text = $"{mediaReader.CurrentTime:mm\\:ss}"; // Update current time label

                // Update the progress bar
                UpdateSeekBar(mediaReader.CurrentTime.TotalSeconds, mediaReader.TotalTime.TotalSeconds);
            }
        }

        private void UpdateSeekBar(double currentTime, double totalTime)
        {
            double progressPercentage = currentTime / totalTime;
            int seekBarWidth = (int)(progressPercentage * seekBarPanel.ClientSize.Width);

            // Update the width of the progress panel
            progressPanel.Width = seekBarWidth;
        }

        private void SeekBarPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (mediaReader != null)
            {
                // Calculate the position where the mouse is hovering on the seek bar
                int seekBarPosition = e.X;
                double seekPercentage = (double)seekBarPosition / seekBarPanel.Width;
                double seekTime = seekPercentage * mediaReader.TotalTime.TotalSeconds;

                // Set the tooltip text to the time where the mouse is hovering
                toolTip1.SetToolTip(seekBarPanel, TimeSpan.FromSeconds(seekTime).ToString(@"hh\:mm\:ss"));
            }
        }

        private void SeekBarPanel_MouseClick(object sender, MouseEventArgs e)
        {
            // Calculate the position where the mouse was clicked on the seek bar
            int seekBarPosition = e.X;

            // Calculate the seek percentage based on the position of the mouse
            double seekPercentage = (double)seekBarPosition / seekBarPanel.Width;

            // Calculate the corresponding seek time
            double seekTime = seekPercentage * mediaReader.TotalTime.TotalSeconds;

            // Seek to the calculated position in the audio track
            mediaReader.CurrentTime = TimeSpan.FromSeconds(seekTime);

            // Update the current time label to reflect the new seeked position
            labelCurrentTime.Text = $"{mediaReader.CurrentTime:mm\\:ss}";

            // Update the progress bar panel to the new seeked point
            UpdateSeekBar(mediaReader.CurrentTime.TotalSeconds, mediaReader.TotalTime.TotalSeconds);
        }

        private void listBoxTracks_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Refresh the list box to update the colors
            listBoxTracks.Refresh();
        }

        private void buttonLoadCsv_Click(object sender, EventArgs e)
        {
            var csvReaderService = new CsvReaderService();
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Select a CSV file"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                tracks = csvReaderService.ReadCsv(openFileDialog.FileName);
                DisplayTracks();
            }
        }

        private void DisplayTracks()
        {
            listBoxTracks.Items.Clear();
            foreach (var track in tracks)
            {
                listBoxTracks.Items.Add(track.Name);
            }
        }

        // Method to toggle play/pause from OverlayForm
        public async Task TogglePlayPause()
        {
            if (currentPlayingIndex >= 0)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Pause();
                    isPaused = true;
                    buttonTogglePlayPause.Image = global::SoundCloud.Properties.Resources.play_icon; // Change button icon to play
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Play();
                    isPaused = false;
                    buttonTogglePlayPause.Image = global::SoundCloud.Properties.Resources.pause_icon; // Change button icon to pause
                }
                else
                {
                    // If not playing or paused, start playing the selected track
                    var selectedIndex = listBoxTracks.SelectedIndex;
                    if (selectedIndex == currentPlayingIndex)
                    {
                        await PlaySelectedTrack(selectedIndex);
                        buttonTogglePlayPause.Image = global::SoundCloud.Properties.Resources.pause_icon; // Change button icon to pause
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a track to play.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            overlayForm?.UpdatePlayPauseIcon(); // Update icon on overlay
        }


        private async void buttonTogglePlayPause_Click(object sender, EventArgs e)
        {
            await TogglePlayPause(); // Call the TogglePlayPause method
        }


        private async Task PlaySelectedTrack(int selectedIndex)
        {
            // Your existing code for playing the track
            if (waveOut.PlaybackState == PlaybackState.Playing)
            {
                manualStop = true;
                waveOut.Stop();
            }

            if (isPaused)
            {

                waveOut.Stop();
                isPaused = false;
                isDownloading = false; // Ensure the flag is reset

            }

            var selectedTrack = tracks[selectedIndex];
            Debug.WriteLine($"Selected Track Name: {selectedTrack.Name}");
            Debug.WriteLine($"Selected Track URL: {selectedTrack.Url}");

            var (mp3Url, thumbUrl, author, name) = await FetchMp3Url(selectedTrack.Url);
            currentImageUrl = thumbUrl;
            currentAuthor = author;
            currentTitle = name;
            textBoxUrl.Text = mp3Url;
            Debug.WriteLine($"Fetched MP3 URL: {mp3Url}");

            if (!string.IsNullOrEmpty(mp3Url))
            {
                await PlayMp3FromUrl(mp3Url, thumbUrl, author, name);
                currentPlayingIndex = selectedIndex; // Update the currently playing index
            }
        }

        private async Task<(string mp3Url, string thumbUrl, string author, string title)> FetchMp3Url(string trackUrl)
        {
            using (var client = new HttpClient())
            {
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(3));

                // Use fallback API if the flag is set
                if (useFallbackApi)
                {
                    return await UseFirstApi(client, trackUrl);
                }

                try
                {
                    var fallbackUrl = $"https://scconverter.net/api/audio.json?url={trackUrl}";
                    var responseTask = client.GetAsync(fallbackUrl, cts.Token);

                    var response = await responseTask;

                    var responseString = await response.Content.ReadAsStringAsync();

                    Debug.WriteLine($"HTTP Status Code: {response.StatusCode}");
                    Debug.WriteLine($"Response: {responseString}");

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseString);

                            // Extract the necessary fields from the response
                            var author = data.track.author.name;
                            var title = data.track.title;

                            string thumbUrl = data.track.thumbnail ?? FindFirstUrl(responseString, "https://i1.sndcdn.com");
                            string mp3Url = data.track.fetchStreamURL ?? FindFirstUrl(responseString, "https://cf-media.sndcdn.com");

                            if (!string.IsNullOrEmpty(mp3Url) && !string.IsNullOrEmpty(thumbUrl))
                            {
                                return (mp3Url, thumbUrl, author, title);
                            }
                            else
                            {
                                Debug.WriteLine($"MP3 URL or track thumbnail not found in response.\nResponse: {responseString}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error parsing JSON response: {ex.Message}\nResponse: {responseString}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Error fetching MP3 URL. Status Code: {response.StatusCode}\nResponse: {responseString}");
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"First API request timed out.");
                    useFallbackApi = true; // Set the flag to use the fallback API for the rest of the session
                }
                catch (HttpRequestException httpEx)
                {
                    Debug.WriteLine($"HTTP Request error: {httpEx.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unexpected error: {ex.Message}");
                }

                // Fallback to the first API if the second one fails or times out
                return await UseFirstApi(client, trackUrl);
            }
        }

        private async Task<(string mp3Url, string thumbUrl, string author, string title)> UseFirstApi(HttpClient client, string trackUrl)
        {
            try
            {
                var url = $"https://scloudplaylistdownloadermp3.com/api/sctrack.php?url={trackUrl}";
                var response = await client.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"HTTP Status Code (Fallback): {response.StatusCode}");
                Debug.WriteLine($"Response (Fallback): {responseString}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseString);
                        if (data?.dlink != null && data?.thumb != null)
                        {
                            var author = data.artist; // Extract author from response
                            var title = data.name; // Extract title from response
                            return (data.dlink, data.thumb, author, title);
                        }
                        else
                        {
                            Debug.WriteLine($"MP3 URL or track thumbnail not found in fallback response.\nResponse: {responseString}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error parsing JSON fallback response: {ex.Message}\nResponse: {responseString}");
                    }
                }
                else
                {
                    Debug.WriteLine($"Error fetching MP3 URL from fallback. Status Code: {response.StatusCode}\nResponse: {responseString}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"HTTP Request error (Fallback): {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error (Fallback): {ex.Message}");
            }

            return (string.Empty, string.Empty, string.Empty, string.Empty);
        }

        private string FindFirstUrl(string text, string urlStart)
        {
            Debug.WriteLine($"Using First API");
            int startIndex = text.IndexOf(urlStart);
            if (startIndex == -1) return string.Empty;

            int endIndex = text.IndexOf("\"", startIndex);
            if (endIndex == -1) return string.Empty;

            return text.Substring(startIndex, endIndex - startIndex);
        }





        private async Task PlayMp3FromUrl(string url, string thumbUrl, string author, string title)
        {
            try
            {
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                }

                if (mediaReader != null)
                {
                    mediaReader.Dispose();
                }

                // Delete the previous temp file if it exists
                if (!string.IsNullOrEmpty(currentTempFilePath) && File.Exists(currentTempFilePath))
                {
                    File.Delete(currentTempFilePath);
                    Debug.WriteLine($"Deleted previous temp file: {currentTempFilePath}");
                }

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var stream = await response.Content.ReadAsStreamAsync();

                    // Save stream to a temporary file
                    currentTempFilePath = Path.GetTempFileName();
                    using (var fileStream = new FileStream(currentTempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    UpdatePictureBoxImage(thumbUrl, author, title);
                    listBoxTracks.Refresh();
                    isDownloading = false; // Set the flag to true at the start of the download
                    mediaReader = new MediaFoundationReader(currentTempFilePath);
                    waveOut = new WaveOutEvent();
                    waveOut.PlaybackStopped += OnPlaybackStopped; // Re-hook into the PlaybackStopped event
                    buttonTogglePlayPause.Image = global::SoundCloud.Properties.Resources.pause_icon;
                    waveOut.Init(mediaReader);
                    RefreshTrackList(); // Refresh the tracklist visual representation
                    waveOut.Play();

                    // Start the timer to update current time label
                    timer.Start();

                    // Update total time label
                    labelTotalTime.Text = $"{mediaReader.TotalTime:mm\\:ss}";

                    if (isOverlayEnabled)
                    {
                        // Show the overlay with author and title
                        overlayForm?.Close();
                        ShowOverlayImage(thumbUrl, author, title);
                        overlayForm.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing MP3: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {

            }
        }

        public bool IsPlaying => waveOut != null && waveOut.PlaybackState == PlaybackState.Playing;


        private void ShowOverlayImage(string imageUrl, string author, string title)
        {
            if (overlayForm != null)
            {
                overlayForm.Close();
                overlayForm = null;
            }

            overlayForm = new OverlayForm(imageUrl, author, title, this); // Pass reference of Form1
            overlayForm.Show();
        }



        private void RefreshTrackList()
        {
            // Force the ListBox to redraw its items
            listBoxTracks.Invalidate();
        }

        private async void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            Debug.WriteLine("Playback stopped.");
            Debug.WriteLine($"Initial manualStop value: {manualStop}");
            Debug.WriteLine($"Initial nextTrack value: {nextTrack}");

            // Close the overlay form
            overlayForm?.Close();
            overlayForm = null;

            if (e.Exception != null)
            {
                MessageBox.Show($"Playback Error: {e.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Debug.WriteLine("No playback error, handling playback stopped event...");

                if (isLooped && !nextTrack && !manualStop) // If loop is enabled and nextTrack is false
                {
                    Debug.WriteLine("Loop enabled, rewinding to the beginning of the same track...");
                    if (currentPlayingIndex >= 0 && currentPlayingIndex < tracks.Count)
                    {
                        await RewindToBeginning(); // Rewind the audio playback to the beginning of the track
                    }
                }
                else if (isRandomModeEnabled && !manualStop) // If random mode is enabled and not manually stopped
                {
                    Debug.WriteLine("Random mode enabled, selecting a random track...");

                    var random = new Random();
                    var selectedIndex = random.Next(0, tracks.Count); // Generate a random index within the range of available tracks
                    listBoxTracks.SelectedIndex = selectedIndex; // Select the randomly chosen track

                    Debug.WriteLine($"Selected random track index: {selectedIndex}");

                    var (mp3Url, thumbUrl, author, name) = await FetchMp3Url(tracks[listBoxTracks.SelectedIndex].Url);
                    textBoxUrl.Text = mp3Url;
                    Debug.WriteLine($"Random Track MP3 URL: {mp3Url}");

                    if (!string.IsNullOrEmpty(mp3Url))
                    {
                        await PlayMp3FromUrl(mp3Url, thumbUrl, author, name);
                        currentPlayingIndex = listBoxTracks.SelectedIndex; // Update the currently playing index
                    }
                }
                else if (currentPlayingIndex >= 0 && currentPlayingIndex < tracks.Count - 1 && !manualStop) // If not looping, not in random mode, and not manually stopped
                {
                    Debug.WriteLine("Selecting next track...");

                    listBoxTracks.SelectedIndex = currentPlayingIndex + 1; // Move to the next track

                    Debug.WriteLine($"manualStop value after checking: {manualStop}");
                    var (mp3Url, thumbUrl, author, name) = await FetchMp3Url(tracks[listBoxTracks.SelectedIndex].Url);
                    textBoxUrl.Text = mp3Url;
                    Debug.WriteLine($"Next Track MP3 URL: {mp3Url}");

                    if (!string.IsNullOrEmpty(mp3Url))
                    {
                        await PlayMp3FromUrl(mp3Url, thumbUrl, author, name);
                        currentPlayingIndex = listBoxTracks.SelectedIndex; // Update the currently playing index
                    }
                }
                else
                {
                    Debug.WriteLine("End of list reached, loop not enabled or manual stop.");
                }
            }
            manualStop = false;
            Debug.WriteLine($"Final manualStop value: {manualStop}");
        }

        private void UpdatePictureBoxImage(string thumbUrl, string author, string title)
        {
            try
            {
                // Download the image from the URL
                using (WebClient webClient = new WebClient())
                {
                    byte[] imageData = webClient.DownloadData(thumbUrl);
                    using (var ms = new System.IO.MemoryStream(imageData))
                    {
                        // Convert byte array to Image
                        Image image = Image.FromStream(ms);
                        // Update pictureBoxThumb.Image
                        pictureBoxThumb.Image = image;
                    }
                }

                // Limit author or title to maximum 60 characters
                if (author.Length > 60)
                {
                    author = author.Substring(0, 57) + "...";
                }

                if (title.Length > 60)
                {
                    title = title.Substring(0, 57) + "...";
                }

                // Update author and title labels
                labelAuthor.Text = author;
                labelTitle.Text = title;
            }
            catch (Exception ex)
            {
                // Handle any exceptions, such as failed download or conversion
                // For now, let's just display the exception message
                MessageBox.Show($"Error updating PictureBox image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RewindToBeginning()
        {
            await Task.Run(() =>
            {
                // Stop the current playback
                waveOut.Stop();

                // Dispose the media reader and wave out objects
                mediaReader.Dispose();
                waveOut.Dispose();

                // Re-initialize the media reader and wave out with the same audio file
                mediaReader = new MediaFoundationReader(currentTempFilePath);
                waveOut = new WaveOutEvent();
                waveOut.PlaybackStopped += OnPlaybackStopped;
                waveOut.Init(mediaReader);
                waveOut.Play();
            });
        }

        public void trackBarVolume_Scroll(object sender, EventArgs e)
        {
            // Adjust the volume based on the position of the trackBarVolume
            waveOut.Volume = (float)trackBarVolume.Value / 100f;
        }

        private void buttonPrevious_Click(object sender, EventArgs e)
        {
            if (tracks.Count == 0)
            {
                MessageBox.Show("No tracks found in the list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (isRandomModeEnabled)
            {
                manualStop = false; // Reset manualStop
                Debug.WriteLine("Random mode enabled, selecting a random track...");

                var random = new Random();
                var selectedIndex = random.Next(0, tracks.Count); // Generate a random index within the range of available tracks
                listBoxTracks.SelectedIndex = selectedIndex; // Select the randomly chosen track

                Debug.WriteLine($"Selected random track index: {selectedIndex}");

                listBoxTracks_DoubleClick(sender, e); // Call listBoxTracks_DoubleClick with the selected index
            }
            else
            {
                if (currentPlayingIndex > 0)
                {
                    manualStop = true;
                    nextTrack = true; // Set nextTrack to true
                    listBoxTracks.SelectedIndex = currentPlayingIndex - 1; // Move to the previous track
                    PlaySelectedTrack(currentPlayingIndex - 1); // Play the selected track
                    nextTrack = false; // Reset nextTrack
                }
            }
        }

        public void buttonNext_Click(object sender, EventArgs e)
        {
            if (tracks.Count == 0)
            {
                MessageBox.Show("No tracks found in the list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (isRandomModeEnabled)
            {
                manualStop = false; // Reset manualStop
                Debug.WriteLine("Random mode enabled, selecting a random track...");

                var random = new Random();
                var selectedIndex = random.Next(0, tracks.Count); // Generate a random index within the range of available tracks
                listBoxTracks.SelectedIndex = selectedIndex; // Select the randomly chosen track

                Debug.WriteLine($"Selected random track index: {selectedIndex}");

                listBoxTracks_DoubleClick(sender, e); // Call listBoxTracks_DoubleClick with the selected index
            }
            else
            {
                if (currentPlayingIndex >= 0 && currentPlayingIndex < tracks.Count - 1)
                {
                    manualStop = true;
                    nextTrack = true; // Set nextTrack to true
                    listBoxTracks.SelectedIndex = currentPlayingIndex + 1; // Move to the next track
                    PlaySelectedTrack(currentPlayingIndex + 1); // Play the selected track
                    nextTrack = false; // Reset nextTrack
                }
            }
        }


        private void ButtonOpenSearchForm_Click(object sender, EventArgs e)
        {
            try
            {
                // Assuming soundCloudClientId is the variable storing the client ID in Form1
                searchForm.ShowDialog();
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                // Log the inner exception details
                MessageBox.Show($"Invocation exception: {ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void listBoxTracks_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var listBox = sender as ListBox;
            if (listBox == null) return;

            // Set background color
            Color backColor = e.Index == currentPlayingIndex ? Color.Green : (e.Index == listBox.SelectedIndex ? Color.Blue : Color.Black);

            // Set text color
            Color textColor = e.Index == currentPlayingIndex ? Color.Black : (e.Index == listBox.SelectedIndex ? Color.White : Color.White);

            // Fill the background
            e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);

            // Draw the item text
            string itemText = listBox.GetItemText(listBox.Items[e.Index]);
            e.Graphics.DrawString(itemText, e.Font, new SolidBrush(textColor), e.Bounds, StringFormat.GenericDefault);

            // Draw focus rectangle if necessary
            e.DrawFocusRectangle();
        }

        private void buttonLoop_Click(object sender, EventArgs e)
        {
            isLooped = !isLooped; // Toggle loop flag

            // If loop mode is enabled, ensure random mode is disabled
            if (isLooped)
            {
                isRandomModeEnabled = false;
                buttonRandom.Image = global::SoundCloud.Properties.Resources.random_icon; // Set random icon to default
            }
            buttonLoop.Image = isLooped ? global::SoundCloud.Properties.Resources.loop_active : global::SoundCloud.Properties.Resources.loop_icon; // Set loop icon based on loop status
        }

        private async void listBoxTracks_DoubleClick(object sender, EventArgs e)
        {
            if (isDownloading)
            {
                MessageBox.Show("A track is currently in progress. Please wait until the current track is played.", "Track in Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (listBoxTracks.SelectedIndex != -1)
            {
                if (waveOut != null && waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Play();
                    isDownloading = true; // Set the flag to true at the start of the download
                    await PlaySelectedTrack(listBoxTracks.SelectedIndex);
                }
                else
                {
                    isDownloading = true; // Set the flag to true at the start of the download
                    await PlaySelectedTrack(listBoxTracks.SelectedIndex);
                }
            }
        }



        private void buttonRandom_Click(object sender, EventArgs e)
        {
            // Check if the tracklist contains only one track
            if (tracks.Count <= 1)
            {
                MessageBox.Show("Random mode cannot be enabled when there is only one track.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Prevent the toggle action
            }

            isRandomModeEnabled = !isRandomModeEnabled; // Toggle the random mode

            // If random mode is enabled, ensure loop mode is disabled
            if (isRandomModeEnabled)
            {
                buttonRandom.Image = global::SoundCloud.Properties.Resources.random_active; // Set random icon to active
                isLooped = false;
                buttonLoop.Image = global::SoundCloud.Properties.Resources.loop_icon; // Set loop icon to default
            }
            else
            {
                // Random mode is disabled
                buttonRandom.Image = global::SoundCloud.Properties.Resources.random_icon; // Set random icon to default
            }
        }


        private void UseTracklist(string tracklistFilePath)
        {
            try
            {
                var csvReaderService = new CsvReaderService();
                tracks = csvReaderService.ReadCsv(tracklistFilePath);
                DisplayTracks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the CSV file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Existing code for your form load event...
            // Example: Call UpdateSeekBar after loading mediaReader
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.historyPanel.Visible = false;
            this.listBoxTracks.Visible = true;
            this.trackBarVolume.Visible = true;
            this.textBoxUrl.Visible = false;
            this.playlistPanel.Visible = false;
            this.buttonCreatePlaylist.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Assuming soundCloudClientId is the variable storing the client ID in Form1
                searchForm.ShowDialog();
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                // Log the inner exception details
                MessageBox.Show($"Invocation exception: {ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void UpdateVolume(int volume)
        {
            trackBarVolume.Value = volume; // Update the main form's track bar value
            waveOut.Volume = (float)volume / 100f; // Update the waveOut volume
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.historyPanel.Visible = true;
            this.listBoxTracks.Visible = false;
            this.trackBarVolume.Visible = false;
            this.textBoxUrl.Visible = false;
            this.playlistPanel.Visible = false;
            this.buttonCreatePlaylist.Visible = false;
        }

        private void buttonPlaylistPage_Click(object sender, EventArgs e)
        {
            this.historyPanel.Visible = false;
            this.listBoxTracks.Visible = false;
            this.trackBarVolume.Visible = false;
            this.textBoxUrl.Visible = false;
            this.playlistPanel.Visible = true;
            this.buttonCreatePlaylist.Visible = true;
            // Clear the playlist panel and reload the playlists
            playlistPanel.Controls.Clear();
            LoadPlaylists();
        }

    }
}
