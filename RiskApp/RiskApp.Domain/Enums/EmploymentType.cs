namespace RiskApp.Domain.Enums;
public enum EmploymentType
{
    FullTime = 1,
    PartTime = 2,
    Contract = 3,
    SelfEmployed = 4,
    Unemployed = 5
}
public enum RiskDecision
{
    Pending = 0,
    Approve = 1,
    Review = 2,
    Decline = 3
}