using Nettside.Models;

namespace Nettside.ViewModels
{
    /// <summary>
    /// represents the datamodel for mapping reports, including geochanges and areachanges.
    /// This ViewModel is used to pass data between the controller and the view in the application.
    /// </summary>
    public class MapReportViewModel
    {
        public IEnumerable<AreaChangeModel?> AreaChanges { get; set; }

      //  public GeoChangesModel? GeoChangesModel { get; set; }

        public AreaChangeModel? AreaChangeModel { get; set; }
        // public IEnumerable<GeoChangesModel?> GeoChanges { get; set; }

    }
}
