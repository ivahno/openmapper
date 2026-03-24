# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 1.x     | Yes       |
| < 1.0   | No        |

## Reporting a Vulnerability

If you discover a security vulnerability in OpenAutoMapper, please report it responsibly.

**Do NOT open a public issue.**

Instead, please use [GitHub's private vulnerability reporting](https://github.com/ivahno/openmapper/security/advisories/new).

### What to Include

- Description of the vulnerability
- Steps to reproduce
- Impact assessment
- Suggested fix (if any)

### Response Timeline

- **Acknowledgment:** Within 48 hours
- **Assessment:** Within 7 days
- **Fix/Disclosure:** Within 30 days for critical issues

## Security Considerations for Object Mappers

OpenAutoMapper takes the following security measures:

- **[SensitiveProperty] attribute** prevents accidental mapping of sensitive fields
- **Compile-time diagnostics** for unmapped destination properties (over-posting prevention)
- **No runtime reflection** eliminates an entire class of metadata-based attacks
- **Deterministic builds** with SourceLink for supply chain verification
- **Zero NuGet dependencies** in core packages
