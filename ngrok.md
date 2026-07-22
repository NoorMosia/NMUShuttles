# Using ngrok with NMU Shuttle

ngrok exposes your local backend to the internet so Flutter Web and physical Android devices can reach it without deploying.

## Setup Order

1. **Start the .NET backend**
   ```bash
   cd Backend
   dotnet run --project NmuShuttle.Api
   ```
   Note the port it listens on (e.g. `http://localhost:5000`).

2. **Start ngrok**
   ```bash
   ngrok http 5000
   ```
   Copy the `Forwarding` HTTPS URL (e.g. `https://abc123.ngrok-free.dev`).

3. **Update the base URL in your Flutter client(s)**
   - **Web Portal:** `WebPortalUI/lib/core/api/dio_client.dart`
   - **Mobile App:** `AppUI/lib/core/api/dio_client.dart`

   Set the `baseUrl` to your ngrok URL + `/api/v1`:
   ```dart
   baseUrl: 'https://abc123.ngrok-free.dev/api/v1',
   ```

4. **Run the Flutter app**
   ```bash
   flutter run -d chrome        # Web portal
   flutter run -d <device_id>   # Mobile app on physical device
   ```

## The Browser Warning Problem

ngrok's free tier injects an interstitial HTML page ("Visit Site" button) on the first request from each client. For API calls this means your app receives HTML instead of JSON, causing data to never load.

**Fix:** Include this header in all HTTP requests:

```
ngrok-skip-browser-warning: true
```

This is already configured in both `DioClient` files via the default headers:

```dart
headers: {
  'Content-Type': 'application/json',
  'Accept': 'application/json',
  'ngrok-skip-browser-warning': 'true',
},
```

## CORS Configuration

When Flutter Web runs on `localhost`, the browser enforces CORS. Your .NET backend must allow the localhost origin.

In `Program.cs` (or wherever CORS is configured), ensure:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()   // Or specify localhost + ngrok origins
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

For production, replace `AllowAnyOrigin()` with specific allowed origins.

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Data doesn't load, no errors in console | ngrok interstitial returned HTML | Ensure `ngrok-skip-browser-warning` header is set |
| CORS errors in browser console | Backend not allowing the request origin | Update CORS policy to allow `localhost` or `*` in dev |
| Connection refused | Backend not running or wrong port in ngrok command | Verify the port matches `dotnet run` output |
| ngrok URL changed | Free tier generates a new URL each session | Update `baseUrl` in Dio client after each restart |
| 401 Unauthorized | JWT token expired or not sent | Check that the auth interceptor is attaching the token |

## Tips

- **Paid ngrok plan** gives you a stable subdomain so you don't have to update the URL every session.
- **Don't commit** the ngrok URL — it's temporary. Keep the default `baseUrl` as `http://10.0.2.2:5000/api/v1` (emulator) or `http://localhost:5000/api/v1` (web) and only override when testing with ngrok.
- The `ngrok-skip-browser-warning` header is harmless when not using ngrok — it's ignored by normal servers.
