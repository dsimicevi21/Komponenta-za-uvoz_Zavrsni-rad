namespace ImportComponent.UI.WinForms;
partial class Form1
{
   
    private System.ComponentModel.IContainer components = null;

    private static readonly System.Drawing.Color AccentColor = System.Drawing.Color.FromArgb(37, 99, 235);
    private static readonly System.Drawing.Color PageBackColor = System.Drawing.Color.FromArgb(245, 247, 250);

    private System.Windows.Forms.Panel _titleBar;
    private System.Windows.Forms.Label _titleLabel;

    private System.Windows.Forms.GroupBox _connectionGroup;
    private System.Windows.Forms.Label _apiUrlLabel;
    private System.Windows.Forms.TextBox _apiUrlTextBox;
    private System.Windows.Forms.Button _loadTargetsButton;

    private System.Windows.Forms.GroupBox _targetGroup;
    private System.Windows.Forms.Label _targetSystemLabel;
    private System.Windows.Forms.ComboBox _targetSystemCombo;
    private System.Windows.Forms.Label _targetTableLabel;
    private System.Windows.Forms.ComboBox _targetTableCombo;

    private System.Windows.Forms.GroupBox _sourceGroup;
    private System.Windows.Forms.Button _selectFileButton;
    private System.Windows.Forms.Label _selectedFileLabel;
    private System.Windows.Forms.Label _entityLabel;
    private System.Windows.Forms.ComboBox _entityCombo;

    private System.Windows.Forms.Button _runButton;
    private System.Windows.Forms.Button _confirmButton;
    private System.Windows.Forms.Button _discardButton;

    private System.Windows.Forms.GroupBox _resetGroup;
    private System.Windows.Forms.Label _resetTargetSystemLabel;
    private System.Windows.Forms.ComboBox _resetTargetSystemCombo;
    private System.Windows.Forms.Label _resetTargetTableLabel;
    private System.Windows.Forms.ComboBox _resetTargetTableCombo;
    private System.Windows.Forms.Button _resetButton;

    private System.Windows.Forms.GroupBox _mappingGroup;
    private System.Windows.Forms.DataGridView _mappingGrid;
    private System.Windows.Forms.DataGridViewTextBoxColumn _sourceFieldColumn;
    private System.Windows.Forms.DataGridViewComboBoxColumn _targetColumnColumn;

    private System.Windows.Forms.GroupBox _stagedGroup;
    private System.Windows.Forms.DataGridView _stagedRecordsGrid;

    private System.Windows.Forms.GroupBox _logGroup;
    private System.Windows.Forms.TextBox _resultTextBox;

    private System.Windows.Forms.Label _statusLabel;


    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

 
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();

        this._titleBar = new Panel();
        this._titleLabel = new Label();

        this._connectionGroup = new GroupBox();
        this._apiUrlLabel = new Label();
        this._apiUrlTextBox = new TextBox();
        this._loadTargetsButton = new Button();

        this._targetGroup = new GroupBox();
        this._targetSystemLabel = new Label();
        this._targetSystemCombo = new ComboBox();
        this._targetTableLabel = new Label();
        this._targetTableCombo = new ComboBox();

        this._sourceGroup = new GroupBox();
        this._selectFileButton = new Button();
        this._selectedFileLabel = new Label();
        this._entityLabel = new Label();
        this._entityCombo = new ComboBox();

        this._runButton = new Button();
        this._confirmButton = new Button();
        this._discardButton = new Button();

        this._resetGroup = new GroupBox();
        this._resetTargetSystemLabel = new Label();
        this._resetTargetSystemCombo = new ComboBox();
        this._resetTargetTableLabel = new Label();
        this._resetTargetTableCombo = new ComboBox();
        this._resetButton = new Button();

        this._mappingGroup = new GroupBox();
        this._mappingGrid = new DataGridView();
        this._sourceFieldColumn = new DataGridViewTextBoxColumn();
        this._targetColumnColumn = new DataGridViewComboBoxColumn();

        this._stagedGroup = new GroupBox();
        this._stagedRecordsGrid = new DataGridView();

