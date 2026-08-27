namespace Jacana.Identity.Domain;

public enum SystemRole
{
    Administrator,
    Doctor,
    Nurse,
    Receptionist,
    LabTechnician,
    Pharmacist,
    StoreKeeper,
    Accountant,
    Cashier,
    RecordsOfficer,
    ITSupport
}

public enum UserStatus
{
    Active,
    Suspended,
    Locked,
    Deactivated
}
