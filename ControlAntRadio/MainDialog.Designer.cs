namespace ControlAntRadio
{
    partial class MainDialog
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
            this.buttonStart = new System.Windows.Forms.Button();
            this.checkBoxUsb = new System.Windows.Forms.CheckBox();
            this.listViewSensors = new System.Windows.Forms.ListView();
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonStart.Location = new System.Drawing.Point(0, 0);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 30);
            this.buttonStart.TabIndex = 1;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // checkBoxUsb
            // 
            this.checkBoxUsb.AutoSize = true;
            this.checkBoxUsb.Dock = System.Windows.Forms.DockStyle.Right;
            this.checkBoxUsb.Location = new System.Drawing.Point(265, 0);
            this.checkBoxUsb.Name = "checkBoxUsb";
            this.checkBoxUsb.Size = new System.Drawing.Size(48, 30);
            this.checkBoxUsb.TabIndex = 2;
            this.checkBoxUsb.Text = "USB";
            this.checkBoxUsb.UseVisualStyleBackColor = true;
            // 
            // listViewSensors
            // 
            this.listViewSensors.AutoArrange = false;
            this.listViewSensors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderType,
            this.columnHeaderNumber});
            this.listViewSensors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSensors.GridLines = true;
            this.listViewSensors.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewSensors.Location = new System.Drawing.Point(0, 0);
            this.listViewSensors.Name = "listViewSensors";
            this.listViewSensors.Size = new System.Drawing.Size(313, 466);
            this.listViewSensors.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.listViewSensors.TabIndex = 3;
            this.listViewSensors.UseCompatibleStateImageBehavior = false;
            this.listViewSensors.View = System.Windows.Forms.View.Details;
            this.listViewSensors.DoubleClick += new System.EventHandler(this.listViewSensors_DoubleClick);
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "Device";
            this.columnHeaderType.Width = 172;
            // 
            // columnHeaderNumber
            // 
            this.columnHeaderNumber.Text = "ID";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonStart);
            this.panel1.Controls.Add(this.checkBoxUsb);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 436);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(313, 30);
            this.panel1.TabIndex = 4;
            // 
            // MainDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(313, 466);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.listViewSensors);
            this.Name = "MainDialog";
            this.Text = "ANT Radio Control Example";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.CheckBox checkBoxUsb;
        private System.Windows.Forms.ListView listViewSensors;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderNumber;
        private System.Windows.Forms.Panel panel1;
    }
}

