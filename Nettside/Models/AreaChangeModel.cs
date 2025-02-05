using System.ComponentModel.DataAnnotations;

namespace Nettside.Models
{
    public class AreaChangeModel
    {
        
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string? AreaJson { get; set; }
        public string? Description { get; set; }
        public string? Kommunenavn { get; set; }
        public string? Fylkenavn { get; set; }

    }
}
