namespace Nettside.Models.ViewModel
{
    public class AreaChangesViewModel
    {
        public int Id { get; set; }
        public string? ViewAreaJson { get; set; }
        public string? ViewDescription { get; set; }
        public string? ViewKommunenavn { get; set; }
        public string? ViewFylkenavn { get; set; }
        public string? Email { get; set; }

        public DateTime? ViewDate { get; set; }
        public string? CaseHandler { get; set; } 
        public string? Submitter { get; set; }

        public string? Status { get; set; }

    }
}
