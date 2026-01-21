param(
    [Parameter(Mandatory = $true)]
    [string]$StorageAccount,

    [Parameter(Mandatory = $true)]
    [string]$Container,

    [Parameter(Mandatory = $true)]
    [string]$FolderPath,

    [Parameter(Mandatory = $true)]
    [string]$JsonFile,

    [Parameter(Mandatory = $true)]
    [string]$SASToken
)

# -----------------------------------------
# Load JSON metadata + tags
# -----------------------------------------
if (-not (Test-Path $JsonFile)) {
    Write-Error "JSON file not found: $JsonFile"
    exit 1
}

$json = Get-Content $JsonFile | ConvertFrom-Json

# Convert metadata to array of "key=value" strings
$metadataArray = @($json.metadata.PSObject.Properties |
    ForEach-Object { "$($_.Name)=$($_.Value)" })

# Convert tags to array of "key=value" strings
$tagsArray = @($json.tags.PSObject.Properties |
    ForEach-Object { "$($_.Name)=$($_.Value)" })

Write-Host "Using metadata: $($metadataArray -join ' ')"
Write-Host "Using tags:     $($tagsArray -join ' ')"

# -----------------------------------------
# Upload loop
# -----------------------------------------
if (-not (Test-Path $FolderPath)) {
    Write-Error "Folder not found: $FolderPath"
    exit 1
}

$files = Get-ChildItem -Path $FolderPath -File -Filter *.txt

foreach ($file in $files) {
    Write-Host "----------------------------------------"
    Write-Host "Uploading: $($file.Name)"

    # Upload blob
    az storage blob upload `
        --account-name $StorageAccount `
        --container-name $Container `
        --name $file.Name `
        --file $file.FullName `
        --overwrite true `
        --auth-mode key

    # Apply metadata (pass as array)
    $metadataArgs = @(
        "storage", "blob", "metadata", "update",
        "--account-name", $StorageAccount,
        "--container-name", $Container,
        "--name", $file.Name,
        "--metadata"
    ) + $metadataArray + @("--auth-mode", "key")
    
    & az @metadataArgs

    # Apply index tags (pass as array)
    $tagsArgs = @(
        "storage", "blob", "tag", "set",
        "--account-name", $StorageAccount,
        "--container-name", $Container,
        "--name", $file.Name,
        "--tags"
    ) + $tagsArray + @("--auth-mode", "key")
    
    & az @tagsArgs

    Write-Host "Done: $($file.Name)"
}

Write-Host "----------------------------------------"
Write-Host "All files processed."
