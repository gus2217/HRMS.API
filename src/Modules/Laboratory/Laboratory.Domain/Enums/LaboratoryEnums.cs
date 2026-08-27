namespace Jacana.Laboratory.Domain;

public enum LabOrderStatus { Pending, InProgress, PartiallyCompleted, Completed, Cancelled }

public enum LabTestStatus { Ordered, SampleCollected, InProgress, Resulted, Rejected }
