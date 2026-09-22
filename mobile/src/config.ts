// Set via build config for release. Must be HTTPS in production.
export const API_BASE_URL = __DEV__ ? 'http://10.0.2.2:5000/api/v1' : 'https://api.example.com/api/v1';
