namespace NmuShuttle.Api.DTOs;

/// <summary>
/// Request body for student OTP request.
/// </summary>
public record StudentOtpRequestDto(string StudentNumber);

/// <summary>
/// Request body for marshal OTP request.
/// </summary>
public record MarshalOtpRequestDto(string StaffNumber);
