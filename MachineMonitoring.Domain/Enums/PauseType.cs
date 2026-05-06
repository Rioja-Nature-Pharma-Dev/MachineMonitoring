namespace MachineMonitoring.Domain.Enums;

public enum PauseType
{
    MachineIncidentCounter = 0,
    MachineIncidentWeigher = 1,
    MachineIncidentLabeler = 2,
    MachineIncidentRepercap = 3,
    MachineIncidentCapper = 4,
    MachineIncidentPositioner = 5,
    MachineIncidentFiller = 6,
    MachineIncidentOther = 7,
    AutomaticWeigherFault = 8,
    AutomaticLabelFault = 9,
    MissingMaterial = 10,
    DefectiveMaterial = 11,
    MaintenanceInProgress = 12,
    CleaningInProgress = 13,
    QualityStop = 14,
    ShiftChange = 15,
    PartialProduction = 16,
    Stop = 17
}