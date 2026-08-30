namespace Jacana.PatientRegistration.Domain;

public enum AllergySeverity { Mild, Moderate, Severe, LifeThreatening }

public enum ConsentType { TreatmentConsent, DataSharingConsent, ShaDataSharingConsent, ResearchConsent }

/// <summary>
/// How the patient intends to pay for care. SHA = national Social Health
/// Authority cover, Other = any other insurer/scheme, Private = self-pay.
/// </summary>
public enum InsuranceType { Sha, Other, Private }

/// <summary>
/// The clinic/department a patient is visiting, using terminology understood
/// by medics and nurses in Kenyan facilities (KEPH-aligned).
/// </summary>
public enum ClinicType
{
    GeneralOutpatient,
    Counselling,
    Laboratory,
    Immunization,
    Wellness,
    ReproductiveHealth,
    ChildWelfare,
    MaternalChildHealth,
    Antenatal,
    Postnatal,
    FamilyPlanning,
    ComprehensiveCareCentre,
    Tuberculosis,
    Nutrition,
    Dental,
    Eye,
    Ent,
    Physiotherapy,
    AdolescentYouthFriendly
}
