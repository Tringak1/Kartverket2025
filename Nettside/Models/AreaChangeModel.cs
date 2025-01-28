using System.ComponentModel.DataAnnotations;

namespace Nettside.Models
{
    public class AreaChangeModel
    {
        [Key]
        public string Id { get; set; }
        // public string GeoJson { get; set; }
        public string Description { get; set; }

    }
}
