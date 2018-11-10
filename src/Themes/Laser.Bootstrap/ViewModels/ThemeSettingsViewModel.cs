using System.Collections.Generic;
namespace Laser.Bootstrap.ViewModels
{
    public class ThemeSettingsViewModel
    {
        public string Swatch { get; set; }
        public bool UseFixedNav { get; set; }
        public bool UseNavSearch { get; set; }
        public bool UseFluidLayout { get; set; }
        public bool UseInverseNav { get; set; }
        public bool UseStickyFooter { get; set; }
        public string TagLineText { get; set; }
        public IList<ThemeInfoViewModel> AdditionalThemes{ get; set; }
    }
}