# HealthMonitoringApp
ASP.Net Core REST API for calculating NewsScore based on input measurements.

## Measurement Types are configured in appsetting.json
- This can be moved from config to database and its values are managed via Admin console.

### Assumptions
 - More measurement typs can be added in future
 - Validation to check invalid Config (overlapping rages, gaps in ranges, duplicate types etc) is skip.

