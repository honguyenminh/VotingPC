namespace FingerGet;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        this.serialOutput = new System.Windows.Forms.TextBox();
        this.label1 = new System.Windows.Forms.Label();
        this.submitButton = new System.Windows.Forms.Button();
        this.statusLabel = new System.Windows.Forms.Label();
        this.status = new System.Windows.Forms.Label();
        this.ClearButton = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // serialOutput
        // 
        this.serialOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                                                          | System.Windows.Forms.AnchorStyles.Left) 
                                                                         | System.Windows.Forms.AnchorStyles.Right)));
        this.serialOutput.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.serialOutput.Location = new System.Drawing.Point(12, 59);
        this.serialOutput.Multiline = true;
        this.serialOutput.Name = "serialOutput";
        this.serialOutput.ReadOnly = true;
        this.serialOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.serialOutput.Size = new System.Drawing.Size(776, 300);
        this.serialOutput.TabIndex = 1;
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.label1.Location = new System.Drawing.Point(12, 37);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(95, 19);
        this.label1.TabIndex = 2;
        this.label1.Text = "Serial Monitor";
        // 
        // submitButton
        // 
        this.submitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.submitButton.Enabled = false;
        this.submitButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.submitButton.Location = new System.Drawing.Point(598, 12);
        this.submitButton.Name = "submitButton";
        this.submitButton.Size = new System.Drawing.Size(190, 41);
        this.submitButton.TabIndex = 0;
        this.submitButton.Text = "Nhập dấu vân tay";
        this.submitButton.UseVisualStyleBackColor = true;
        this.submitButton.Click += new System.EventHandler(this.SubmitButton_Click);
        // 
        // statusLabel
        // 
        this.statusLabel.AutoSize = true;
        this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.statusLabel.Location = new System.Drawing.Point(12, 12);
        this.statusLabel.Name = "statusLabel";
        this.statusLabel.Size = new System.Drawing.Size(50, 19);
        this.statusLabel.TabIndex = 3;
        this.statusLabel.Text = "Status:";
        // 
        // status
        // 
        this.status.AutoSize = true;
        this.status.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.status.ForeColor = System.Drawing.Color.Red;
        this.status.Location = new System.Drawing.Point(60, 12);
        this.status.Name = "status";
        this.status.Size = new System.Drawing.Size(128, 19);
        this.status.TabIndex = 4;
        this.status.Text = "Đang tìm Arduino...";
        // 
        // ClearButton
        // 
        this.ClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ClearButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.ClearButton.Location = new System.Drawing.Point(500, 12);
        this.ClearButton.Name = "ClearButton";
        this.ClearButton.Size = new System.Drawing.Size(92, 41);
        this.ClearButton.TabIndex = 5;
        this.ClearButton.Text = "Xóa log";
        this.ClearButton.UseVisualStyleBackColor = true;
        this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 368);
        this.Controls.Add(this.ClearButton);
        this.Controls.Add(this.status);
        this.Controls.Add(this.statusLabel);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.serialOutput);
        this.Controls.Add(this.submitButton);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MinimumSize = new System.Drawing.Size(700, 300);
        this.Name = "MainForm";
        this.Text = "FingerGet - Nhập vân tay";
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.TextBox serialOutput;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button submitButton;
    private System.Windows.Forms.Label statusLabel;
    private System.Windows.Forms.Label status;
    private System.Windows.Forms.Button ClearButton;
}