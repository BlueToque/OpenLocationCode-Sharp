namespace Test
{
    partial class TestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            this.myLatLonTextBox = new System.Windows.Forms.TextBox();
            this.myEncodeButton = new System.Windows.Forms.Button();
            this.myDecodeButton = new System.Windows.Forms.Button();
            this.myEncodedTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // myLatLonTextBox
            // 
            this.myLatLonTextBox.Location = new System.Drawing.Point(29, 43);
            this.myLatLonTextBox.Name = "myLatLonTextBox";
            this.myLatLonTextBox.Size = new System.Drawing.Size(217, 20);
            this.myLatLonTextBox.TabIndex = 0;
            // 
            // myEncodeButton
            // 
            this.myEncodeButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.myEncodeButton.Location = new System.Drawing.Point(252, 25);
            this.myEncodeButton.Name = "myEncodeButton";
            this.myEncodeButton.Size = new System.Drawing.Size(75, 23);
            this.myEncodeButton.TabIndex = 1;
            this.myEncodeButton.Text = "Encode";
            this.myEncodeButton.UseVisualStyleBackColor = true;
            this.myEncodeButton.Click += new System.EventHandler(this.myEncodeButton_Click);
            // 
            // myDecodeButton
            // 
            this.myDecodeButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.myDecodeButton.Location = new System.Drawing.Point(252, 54);
            this.myDecodeButton.Name = "myDecodeButton";
            this.myDecodeButton.Size = new System.Drawing.Size(75, 23);
            this.myDecodeButton.TabIndex = 2;
            this.myDecodeButton.Text = "Decode";
            this.myDecodeButton.UseVisualStyleBackColor = true;
            this.myDecodeButton.Click += new System.EventHandler(this.myDecodeButton_Click);
            // 
            // myEncodedTextBox
            // 
            this.myEncodedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.myEncodedTextBox.Location = new System.Drawing.Point(333, 43);
            this.myEncodedTextBox.Name = "myEncodedTextBox";
            this.myEncodedTextBox.Size = new System.Drawing.Size(217, 20);
            this.myEncodedTextBox.TabIndex = 3;
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(579, 97);
            this.Controls.Add(this.myEncodedTextBox);
            this.Controls.Add(this.myDecodeButton);
            this.Controls.Add(this.myEncodeButton);
            this.Controls.Add(this.myLatLonTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TestForm";
            this.Text = "Test Open Location Code";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox myLatLonTextBox;
        private System.Windows.Forms.Button myEncodeButton;
        private System.Windows.Forms.Button myDecodeButton;
        private System.Windows.Forms.TextBox myEncodedTextBox;
    }
}

