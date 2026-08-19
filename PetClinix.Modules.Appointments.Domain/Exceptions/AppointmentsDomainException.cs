namespace PetClinix.Modules.Appointments.Domain.Exceptions;

public sealed class AppointmentsDomainException : Exception
{
    public string Code { get; }
    public AppointmentsDomainException(string code, string message) : base(message) { Code = code; }
}