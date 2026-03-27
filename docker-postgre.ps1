# --- CONFIGURAÇÕES ---
$CONTAINER_NAME = "info-postgre"
$POSTGRES_USER = "root"
$POSTGRES_PASSWORD = "c0nnectadmin"
$PORT = 5432
$IMAGE = "postgres:alpine"
$POSTGRES_DB = "infoX_db"

$ROOT_PATH = Get-Location
$DB_PATH = Join-Path $ROOT_PATH "database"

# 1. Garante que a pasta está limpa (importante pelo erro anterior!)
if (Test-Path $DB_PATH) {
    Write-Host "Limpando pasta database antiga..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $DB_PATH
}
New-Item -ItemType Directory -Path $DB_PATH | Out-Null

Write-Host "--- Iniciando PostgreSQL Alpine ---" -ForegroundColor Cyan

# 2. Remove container antigo se existir
docker rm -f $CONTAINER_NAME 2>$null

# 3. Roda o comando em uma linha só para não ter erro de caractere de escape
docker run -d --name $CONTAINER_NAME -e POSTGRES_USER=$POSTGRES_USER -e POSTGRES_PASSWORD=$POSTGRES_PASSWORD -e POSTGRES_DB=$POSTGRES_DB -p "${PORT}:${PORT}" -v "${DB_PATH}:/var/lib/postgresql" --restart unless-stopped $IMAGE

Write-Host "Aguardando 5 segundos para o banco subir..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# 4. Tenta entrar no shell
docker exec -it $CONTAINER_NAME sh