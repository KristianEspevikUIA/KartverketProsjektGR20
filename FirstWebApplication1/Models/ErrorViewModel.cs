namespace FirstWebApplication1.Models //namespace for � organisere koden, alle models i prosjektet samles her
{
    public class ErrorViewModel //Klasse som representerer feilmelding, hvis noe g�r galt f�r brukeren feilmelding
    {
        public string? RequestId { get; set; } //Gir en ID som viser hvilken foresp�rsel feilen skjedde p�,? betyr at verdien kan v�re null

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId); // sjekker om Requestid finnes, bool gir true or false, hvis Requestid har verdi (true), kan vises, hvis (false), kan ikke vises
    }
}
