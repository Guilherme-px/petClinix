namespace PetClinix.Modules.Appointments.Application.Contracts;

public interface IClinicScheduleService
{
    (TimeOnly Start, TimeOnly End) GetWorkingHours();
}