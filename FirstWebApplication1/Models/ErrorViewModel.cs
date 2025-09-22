namespace FirstWebApplication1.Models //namespace for å organisere koden, alle models i prosjektet samles her
{
    public class ErrorViewModel //Klasse som representerer feilmelding, hvis noe går galt får brukeren feilmelding
    {
        public string? RequestId { get; set; } //Gir en ID som viser hvilken forespørsel feilen skjedde på,? betyr at verdien kan være null

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId); // sjekker om Requestid finnes, bool gir true or false, hvis Requestid har verdi (true), kan vises, hvis (false), kan ikke vises
    }
}
