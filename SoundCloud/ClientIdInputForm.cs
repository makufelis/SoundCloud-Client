using System;
using System.Windows.Forms;

namespace SoundCloud
{
    partial class ClientIdInputForm : Form
    {
        public string ClientId { get; private set; }

        public ClientIdInputForm(string lastClientId)
        {
            InitializeComponent();
            txtClientId.Text = lastClientId;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            // Get the client ID from the TextBox
            ClientId = txtClientId.Text.Trim();

            // Close the form
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnOpenLink_Click(object sender, EventArgs e)
        {
            string url = "https://pastebin.com/raw/6CPjZ3qt";
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open link: {ex.Message}");
            }
        }

    }
}