        this._logGroup = new GroupBox();
        this._resultTextBox = new TextBox();

        this._statusLabel = new Label();

        ((System.ComponentModel.ISupportInitialize)(this._mappingGrid)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this._stagedRecordsGrid)).BeginInit();
        this.SuspendLayout();

        this._titleBar.BackColor = AccentColor;
        this._titleBar.Location = new System.Drawing.Point(0, 0);
        this._titleBar.Size = new System.Drawing.Size(1200, 52);
        this._titleBar.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)));

        this._titleLabel.AutoSize = true;
        this._titleLabel.ForeColor = System.Drawing.Color.White;
        this._titleLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        this._titleLabel.Location = new System.Drawing.Point(20, 13);
        this._titleLabel.Text = "Import Component";
        this._titleBar.Controls.Add(this._titleLabel);

        this._connectionGroup.Text = "Spajanje na API";
        this._connectionGroup.Location = new System.Drawing.Point(12, 62);
        this._connectionGroup.Size = new System.Drawing.Size(360, 90);

        this._apiUrlLabel.AutoSize = true;
        this._apiUrlLabel.Location = new System.Drawing.Point(12, 28);
        this._apiUrlLabel.Text = "API URL:";

        this._apiUrlTextBox.Location = new System.Drawing.Point(12, 48);
        this._apiUrlTextBox.Size = new System.Drawing.Size(220, 23);
        this._apiUrlTextBox.Text = "http://localhost:5183";

        this._loadTargetsButton.Location = new System.Drawing.Point(240, 47);
        this._loadTargetsButton.Size = new System.Drawing.Size(104, 25);
        this._loadTargetsButton.Text = "Load targets";
        this._loadTargetsButton.Click += new System.EventHandler(this.LoadTargetsButton_Click);

        this._connectionGroup.Controls.Add(this._apiUrlLabel);
        this._connectionGroup.Controls.Add(this._apiUrlTextBox);
        this._connectionGroup.Controls.Add(this._loadTargetsButton);

        this._targetGroup.Text = "Odabir ciljnog sustava i tablice";
        this._targetGroup.Location = new System.Drawing.Point(12, 160);
        this._targetGroup.Size = new System.Drawing.Size(360, 112);

        this._targetSystemLabel.AutoSize = true;
        this._targetSystemLabel.Location = new System.Drawing.Point(12, 28);
        this._targetSystemLabel.Text = "Target system:";

        this._targetSystemCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        this._targetSystemCombo.Location = new System.Drawing.Point(12, 48);
        this._targetSystemCombo.Size = new System.Drawing.Size(332, 23);
        this._targetSystemCombo.SelectedIndexChanged += new System.EventHandler(this.TargetSystemCombo_SelectedIndexChanged);

        this._targetTableLabel.AutoSize = true;
        this._targetTableLabel.Location = new System.Drawing.Point(12, 76);
        this._targetTableLabel.Text = "Target table:";

        this._targetTableCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        this._targetTableCombo.Location = new System.Drawing.Point(100, 73);
        this._targetTableCombo.Size = new System.Drawing.Size(244, 23);
        this._targetTableCombo.SelectedIndexChanged += new System.EventHandler(this.TargetTableCombo_SelectedIndexChanged);

        this._targetGroup.Controls.Add(this._targetSystemLabel);
        this._targetGroup.Controls.Add(this._targetSystemCombo);
        this._targetGroup.Controls.Add(this._targetTableLabel);
        this._targetGroup.Controls.Add(this._targetTableCombo);

        this._sourceGroup.Text = "Odabir importa";
        this._sourceGroup.Location = new System.Drawing.Point(12, 280);
        this._sourceGroup.Size = new System.Drawing.Size(360, 118);

        this._selectFileButton.Location = new System.Drawing.Point(12, 25);
        this._selectFileButton.Size = new System.Drawing.Size(140, 25);
        this._selectFileButton.Text = "Select file...";
        this._selectFileButton.Click += new System.EventHandler(this.SelectFileButton_Click);

        this._selectedFileLabel.AutoSize = false;
        this._selectedFileLabel.Location = new System.Drawing.Point(160, 29);
        this._selectedFileLabel.Size = new System.Drawing.Size(188, 34);
        this._selectedFileLabel.Text = "(no file selected)";

        this._entityLabel.AutoSize = true;
        this._entityLabel.Location = new System.Drawing.Point(12, 65);
        this._entityLabel.Text = "Skup:";

        this._entityCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        this._entityCombo.Location = new System.Drawing.Point(100, 62);
        this._entityCombo.Size = new System.Drawing.Size(248, 23);
        this._entityCombo.SelectedIndexChanged += new System.EventHandler(this.EntityCombo_SelectedIndexChanged);

        this._sourceGroup.Controls.Add(this._selectFileButton);
        this._sourceGroup.Controls.Add(this._selectedFileLabel);
        this._sourceGroup.Controls.Add(this._entityLabel);
        this._sourceGroup.Controls.Add(this._entityCombo);

        this._runButton.Location = new System.Drawing.Point(12, 410);
        this._runButton.Size = new System.Drawing.Size(360, 38);
        this._runButton.Text = "Run to staging";
        this._runButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        this._runButton.BackColor = AccentColor;
        this._runButton.ForeColor = System.Drawing.Color.White;
        this._runButton.FlatStyle = FlatStyle.Flat;
        this._runButton.FlatAppearance.BorderSize = 0;
        this._runButton.Click += new System.EventHandler(this.RunButton_Click);

        this._confirmButton.Location = new System.Drawing.Point(12, 458);
        this._confirmButton.Size = new System.Drawing.Size(174, 32);
        this._confirmButton.Text = "Confirm";
        this._confirmButton.Enabled = false;
        this._confirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);

        this._discardButton.Location = new System.Drawing.Point(198, 458);
        this._discardButton.Size = new System.Drawing.Size(174, 32);
        this._discardButton.Text = "Discard";
        this._discardButton.Enabled = false;
        this._discardButton.Click += new System.EventHandler(this.DiscardButton_Click);

        
        this._resetGroup.Text = "Reset baze za testiranje";
        this._resetGroup.Location = new System.Drawing.Point(12, 500);
        this._resetGroup.Size = new System.Drawing.Size(360, 158);

        this._resetTargetSystemLabel.AutoSize = true;
        this._resetTargetSystemLabel.Location = new System.Drawing.Point(12, 28);
        this._resetTargetSystemLabel.Text = "Target system:";

        this._resetTargetSystemCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        this._resetTargetSystemCombo.Location = new System.Drawing.Point(12, 48);
        this._resetTargetSystemCombo.Size = new System.Drawing.Size(332, 23);
        this._resetTargetSystemCombo.SelectedIndexChanged += new System.EventHandler(this.ResetTargetSystemCombo_SelectedIndexChanged);

        this._resetTargetTableLabel.AutoSize = true;
        this._resetTargetTableLabel.Location = new System.Drawing.Point(12, 76);
        this._resetTargetTableLabel.Text = "Target table:";

        this._resetTargetTableCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        this._resetTargetTableCombo.Location = new System.Drawing.Point(100, 73);
        this._resetTargetTableCombo.Size = new System.Drawing.Size(244, 23);

        this._resetButton.Location = new System.Drawing.Point(12, 106);
        this._resetButton.Size = new System.Drawing.Size(332, 32);
        this._resetButton.Text = "Reset na testne vrijednosti";
        this._resetButton.BackColor = System.Drawing.Color.FromArgb(217, 119, 6);
        this._resetButton.ForeColor = System.Drawing.Color.White;
        this._resetButton.FlatStyle = FlatStyle.Flat;
        this._resetButton.FlatAppearance.BorderSize = 0;
        this._resetButton.Click += new System.EventHandler(this.ResetButton_Click);

        this._resetGroup.Controls.Add(this._resetTargetSystemLabel);
        this._resetGroup.Controls.Add(this._resetTargetSystemCombo);
        this._resetGroup.Controls.Add(this._resetTargetTableLabel);
        this._resetGroup.Controls.Add(this._resetTargetTableCombo);
        this._resetGroup.Controls.Add(this._resetButton);

        this._sourceFieldColumn.HeaderText = "Source field";
        this._sourceFieldColumn.Name = "SourceField";
        this._sourceFieldColumn.ReadOnly = true;
        this._sourceFieldColumn.Width = 200;

        this._targetColumnColumn.HeaderText = "Target column";
        this._targetColumnColumn.Name = "TargetColumn";
        this._targetColumnColumn.Width = 200;

        this._mappingGrid.AllowUserToAddRows = false;
        this._mappingGrid.AllowUserToDeleteRows = false;
        this._mappingGrid.AutoGenerateColumns = false;
        this._mappingGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._sourceFieldColumn,
            this._targetColumnColumn});
        this._mappingGrid.Location = new System.Drawing.Point(10, 22);
        this._mappingGrid.Size = new System.Drawing.Size(778, 158);
        this._mappingGrid.RowHeadersVisible = false;
        this._mappingGrid.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));

        this._mappingGroup.Text = "Mapiranje polja";
        this._mappingGroup.Location = new System.Drawing.Point(386, 62);
        this._mappingGroup.Size = new System.Drawing.Size(798, 190);
        this._mappingGroup.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right))));
        this._mappingGroup.Controls.Add(this._mappingGrid);

        this._stagedRecordsGrid.AllowUserToAddRows = false;
        this._stagedRecordsGrid.AllowUserToDeleteRows = false;
        this._stagedRecordsGrid.ReadOnly = true;
        this._stagedRecordsGrid.Location = new System.Drawing.Point(10, 22);
        this._stagedRecordsGrid.Size = new System.Drawing.Size(778, 258);
        this._stagedRecordsGrid.RowHeadersVisible = false;
        this._stagedRecordsGrid.AllowUserToResizeRows = false;
        this._stagedRecordsGrid.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));

        this._stagedGroup.Text = "Uvezeni podaci";
        this._stagedGroup.Location = new System.Drawing.Point(386, 260);
        this._stagedGroup.Size = new System.Drawing.Size(798, 290);
        this._stagedGroup.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this._stagedGroup.Controls.Add(this._stagedRecordsGrid);

        this._resultTextBox.Location = new System.Drawing.Point(10, 22);
        this._resultTextBox.Size = new System.Drawing.Size(778, 108);
        this._resultTextBox.Multiline = true;
        this._resultTextBox.ReadOnly = true;
        this._resultTextBox.ScrollBars = ScrollBars.Vertical;
        this._resultTextBox.Anchor = ((AnchorStyles)((((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right))));

        this._logGroup.Text = "Rezultati";
        this._logGroup.Location = new System.Drawing.Point(386, 560);
        this._logGroup.Size = new System.Drawing.Size(798, 140);
        this._logGroup.Anchor = ((AnchorStyles)((((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right))));
        this._logGroup.Controls.Add(this._resultTextBox);

        this._statusLabel.AutoSize = false;
        this._statusLabel.Location = new System.Drawing.Point(12, 710);
        this._statusLabel.Size = new System.Drawing.Size(1172, 26);
        this._statusLabel.ForeColor = System.Drawing.Color.DimGray;
        this._statusLabel.Anchor = ((AnchorStyles)((((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right))));

        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = PageBackColor;
        this.ClientSize = new System.Drawing.Size(1200, 748);
        this.MinimumSize = new System.Drawing.Size(1000, 600);
        this.Text = "Import Component";
        this.Controls.Add(this._titleBar);
        this.Controls.Add(this._connectionGroup);
        this.Controls.Add(this._targetGroup);
        this.Controls.Add(this._sourceGroup);
        this.Controls.Add(this._runButton);
        this.Controls.Add(this._confirmButton);
        this.Controls.Add(this._discardButton);
        this.Controls.Add(this._resetGroup);
        this.Controls.Add(this._mappingGroup);
        this.Controls.Add(this._stagedGroup);
        this.Controls.Add(this._logGroup);
        this.Controls.Add(this._statusLabel);

        ((System.ComponentModel.ISupportInitialize)(this._mappingGrid)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this._stagedRecordsGrid)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion
}
