# Connecting a Real Phone to the Backend

The app currently uses `http://10.0.2.2:5000` which is the Android emulator's alias for your PC's localhost. A real phone can't reach that address.

## Option 1: Same Wi-Fi Network (Easiest)

1. **Find your PC's local IP**: open PowerShell and run `ipconfig` — look for your Wi-Fi adapter's IPv4 (e.g., `192.168.1.105`)

2. **Start the backend listening on all interfaces**:
   ```powershell
   & "C:\Users\s229895735\Documents\dotnet-sdk-9.0.315-win-x64\dotnet.exe" run --project Backend\NmuShuttle.Api --urls "http://0.0.0.0:5000"
   ```

3. **Update the app's base URL** in `AppUI/lib/core/api/dio_client.dart`:
   ```dart
   baseUrl: baseUrl ?? 'http://192.168.1.105:5000/api/v1',  // your PC's IP
   ```

4. **Ensure your phone and PC are on the same Wi-Fi network**

5. **Allow port 5000 through Windows Firewall** if blocked:
   ```powershell
   New-NetFirewallRule -DisplayName "NMU Shuttle API" -Direction Inbound -Port 5000 -Protocol TCP -Action Allow
   ```

## Option 2: ngrok (Works From Any Network)

1. Install ngrok: https://ngrok.com/download
2. Run: `ngrok http 5000`
3. It gives you a public URL like `https://abc123.ngrok.io`
4. Set that as the base URL in `dio_client.dart`:
   ```dart
   baseUrl: baseUrl ?? 'https://abc123.ngrok.io/api/v1',
   ```

No firewall config needed, works from anywhere — but slower since traffic routes through ngrok's servers.

## Notes

- The emulator uses `10.0.2.2` to reach the host machine's localhost. A real phone needs the actual network IP.
- If you're on university/corporate Wi-Fi that blocks device-to-device traffic, use ngrok instead.
- To avoid editing code each time, you can make the base URL configurable via a build flag or environment-specific config file.
