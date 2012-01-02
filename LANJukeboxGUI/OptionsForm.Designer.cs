namespace LANJukeboxGUI
{
    partial class OptionsForm
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label labelHistorySize;
            System.Windows.Forms.Label labelLastFm;
            this.comboBoxDevices = new System.Windows.Forms.ComboBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.numericUpDownHistorySize = new System.Windows.Forms.NumericUpDown();
            this.buttonLastFm = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            labelHistorySize = new System.Windows.Forms.Label();
            labelLastFm = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHistorySize)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 15);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(69, 13);
            label1.TabIndex = 1;
            label1.Text = "Audio device";
            // 
            // labelHistorySize
            // 
            labelHistorySize.AutoSize = true;
            labelHistorySize.Location = new System.Drawing.Point(12, 44);
            labelHistorySize.Name = "labelHistorySize";
            labelHistorySize.Size = new System.Drawing.Size(62, 13);
            labelHistorySize.TabIndex = 5;
            labelHistorySize.Text = "History Size";
            // 
            // labelLastFm
            // 
            labelLastFm.AutoSize = true;
            labelLastFm.Location = new System.Drawing.Point(12, 73);
            labelLastFm.Name = "labelLastFm";
            labelLastFm.Size = new System.Drawing.Size(41, 13);
            labelLastFm.TabIndex = 7;
            labelLastFm.Text = "Last.fm";
            // 
            // comboBoxDevices
            // 
            this.comboBoxDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDevices.FormattingEnabled = true;
            this.comboBoxDevices.Location = new System.Drawing.Point(87, 12);
            this.comboBoxDevices.Name = "comboBoxDevices";
            this.comboBoxDevices.Size = new System.Drawing.Size(366, 21);
            this.comboBoxDevices.TabIndex = 2;
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(378, 151);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 3;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(297, 151);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // numericUpDownHistorySize
            // 
            this.numericUpDownHistorySize.Location = new System.Drawing.Point(87, 42);
            this.numericUpDownHistorySize.Name = "numericUpDownHistorySize";
            this.numericUpDownHistorySize.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownHistorySize.TabIndex = 6;
            // 
            // buttonLastFm
            // 
            this.buttonLastFm.Location = new System.Drawing.Point(87, 68);
            this.buttonLastFm.Name = "buttonLastFm";
            this.buttonLastFm.Size = new System.Drawing.Size(89, 23);
            this.buttonLastFm.TabIndex = 8;
            this.buttonLastFm.Text = "Authenticate";
            this.buttonLastFm.UseVisualStyleBackColor = true;
            this.buttonLastFm.Click += new System.EventHandler(this.buttonLastFm_Click);
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(465, 186);
            this.Controls.Add(this.buttonLastFm);
            this.Controls.Add(labelLastFm);
            this.Controls.Add(this.numericUpDownHistorySize);
            this.Controls.Add(labelHistorySize);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.comboBoxDevices);
            this.Controls.Add(label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "OptionsForm";
            this.Text = "Options";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHistorySize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxDevices;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.NumericUpDown numericUpDownHistorySize;
        private System.Windows.Forms.Button buttonLastFm;
    }
}