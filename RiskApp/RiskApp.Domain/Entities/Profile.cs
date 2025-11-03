using RiskApp.Domain.Abstraction;
namespace RiskApp.Domain.Entities;
public class Profile : BaseEntity
{
    public string FullName { get; private set; } = default!;
    public DateOnly DateOfBirth { get; private set; }
    public string NationalId { get; private set; } = default!; // PAN/Aadhaar/etc.
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }

    // Navigation
    public ICollection<EmploymentRecord> EmploymentHistory { get; private set; } = new List<EmploymentRecord>();
    public ICollection<Earning> Earnings { get; private set; } = new List<Earning>();
    private Profile() { } // EF
    public Profile(string fullName, DateOnly dob, string nationalId, string? email = null, string? phone = null, string? address = null)
    {
        FullName = fullName.Trim();
        DateOfBirth = dob;
        NationalId = nationalId.Trim();
        Email = email;
        Phone = phone;
        Address = address;
    }
    public void UpdateContact(string? email, string? phone, string? address)
    {
        Email = email;
        Phone = phone;
        Address = address;
        Touch();
    }
}
