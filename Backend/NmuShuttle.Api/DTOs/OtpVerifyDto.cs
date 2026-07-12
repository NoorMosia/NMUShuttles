namespace NmuShuttle.Api.DTOs;

/// <summary>
/// Request body for student OTP verification.
/// </summary>
public record StudentOtpVerifyDto(string StudentNumber, string Otp);

/// <summary>
/// Request body for marshal OTP verification.
/// </summary>
public record MarshalOtpVerifyDto(string StaffNumber, string Otp);
