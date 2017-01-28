namespace DemoApp
{
    partial class GraphicsView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._bottomLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _bottomLabel
            // 
            this._bottomLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._bottomLabel.AutoSize = true;
            this._bottomLabel.Location = new System.Drawing.Point(14, 490);
            this._bottomLabel.Name = "_bottomLabel";
            this._bottomLabel.Size = new System.Drawing.Size(36, 25);
            this._bottomLabel.TabIndex = 0;
            this._bottomLabel.Text = "10";
            this._bottomLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // GraphicsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._bottomLabel);
            this.Name = "GraphicsView";
            this.Size = new System.Drawing.Size(762, 531);
            this.Load += new System.EventHandler(this.GraphicsView_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.GraphicsView_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _bottomLabel;
    }
}
