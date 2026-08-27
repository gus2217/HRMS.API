namespace Jacana.Pharmacy.Domain;

public enum PrescriptionStatus { Pending, PartiallyDispensed, FullyDispensed, Cancelled }

public enum PrescriptionItemStatus { Pending, PartiallyDispensed, Dispensed, OutOfStock, Cancelled }
