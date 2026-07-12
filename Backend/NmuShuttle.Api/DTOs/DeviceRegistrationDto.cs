namespace NmuShuttle.Api.DTOs;

/// <summary>
/// Request DTO for registering a device for push notifications.
/// </summary>
public class DeviceRegistrationDto
{
    /// <summary>
    /// Firebase Cloud Messaging token for the device.
    /// </summary>
    public string FcmToken { get; set; } = string.Empty;

    /// <summary>
    /// Device platform — must be "android" or "ios".
    /// </summary>
    public string Platform { get; set; } = string.Empty;
}
