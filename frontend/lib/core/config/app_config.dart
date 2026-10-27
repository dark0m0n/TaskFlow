class AppConfig {
  const AppConfig._();

  // Override for iOS simulators, physical devices and deployed environments.
  static const _configuredBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://10.0.2.2:5079/api/',
  );

  static String get apiBaseUrl => _configuredBaseUrl.endsWith('/')
      ? _configuredBaseUrl
      : '$_configuredBaseUrl/';
}
