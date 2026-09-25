// ASP.NET Core's ClaimTypes.* constants are actually long URI strings, and
// JwtService writes tokens using those Claim objects directly — so the JWT
// payload has these full URIs as its keys, not short names like "sub" or "role".
// Named here once so the rest of the app never has to hardcode them.
export const CLAIM_KEYS = {
  id: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
  email: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
  role: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
  departmentId: "DepartmentId",
  firstName: "FirstName",
  lastName: "LastName",
}
