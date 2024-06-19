namespace SoundCloud
{
    partial class ClientIdInputForm
    {
        private System.ComponentModel.IContainer components = null;

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
            this.lblClientId = new System.Windows.Forms.Label();
            this.txtClientId = new System.Windows.Forms.TextBox();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.btnOpenLink = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblClientId
            // 
            this.lblClientId.AutoSize = true;
            this.lblClientId.ForeColor = System.Drawing.Color.White;
            this.lblClientId.Location = new System.Drawing.Point(13, 13);
            this.lblClientId.Name = "lblClientId";
            this.lblClientId.Size = new System.Drawing.Size(78, 13);
            this.lblClientId.TabIndex = 0;
            this.lblClientId.Text = "Enter Client ID:";
            // 
            // txtClientId
            // 
            this.txtClientId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtClientId.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtClientId.ForeColor = System.Drawing.Color.White;
            this.txtClientId.Location = new System.Drawing.Point(16, 30);
            this.txtClientId.Name = "txtClientId";
            this.txtClientId.Size = new System.Drawing.Size(250, 13);
            this.txtClientId.TabIndex = 1;
            // 
            // btnSubmit
            // 
            this.btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSubmit.ForeColor = System.Drawing.Color.White;
            this.btnSubmit.Location = new System.Drawing.Point(16, 67);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(75, 23);
            this.btnSubmit.TabIndex = 2;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // btnOpenLink
            // 
            this.btnOpenLink.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenLink.ForeColor = System.Drawing.Color.White;
            this.btnOpenLink.Location = new System.Drawing.Point(163, 67);
            this.btnOpenLink.Name = "btnOpenLink";
            this.btnOpenLink.Size = new System.Drawing.Size(103, 23);
            this.btnOpenLink.TabIndex = 3;
            this.btnOpenLink.Text = "Get Client ID";
            this.btnOpenLink.UseVisualStyleBackColor = true;
            this.btnOpenLink.Click += new System.EventHandler(this.btnOpenLink_Click);
            // 
            // ClientIdInputForm
            // 
            this.AcceptButton = this.btnSubmit;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(284, 102);
            this.Controls.Add(this.btnOpenLink);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.txtClientId);
            this.Controls.Add(this.lblClientId);
            this.Name = "ClientIdInputForm";
            this.Text = "SoundCloud Client ID";
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        private System.Windows.Forms.Label lblClientId;
        private System.Windows.Forms.TextBox txtClientId;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Button btnOpenLink;
    }
}
