using Newtonsoft.Json;
using SoundCloudPlayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SoundCloud.SearchForm;

namespace SoundCloud
{
    public partial class SearchForm : Form
    {
        private string soundCloudClientId;
        private Panel userPanel;
        private int userItemHeight = 80; // Height of each user item
        private Form1 mainForm; // Reference to the main form
        private string historyFilePath = "history.txt"; // File path for history

        public SearchForm(string clientId, Form1 mainForm)
        {
            InitializeComponent();
            soundCloudClientId = clientId;
            this.mainForm = mainForm; // Initialize the main form reference

            // Create a TableLayoutPanel to manage the layout
            TableLayoutPanel layoutPanel = new TableLayoutPanel();
            layoutPanel.Dock = DockStyle.Fill;
            layoutPanel.RowCount = 2;
            layoutPanel.ColumnCount = 1;
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutPanel.Padding = new Padding(10); // Add padding to create space

            // Create a panel to contain the user list
            userPanel = new Panel();
            userPanel.AutoScroll = true;
            userPanel.Dock = DockStyle.Fill;

            // Add the search controls to the first row of the TableLayoutPanel
            FlowLayoutPanel searchPanel = new FlowLayoutPanel();
            searchPanel.Dock = DockStyle.Fill;
            searchPanel.AutoSize = true;
            searchPanel.Controls.Add(searchTextBox);
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(userCheckBox);
            searchPanel.Controls.Add(trackCheckBox);
            searchPanel.Controls.Add(playlistCheckBox); // Add playlistCheckBox to the searchPanel

            layoutPanel.Controls.Add(searchPanel, 0, 0);
            layoutPanel.Controls.Add(userPanel, 0, 1);

            // Add the TableLayoutPanel to the form
            Controls.Add(layoutPanel);

            // Event handlers for checkboxes to ensure only one is checked at a time
            userCheckBox.CheckedChanged += (s, e) => {
                if (userCheckBox.Checked)
                {
                    trackCheckBox.Checked = false;
                    playlistCheckBox.Checked = false;
                }
            };

            trackCheckBox.CheckedChanged += (s, e) => {
                if (trackCheckBox.Checked)
                {
                    userCheckBox.Checked = false;
                    playlistCheckBox.Checked = false;
                }
            };

            playlistCheckBox.CheckedChanged += (s, e) => {
                if (playlistCheckBox.Checked)
                {
                    userCheckBox.Checked = false;
                    trackCheckBox.Checked = false;
                }
            };

            // Calculate the top position of the user panel
            userPanel.Top = searchTextBox.Bottom + 20; // Adjust as needed
        }

        private async void SearchButton_Click(object sender, EventArgs e)
        {
            string searchQuery = searchTextBox.Text;
            bool searchUsers = userCheckBox.Checked;
            bool searchTracks = trackCheckBox.Checked;
            bool searchPlaylists = playlistCheckBox.Checked;

            // Clear previous search results
            userPanel.Controls.Clear();

            if (searchUsers)
            {
                await SearchUsers(searchQuery);
            }

            if (searchTracks)
            {
                await SearchTracks(searchQuery);
            }

            if (searchPlaylists)
            {
                await SearchPlaylists(searchQuery);
            }
        }

