using SoundCloudPlayer;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundCloud
{
    static class Program
    {
        private const string ClientIdFilePath = "clientId.txt";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Retrieve the last entered SoundCloud Client ID from the file
            string lastClientId = LoadLastClientId();

            // Check the session status of the last used Client ID
            if (!string.IsNullOrEmpty(lastClientId))
            {
                Task<bool> sessionTask = CheckSession(lastClientId);
                sessionTask.Wait();

                bool sessionStatus = sessionTask.Result;

                if (sessionStatus)
                {
                    // Run Form1 with the valid SoundCloud Client ID
                    Application.Run(new Form1(lastClientId));
                    return; // Exit the method to prevent showing the input form
                }
            }

            // If the last Client ID is invalid or not provided, show the ClientIdInputForm
            using (var clientIdInputForm = new ClientIdInputForm(lastClientId))
            {
                if (clientIdInputForm.ShowDialog() == DialogResult.OK)
                {
                    string soundCloudClientId = clientIdInputForm.ClientId;

                    if (!string.IsNullOrEmpty(soundCloudClientId))
                    {
                        Task<bool> sessionTask = CheckSession(soundCloudClientId);
                        sessionTask.Wait();

                        bool sessionStatus = sessionTask.Result;

                        if (sessionStatus)
                        {
                            SaveClientId(soundCloudClientId);
                            Application.Run(new Form1(soundCloudClientId));
                        }
                        else
                        {
                            MessageBox.Show("Wrong SoundCloud Client ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No SoundCloud Client ID provided. Exiting application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        static async Task<bool> CheckSession(string clientId)
        {
            using (var client = new HttpClient())
            {
                string url = $"https://api-auth.soundcloud.com/oauth/session?client_id={clientId}";

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (responseContent.Contains("\"session\":false") || responseContent.Contains("\"session\":true"))
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Unexpected response from SoundCloud API", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Failed to communicate with SoundCloud API", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        static void SaveClientId(string clientId)
        {
            try
            {
                File.WriteAllText(ClientIdFilePath, clientId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save SoundCloud Client ID: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static string LoadLastClientId()
        {
            try
            {
                if (File.Exists(ClientIdFilePath))
                {
                    return File.ReadAllText(ClientIdFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load SoundCloud Client ID: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return string.Empty;
        }
    }
}
