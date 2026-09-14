using System.Windows.Controls;

namespace ChemicalSpill.View.Controls
{
    /// <summary>
    /// Блок стадии мнемосхемы. Данные берутся из StageViewModel через DataContext,
    /// поэтому элемент не содержит логики и подходит для повторного использования.
    /// </summary>
    public partial class StageBlock : UserControl
    {
        public StageBlock()
        {
            InitializeComponent();
        }
    }
}