        private async Task SearchUsers(string searchQuery)
        {
            if (searchQuery.StartsWith("https://soundcloud.com/") && userCheckBox.Checked)
            {
                await SearchUserByUrl(searchQuery);
            }
            
            else
            {
                string apiUrl = $"https://api-v2.soundcloud.com/search/users?q={searchQuery}&variant_ids=&facet=place&client_id={soundCloudClientId}&limit=20&offset=0&linked_partitioning=1&app_version=1717603207&app_locale=en";

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(apiUrl);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Parse JSON response to extract user data
                        List<User> users = ParseUserResponse(responseBody);

                        // Display user data in the panel
                        DisplayUsers(users);
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"An error occurred while making the request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task SearchPlaylists(string searchQuery)
        {
            if (searchQuery.StartsWith("https://soundcloud.com/") && searchQuery.Contains("/sets/") && playlistCheckBox.Checked)
            {
                await SearchPlaylistByUrl(searchQuery);
            }
            else
            {
                string apiUrl = $"https://api-v2.soundcloud.com/search/playlists?q={searchQuery}&client_id={soundCloudClientId}&limit=20&offset=0&linked_partitioning=1&app_version=1717603207&app_locale=en";

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(apiUrl);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Parse JSON response to extract playlist data
                        List<Playlist> playlists = ParsePlaylistResponse(responseBody);

                        // Display playlist data in the panel
                        DisplayPlaylists(playlists);
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"An error occurred while making the request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task SearchPlaylistByUrl(string playlistUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(playlistUrl);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Extract playlist data from HTML response
                    Playlist playlist = ParsePlaylistFromHtml(playlistUrl, responseBody);

                    // Display playlist data in the panel
                    DisplayPlaylists(new List<Playlist> { playlist });
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"An error occurred while making the request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Playlist ParsePlaylistFromHtml(string playlistUrl, string html)
        {
            string artworkUrl = null;
            string id = null;
            string title = null;

            // Find artwork URL
            int artworkStart = html.IndexOf("https://i1.sndcdn.com/");
            if (artworkStart != -1)
            {
                int artworkEnd = html.IndexOf("\"", artworkStart);
                if (artworkEnd != -1)
                {
                    artworkUrl = html.Substring(artworkStart, artworkEnd - artworkStart);
                }
            }

            // Find playlist ID
            int idStart = html.IndexOf("<meta property=\"twitter:app:url:googleplay\" content=\"soundcloud://playlists:");
            if (idStart != -1)
            {
                int contentStart = html.IndexOf("soundcloud://playlists:", idStart) + "soundcloud://playlists:".Length;
                int contentEnd = html.IndexOf("\"", contentStart);
                if (contentEnd != -1)
                {
                    id = html.Substring(contentStart, contentEnd - contentStart);
                }
            }

            // Find title
            int titleStart = html.IndexOf("<meta property=\"twitter:title\" content=\"");
            if (titleStart != -1)
            {
                int contentStart = html.IndexOf("content=\"", titleStart) + "content=\"".Length;
                int contentEnd = html.IndexOf("\"", contentStart);
                if (contentEnd != -1)
                {
                    title = html.Substring(contentStart, contentEnd - contentStart);
                }
            }

            // Create and return the Playlist object
            return new Playlist
            {
                ArtworkUrl = artworkUrl ?? "default_artwork_url", // Use default image if artworkUrl is null
                Id = id,
                Permalink = playlistUrl,
                Title = title
            };
        }

        private async Task SearchTracks(string searchQuery)
        {
            if (searchQuery.StartsWith("https://soundcloud.com/") && trackCheckBox.Checked)
            {
                await SearchTrackByUrl(searchQuery);
            }
            else
            {
                string apiUrl = $"https://api-v2.soundcloud.com/search/tracks?q={searchQuery}&variant_ids=&facet=genre&client_id={soundCloudClientId}&limit=20&offset=0&linked_partitioning=1&app_version=1717603207&app_locale=en";

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(apiUrl);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Parse JSON response to extract track data
                        List<Track> tracks = ParseTrackResponse(responseBody);

                        // Display track data in the panel
                        DisplayTracks(tracks);
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"An error occurred while making the request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task SearchTrackByUrl(string trackUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(trackUrl);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Extract track data from HTML response
                    Track track = ParseTrackFromHtml(trackUrl, responseBody);

                    // Display track data in the panel
                    DisplayTracks(new List<Track> { track });
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"An error occurred while making the request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Track ParseTrackFromHtml(string trackUrl, string html)
        {
            string artworkUrl = null;
            string title = null;

            // Find artwork URL
            int artworkStart = html.IndexOf("https://i1.sndcdn.com/");
            if (artworkStart != -1)
            {
                int artworkEnd = html.IndexOf("\"", artworkStart);
                if (artworkEnd != -1)
                {
                    artworkUrl = html.Substring(artworkStart, artworkEnd - artworkStart);
                }
            }

            // Find title
            int titleStart = html.IndexOf("<meta property=\"twitter:title\" content=\"");
            if (titleStart != -1)
            {
                int contentStart = html.IndexOf("content=\"", titleStart) + "content=\"".Length;
                int contentEnd = html.IndexOf("\"", contentStart);
                if (contentEnd != -1)
                {
                    title = html.Substring(contentStart, contentEnd - contentStart);
                }
            }

            // Create and return the Track object
            return new Track
            {
                PermalinkUrl = trackUrl,
                ArtworkUrl = artworkUrl ?? "default_artwork_url", // Use default image if artworkUrl is null
                Title = title
            };
        }

        private List<User> ParseUserResponse(string responseBody)
        {
            dynamic responseJson = JsonConvert.DeserializeObject(responseBody);
            List<User> users = new List<User>();

            foreach (dynamic userElement in responseJson.collection)
            {
                User user = new User
                {
                    AvatarUrl = userElement.avatar_url,
                    Urn = userElement.urn,
                    Username = userElement.username,
                    Id = userElement.urn.ToString().Replace("soundcloud:users:", "")
                };

                users.Add(user);
            }

            return users;
        }

        private async Task SearchUserByUrl(string userUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(userUrl);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Extract user data from HTML response
                    User user = ParseUserFromHtml(responseBody);

                    // Display user data in the panel
                    DisplayUsers(new List<User> { user });
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"An error occurred while making the request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private User ParseUserFromHtml(string html)
        {
            string avatarUrl = null;
            string id = null;
            string username = null;

            // Find avatar URL
            int avatarStart = html.IndexOf("https://i1.sndcdn.com/");
            if (avatarStart != -1)
            {
                int avatarEnd = html.IndexOf("\"", avatarStart);
                if (avatarEnd != -1)
                {
                    avatarUrl = html.Substring(avatarStart, avatarEnd - avatarStart);
                }
            }

            // Find user ID
            int idStart = html.IndexOf("soundcloud:users:");
            if (idStart != -1)
            {
                int idEnd = html.IndexOf("\"", idStart);
                if (idEnd != -1)
                {
                    id = html.Substring(idStart + "soundcloud:users:".Length, idEnd - idStart - "soundcloud:users:".Length);
                }
            }

            // Find username
            int usernameStart = html.IndexOf("<meta property=\"twitter:title\" content=\"");
            if (usernameStart != -1)
            {
                int contentStart = html.IndexOf("content=\"", usernameStart) + "content=\"".Length;
                int contentEnd = html.IndexOf("\"", contentStart);
                if (contentEnd != -1)
                {
                    username = html.Substring(contentStart, contentEnd - contentStart);
                }
            }

            // Create and return the User object
            return new User
            {
                AvatarUrl = avatarUrl,
                Urn = $"soundcloud:users:{id}",
                Username = username,
                Id = id
            };
        }

        private List<Track> ParseTrackResponse(string responseBody)
        {
            dynamic responseJson = JsonConvert.DeserializeObject(responseBody);
            List<Track> tracks = new List<Track>();

            foreach (dynamic trackElement in responseJson.collection)
            {
                if (trackElement.kind == "track")
                {
                    string permalinkUrl = trackElement.permalink_url;
                    string artworkUrl = trackElement.artwork_url ?? "default_artwork_url"; // Use a default image if artwork_url is null
                    string title = ((string)trackElement.title).Replace("`", " ").Replace("\"", " ").Replace(",", " ");

                    Track track = new Track
                    {
                        PermalinkUrl = permalinkUrl,
                        ArtworkUrl = artworkUrl,
                        Title = title
                    };

                    tracks.Add(track);
                }
            }

            return tracks;
        }

        private void DisplayUsers(List<User> users)
        {
            int top = 0;
            foreach (User user in users)
            {
                // Create a panel for each user
                Panel userPanelItem = new Panel();
                userPanelItem.AutoSize = true;
                userPanelItem.Width = userPanel.Width;
                userPanelItem.Height = userItemHeight;
                userPanelItem.Location = new Point(0, top);

                // Create a PictureBox for the user's image
                PictureBox pictureBox = new PictureBox();
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Width = 60;
                pictureBox.Height = 60;
                pictureBox.Location = new Point(10, 10);
                pictureBox.ImageLocation = user.AvatarUrl;

                // Create a Label for the user's name
                Label usernameLabel = new Label();
                usernameLabel.AutoSize = true;
                usernameLabel.Text = user.Username;
                usernameLabel.Font = new Font(usernameLabel.Font, FontStyle.Bold);
                usernameLabel.Location = new Point(pictureBox.Right + 10, 10);

                // Create a Label for the user's ID
                Label idLabel = new Label();
                idLabel.AutoSize = true;
                idLabel.Text = $"ID: {user.Id}";
                idLabel.Location = new Point(pictureBox.Right + 10, usernameLabel.Bottom + 5);

                // Create the "Fetch Likes" button
                Button fetchLikesButton = new Button();
                fetchLikesButton.Text = "Get Liked";
                fetchLikesButton.Location = new Point(pictureBox.Right + 10, idLabel.Bottom + 10);
                fetchLikesButton.Click += async (sender, e) =>
                {
                    SaveToHistory($"Likes: {user.Username}, {user.Id}, {user.AvatarUrl}");
                    mainForm.LoadHistory();
                    await FetchUserLikes(user.Id);
                };

                // Create the "Fetch Playlist" button
                Button fetchPlaylistButton = new Button();
                fetchPlaylistButton.Text = "Get Playlists";
                fetchPlaylistButton.Location = new Point(fetchLikesButton.Right + 10, idLabel.Bottom + 10);
                fetchPlaylistButton.Click += async (sender, e) =>
                {
                    await FetchUserPlaylists(user.Id);
                };

                // Create the "+" button
                Button addToHistoryButton = new Button();
                addToHistoryButton.Text = "+";
                addToHistoryButton.Location = new Point(fetchPlaylistButton.Right + 10, idLabel.Bottom + 10);
                addToHistoryButton.Click += (sender, e) =>
                {
                    SaveToHistory($"Likes: {user.Username}, {user.Id}, {user.AvatarUrl}");
                    mainForm.LoadHistory();
                };

                // Add PictureBox, Labels, and buttons to the user panel
                userPanelItem.Controls.Add(pictureBox);
                userPanelItem.Controls.Add(usernameLabel);
                userPanelItem.Controls.Add(idLabel);
                userPanelItem.Controls.Add(fetchLikesButton);
                userPanelItem.Controls.Add(fetchPlaylistButton);
                userPanelItem.Controls.Add(addToHistoryButton);

                // Add the user panel to the user list panel
                userPanel.Controls.Add(userPanelItem);

                // Increment top position for the next user item
                top += userItemHeight + 10; // Add some spacing between user items
            }
        }


        private async Task FetchUserPlaylists(string userId)
        {
            string apiUrl = $"https://api-v2.soundcloud.com/users/{userId}/playlists_without_albums?client_id={soundCloudClientId}&limit=10&offset=0&linked_partitioning=1&app_version=1717603207&app_locale=en";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Parse the response to extract playlist data
                    List<Playlist> playlists = ParsePlaylistResponse(responseBody);

                    // Clear previous search results
                    userPanel.Controls.Clear();

                    // Display the playlists in the panel
                    DisplayPlaylists(playlists);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"An error occurred while fetching playlists: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<Playlist> ParsePlaylistResponse(string responseBody)
        {
            Debug.WriteLine($"ParsePlaylistResponse called with responseBody: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}"); // Print first 200 chars of response
            dynamic responseJson = JsonConvert.DeserializeObject(responseBody);
            List<Playlist> playlists = new List<Playlist>();

            foreach (dynamic playlistElement in responseJson.collection)
            {
                Debug.WriteLine($"Parsing playlist element: {playlistElement}");

                Playlist playlist = new Playlist
                {
                    ArtworkUrl = playlistElement.artwork_url,
                    Id = playlistElement.id.ToString(),
                    Permalink = playlistElement.permalink,
                    Title = playlistElement.title
                };
                Debug.WriteLine($"Parsed playlist: {playlist.Title}, {playlist.Id}, {playlist.Permalink}, {playlist.ArtworkUrl}");

                playlists.Add(playlist);
            }

            return playlists;
        }

        private void DisplayPlaylists(List<Playlist> playlists)
        {
            int top = 0;
            foreach (Playlist playlist in playlists)
            {
                // Create a panel for each playlist
                Panel playlistPanelItem = new Panel();
                playlistPanelItem.AutoSize = true;
                playlistPanelItem.Width = userPanel.Width;
                playlistPanelItem.Height = userItemHeight;
                playlistPanelItem.Location = new Point(0, top);

                // Create a PictureBox for the playlist's artwork
                PictureBox pictureBox = new PictureBox();
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Width = 60;
                pictureBox.Height = 60;
                pictureBox.Location = new Point(10, 10);
                pictureBox.ImageLocation = playlist.ArtworkUrl;

                // Create a Label for the playlist's title
                Label titleLabel = new Label();
                titleLabel.AutoSize = true;
                titleLabel.Text = playlist.Title;
                titleLabel.Font = new Font(titleLabel.Font, FontStyle.Bold);
                titleLabel.Location = new Point(pictureBox.Right + 10, 10);

                // Create a Label for the playlist's URL
                Label urlLabel = new Label();
                urlLabel.AutoSize = true;
                urlLabel.Text = $"URL: {playlist.Permalink}";
                urlLabel.Location = new Point(pictureBox.Right + 10, titleLabel.Bottom + 5);

                // Create the "Fetch Tracks" button
                Button fetchTracksButton = new Button();
                fetchTracksButton.Text = "Get Playlist Tracks";
                fetchTracksButton.Location = new Point(pictureBox.Right + 10, urlLabel.Bottom + 10);
                fetchTracksButton.Click += async (sender, e) =>
                {
                    SaveToHistory($"Playlist: {playlist.Title}, {playlist.Id}, {playlist.Permalink}, {playlist.ArtworkUrl}");
                    mainForm.LoadHistory();
                    await FetchPlaylistTracks(playlist.Id, playlist.Title, playlist.Permalink, playlist.ArtworkUrl);
                };

                // Create the "+" button
                Button addToHistoryButton = new Button();
                addToHistoryButton.Text = "+";
                addToHistoryButton.Location = new Point(fetchTracksButton.Right + 10, urlLabel.Bottom + 10);
                addToHistoryButton.Click += (sender, e) =>
                {
                    SaveToHistory($"Playlist: {playlist.Title}, {playlist.Id}, {playlist.Permalink}, {playlist.ArtworkUrl}");
                    mainForm.LoadHistory();
                };

                // Add PictureBox, Labels, and "Fetch Tracks" button to the playlist panel
                playlistPanelItem.Controls.Add(pictureBox);
                playlistPanelItem.Controls.Add(titleLabel);
                playlistPanelItem.Controls.Add(urlLabel);
                playlistPanelItem.Controls.Add(fetchTracksButton);
                playlistPanelItem.Controls.Add(addToHistoryButton);

                // Add the playlist panel to the playlist list panel
                userPanel.Controls.Add(playlistPanelItem);

                // Increment top position for the next playlist item
                top += userItemHeight + 10; // Add some spacing between playlist items
            }
        }


        public async Task FetchPlaylistTracks(string playlistId, string playlistTitle, string playlistPermalink, string playlistArtworkUrl)
        {
            string apiUrl = $"https://api-v2.soundcloud.com/playlists/{playlistId}?representation=full&client_id={soundCloudClientId}&app_version=1717603207&app_locale=en";
            Debug.WriteLine($"FetchPlaylistTracks called with playlistId: {playlistId}, playlistTitle: {playlistTitle}, playlistPermalink: {playlistPermalink}, playlistArtworkUrl: {playlistArtworkUrl}");
            Debug.WriteLine($"API URL: {apiUrl}");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    Debug.WriteLine($"HTTP response status code: {response.StatusCode}");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Response body: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}"); // Print first 200 chars of response

                    // Parse the response to extract track IDs
                    List<string> trackIds = ParsePlaylistTracksResponse(responseBody);
                    Debug.WriteLine($"Track IDs parsed: {string.Join(", ", trackIds)}");

                    if (trackIds.Count > 0)
                    {
                        // Fetch track details
                        await FetchTrackDetails(trackIds, playlistTitle, playlistPermalink, playlistArtworkUrl, playlistId);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP request exception: {ex.Message}");
                MessageBox.Show($"An error occurred while fetching playlist tracks: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> ParsePlaylistTracksResponse(string responseBody)
        {
            dynamic responseJson = JsonConvert.DeserializeObject(responseBody);
            List<string> trackIds = new List<string>();

            foreach (dynamic trackElement in responseJson.tracks)
            {
                string trackId = trackElement.id?.ToString();
                trackIds.Add(trackId);
                
                
            }

            return trackIds;
        }



        private async Task FetchTrackDetails(List<string> trackIds, string playlistTitle, string playlistPermalink, string playlistArtworkUrl, string playlistId)
        {
            // Define the maximum number of IDs that can be included in one API request
            int maxIdsPerRequest = 50;

            // Split the track IDs into batches
            var trackIdBatches = trackIds.Select((id, index) => new { id, index })
                                         .GroupBy(x => x.index / maxIdsPerRequest)
                                         .Select(group => group.Select(x => x.id).ToList())
                                         .ToList();

            List<string> trackInfoList = new List<string>();

            foreach (var trackIdBatch in trackIdBatches)
            {
                string ids = string.Join(",", trackIdBatch);
                string apiUrl = $"https://api-v2.soundcloud.com/tracks?ids={ids}&client_id={soundCloudClientId}&app_version=1717603207&app_locale=en";
                Debug.WriteLine($"FetchTrackDetails called with trackIds: {ids}, playlistTitle: {playlistTitle}, playlistPermalink: {playlistPermalink}, playlistArtworkUrl: {playlistArtworkUrl}, playlistId: {playlistId}");
                Debug.WriteLine($"API URL: {apiUrl}");

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        Debug.WriteLine("Sending HTTP request to fetch track details...");
                        HttpResponseMessage response = await client.GetAsync(apiUrl);
                        Debug.WriteLine($"HTTP response status code: {response.StatusCode}");
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"Response body: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}"); // Print first 200 chars of response

                        // Parse the response to extract track details
                        List<Track> tracks = ParsePlaylistTrackResponse(responseBody);
                        Debug.WriteLine($"Number of tracks parsed: {tracks.Count}");

                        foreach (Track track in tracks)
                        {
                            Debug.WriteLine($"Parsed track: {track.Title}, {track.PermalinkUrl}");
                            trackInfoList.Add($"{track.PermalinkUrl}, {track.Title}");
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine($"HTTP request exception: {ex.Message}");
                    MessageBox.Show($"An error occurred while fetching track details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Save the track details to a temporary CSV file
            string tempCsvFilePath = SaveTempCsv(trackInfoList);
            Debug.WriteLine($"Temporary CSV file path: {tempCsvFilePath}");

            // Save the action to history
            

            // Directly call the method in Form1 to use the tracklist
            mainForm.ReceiveTracklist(tempCsvFilePath);
            Debug.WriteLine("Tracklist sent to main form.");
            Debug.WriteLine("History loaded in main form.");
        }




        private List<Track> ParsePlaylistTrackResponse(string responseBody)
        {
            dynamic responseJson = JsonConvert.DeserializeObject(responseBody);
            List<Track> tracks = new List<Track>();

            foreach (dynamic trackElement in responseJson)
            {
                Track track = new Track
                {
                    PermalinkUrl = trackElement.permalink_url,
                    Title = trackElement.title
                };
                tracks.Add(track);
            }

            return tracks;
        }


        public async Task FetchUserLikes(string userId)
        {
            string apiUrl = $"https://api-v2.soundcloud.com/users/{userId}/likes?client_id={soundCloudClientId}&limit=1000&offset=0&linked_partitioning=1&app_version=1717603207&app_locale=en";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Parse the response to extract permalink and title for each track
                    List<string> trackInfoList = ParseLikesResponse(responseBody);

                    // Save the tracklist as a temporary CSV file
                    string tempCsvFilePath = SaveTempCsv(trackInfoList);

                    // Directly call the method in Form1 to use the tracklist
                    mainForm.ReceiveTracklist(tempCsvFilePath);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"An error occurred while fetching likes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> ParseLikesResponse(string responseBody)
        {
            List<string> trackInfoList = new List<string>();

            try
            {
                dynamic responseJson = JsonConvert.DeserializeObject(responseBody);

                foreach (dynamic element in responseJson.collection)
                {
                    if (element.track != null) // Ensure it's a track and not a playlist
                    {
                        string permalink = element.track.permalink_url;
                        string title = ((string)element.track.title).Replace("`", " ").Replace("\"", " ").Replace(",", " ");

                        string trackInfo = $"{permalink}, {title}";
                        trackInfoList.Add(trackInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception as needed
            }

            return trackInfoList;
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


        private void SaveToHistory(string entry)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(historyFilePath, true))
                {
                    sw.WriteLine($"{DateTime.Now}: {entry}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayTracks(List<Track> tracks)
        {
            int top = 0;
            foreach (Track track in tracks)
            {
                // Create a panel for each track
                Panel trackPanelItem = new Panel();
                trackPanelItem.AutoSize = true;
                trackPanelItem.Width = userPanel.Width;
                trackPanelItem.Height = userItemHeight;
                trackPanelItem.Location = new Point(0, top);

                // Create a PictureBox for the track's artwork
                PictureBox pictureBox = new PictureBox();
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Width = 60;
                pictureBox.Height = 60;
                pictureBox.Location = new Point(10, 10);
                pictureBox.ImageLocation = track.ArtworkUrl;

                // Create a Label for the track's title
                Label titleLabel = new Label();
                titleLabel.AutoSize = true;
                titleLabel.Text = track.Title;
                titleLabel.Font = new Font(titleLabel.Font, FontStyle.Bold);
                titleLabel.Location = new Point(pictureBox.Right + 10, 10);

                // Create a Label for the track's URL
                Label urlLabel = new Label();
                urlLabel.AutoSize = true;
                urlLabel.Text = $"URL: {track.PermalinkUrl}";
                urlLabel.Location = new Point(pictureBox.Right + 10, titleLabel.Bottom + 5);

                // Create the "Play" button
                Button playButton = new Button();
                playButton.Text = "Play";
                playButton.ForeColor = Color.White;
                playButton.Location = new Point(pictureBox.Right + 10, urlLabel.Bottom + 10);
                playButton.Click += (sender, e) =>
                {
                    // Save the track URL and title to a temporary CSV file
                    string tempCsvFilePath = SaveTempCsv(new List<string> { $"{track.PermalinkUrl}, {track.Title}" });

                    // Save the action to history
                    SaveToHistory($"Track: {track.Title}, {track.PermalinkUrl}, {track.ArtworkUrl}");

                    // Directly call the method in Form1 to use the tracklist
                    mainForm.ReceiveTracklist(tempCsvFilePath);
                    mainForm.LoadHistory();
                };

                // Create the "+" button
                Button addToHistoryButton = new Button();
                addToHistoryButton.Text = "+";
                addToHistoryButton.Location = new Point(playButton.Right + 10, urlLabel.Bottom + 10);
                addToHistoryButton.Click += (sender, e) =>
                {
                    SaveToHistory($"Track: {track.Title}, {track.PermalinkUrl}, {track.ArtworkUrl}");
                    mainForm.LoadHistory();
                };

                // Add PictureBox, Labels, and "Play" button to the track panel
                trackPanelItem.Controls.Add(pictureBox);
                trackPanelItem.Controls.Add(titleLabel);
                trackPanelItem.Controls.Add(urlLabel);
                trackPanelItem.Controls.Add(playButton);
                trackPanelItem.Controls.Add(addToHistoryButton);

                // Add the track panel to the track list panel
                userPanel.Controls.Add(trackPanelItem);

                // Increment top position for the next track item
                top += userItemHeight + 10; // Add some spacing between track items
            }
        }


        public class User
        {
            public string AvatarUrl { get; set; }
            public string Id { get; set; }
            public string Username { get; set; }
            public string Urn { get; set; }
        }

        public class Track
        {
            public string PermalinkUrl { get; set; }
            public string ArtworkUrl { get; set; }
            public string Title { get; set; }
        }

        public class Playlist
        {
            public string ArtworkUrl { get; set; }
            public string Id { get; set; }
            public string Permalink { get; set; }
            public string Title { get; set; }
        }
    }
}
