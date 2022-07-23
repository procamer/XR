using System;
using System.Windows.Forms;

namespace XR
{
    public partial class PropertiesGrid : Form
    {
        public PropertiesGrid()
        {
            InitializeComponent();
        }

        private void PreferencesGrid_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = new GridProperties
            {
                Color = Settings.Properties.Default.GridColor,
                CellSize = Settings.Properties.Default.GridCellSize,
                HorizontalDivisions = Settings.Properties.Default.GridHorizontalDivisions,
                VerticalDivisions = Settings.Properties.Default.GridVerticalDivisions
            };
        }
    }
}
