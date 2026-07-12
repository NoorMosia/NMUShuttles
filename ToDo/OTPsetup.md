# OTP Email System

## Current State

Email sending is **NOT implemented yet**. The OTP is generated and stored in the database, but not actually emailed to the user. In development mode, the plain OTP is logged to the console instead.

## How the Flow Works

1. Student/Marshal hits `POST /api/v1/auth/student/otp/request` (or marshal equivalent) with their identifier
2. System looks up the user in the database
3. Generates a random 5-digit code (10000–99999)
4. Hashes it with SHA-256 and stores the hash + expiry (now + 5 minutes) in the user's `OtpHash`/`OtpExpiry` fields
5. **In development**: logs the plain OTP to the console (e.g., `OTP for s312847563: 48291`)
6. **In production**: nothing happens — the email integration hasn't been wired up yet
7. User submits the OTP via `POST /api/v1/auth/student/otp/verify`
8. System hashes the submitted code, compares against the stored hash, and checks expiry
9. If valid → issues a JWT token, clears OTP fields (single-use enforcement)
10. If invalid or expired → rejects with HTTP 401, still clears OTP fields

## Security Properties

- **Single-use**: OTP fields are cleared after ANY verification attempt (pass or fail). A code cannot be reused.
- **5-minute expiry**: OTPs expire exactly 5 minutes after generation.
- **Hashed storage**: Only the SHA-256 hash is stored in the database, never the plain OTP.
- **Rate limited**: OTP request endpoints are limited to 5 requests per minute per IP.

## What's Missing (Azure Communication Services Integration)

The spec calls for Azure Communication Services (Email) to deliver the OTP to the student's/marshal's university email address. This requires:

1. An `IEmailService` interface and implementation that calls the Azure Communication Services Email SDK
2. NuGet package: `Azure.Communication.Email`
3. Configuration keys (already defined in `.env` and steering docs):
   - `AZURE_COMM_CONNECTION_STRING` — Azure Communication Services connection string
   - `AZURE_COMM_SENDER_ADDRESS` — Verified sender email (e.g., `DoNotReply@your-domain.azurecomm.net`)

The `OtpService` currently has a placeholder where the email call should go (the `_logger.LogInformation` line). Once the email service is implemented, it would be injected into `OtpService` and called after storing the hash.

## Local Testing (Without Email)

For local development and testing, you can read the OTP directly from the console logs. When the app runs in Development mode, it prints:

```
info: NmuShuttle.Api.Services.OtpService[0]
      OTP for s312847563: 48291
```

Use that code to verify via the `/otp/verify` endpoint.
