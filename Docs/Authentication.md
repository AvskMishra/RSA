# Authentication Approaches in ASP.NET Core

## ASP.NET Core Identity + JWT (EF Core + Roles)

### What it is?
Use the built-in **ASP.NET Core Identity** system (users, roles, password hashing, lockouts, etc.) stored in your SQLite DB, and issue JWTs on sign-in.

#### Pros
- Full user/role model out of the box (`UserManager` / `RoleManager`).
- Secure password hashing, lockout, email confirmation hooks.
- Easiest to expand (password reset, 2FA later).

#### Cons
- Most complex to set up initially.
- May include features you don't need.
- Heavier DB schema.

### When to use?
You need a full-featured user system with roles and plan to expand features later.

## Lightweight Custom Auth + JWT (No Identity)

### What it is?
Create a simple `User` table, hash passwords yourself (e.g., BCrypt / Argon2), and issue JWTs manually. Add roles as a string or collection claim.

#### Pros
- Minimal dependencies and tables.
- Very explicit control.

#### Cons
- You must implement security hygiene: hashing, lockouts, password policies, etc.
- More custom code to maintain.

### Best for - 
Small internal services, PoCs, or when Identity is “too heavy”.

## External IdP (Auth0 / Okta / Azure AD / Entra ID) → JWT Validation Only

### What it is?
Delegate login to an external Identity Provider. Your API only validates incoming JWTs and enforces roles/claims.

#### Pros
- No password handling or storage in your app.
- SSO, social logins, enterprise-ready.

#### Cons
- Requires IdP setup and often paid tiers at scale.
- Roles/claims management lives in the IdP.

### Best for -
Teams already using an IdP or needing SSO/enterprise authentication.

## Role Enforcement (Works With All Options)

### Attribute-Based
```csharp
[Authorize(Roles = "Writer")]
[Authorize(Roles = "Reader,Writer")]
```

### Policy-Based(Recommended)

Define policies like `CanWrite` that check `role == Writer`.

## Implementation Preview(ASP.NET Core Identity + JWT)

### Packages(API Project)

```
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.IdentityModel.Tokens
```

### DBContext

- Make `RiskAppDbContext` inherit `IdentityDbContext<AppUser, IdentityRole, string>`.
- Or keep a separate AuthDbContext(simpler to reuse an existing DB).
- Add migration(creates Identity tables in SQLite).

### AppUser

Minimal User entity -
```csharp
public class AppUser : IdentityUser
{
    // Optional: DisplayName, etc.
}
```

### JWT Configuration

- `appsettings.json` keys:
    - `Jwt:Issuer`
    - `Jwt:Audience`
    - `Jwt:Key`(symmetric)
- Wire `JwtBearer` authentication in `Program.cs`.

### Auth Endpoints

- `POST /api/auth/register` - _Creates user and assigns role_
- `POST /api/auth/token` - _Username/password → JWT with role claim_
- Optional - Refresh tokens later

### Role Seeding

- Seed Roles
    - `Reader`
    - `Writer`
- Seed demo users
    - `reader@demo.local` → Reader
    - `writer@demo.local` → Writer  