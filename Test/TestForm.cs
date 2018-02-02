using System;
using System.Windows.Forms;
using com.google.openlocationcode;

namespace Test
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        private void myEncodeButton_Click(object sender, EventArgs e)
        {
            try
            {
                string[] values = myLatLonTextBox.Text.Split(' ');
                if (values.Length != 2)
                    return;
                double lat = Double.Parse(values[0]);
                double lon = Double.Parse(values[1]);

                myEncodedTextBox.Text = OpenLocationCode.Encode(lat, lon);
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, string.Format("Error encoding: {0}", ex.Message), 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void myDecodeButton_Click(object sender, EventArgs e)
        {
            try
            {
                var codeArea = OpenLocationCode.Decode(myEncodedTextBox.Text);
                myLatLonTextBox.Text = string.Format("{0} {1}", codeArea.CenterLatitude, codeArea.CenterLongitude);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, string.Format("Error encoding: {0}", ex.Message),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
