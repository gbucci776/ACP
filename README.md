dotnet ef database update `
  --project .\ACP\ACP.csproj `
  --startup-project .\ACP\ACP.csproj
docker compose exec mariadb mariadb -u acp_app -p acp_dev

Email: client.demo@acp.local
Password: ClientDemo2026!

Email: consumer.demo@acp.local
Password: ConsumerDemo2026!

dotnet user-secrets set `
  "DemoAccounts:Client:Email" `
  "client.demo@acp.local" `
  --project .\ACP\ACP.csproj

dotnet user-secrets set `
  "DemoAccounts:Client:Password" `
  "ClientDemo2026!" `
  --project .\ACP\ACP.csproj

dotnet user-secrets set `
  "DemoAccounts:Consumer:Email" `
  "consumer.demo@acp.local" `
  --project .\ACP\ACP.csproj

dotnet user-secrets set `
  "DemoAccounts:Consumer:Password" `
  "ConsumerDemo2026!" `
  --project .\ACP\ACP.csproj
