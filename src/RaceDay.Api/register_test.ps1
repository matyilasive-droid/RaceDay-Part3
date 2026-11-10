$body = @{
    firstName = 'Test'
    lastName  = 'Runner'
    email     = 'runner_script@example.local'
    password  = 'Secret123!'
    role      = 'Runner'
} | ConvertTo-Json

try {
    $resp = Invoke-RestMethod -Uri http://localhost:5291/Users -Method Post -Body $body -ContentType 'application/json' -ErrorAction Stop
    $resp | ConvertTo-Json -Depth 5
} catch {
    Write-Error $_.Exception.ToString()
}
