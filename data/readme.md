# Uplaod files in the Storage Account

## Step 1: Login in Azure

Run the following commands to login in Azure creating a logged session:

``` bash
az login
```

## Step 2: Generate Storage SAS

Run the following commands to generate the SAS token for the storage you created during deployment phase:

``` bash
$expiry = (Get-Date).AddHours(1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mmZ")

$sastoken = (az storage container generate-sas --account-name <storageAccountName> --name documents --permissions w --expiry $expiry --only-show-errors) 
```

where:
- `<storageAccountName>` is the name of the storage deployed.

The variable `$sastoken` will be used in the next commands.

## Step 3: Upload invoice files

Use the following command to uplad the fake invoices contained in the `data\invoices` folder:

``` pws
.\upload.ps1 -StorageAccount <storageAccountName> -Container documents  -FolderPath "invoices\*" -JsonFile "invoices\blobinfo.json" -SASToken $sastoken
```

where:
- `<storageAccountName>` is the name of the storage deployed.

## Step 4: Upload report files

Use the following command to uplad the fake reports contained in the `data\reports` folder:

``` pws
.\upload.ps1 -StorageAccount <storageAccountName> -Container documents  -FolderPath "reports\*" -JsonFile "reports\blobinfo.json" -SASToken $sastoken
```

where:
- `<storageAccountName>` is the name of the storage deployed.

## Step 5: Upload draft files

Use the following command to uplad the fake drafts contained in the `data\drafts` folder:

``` pws
.\upload.ps1 -StorageAccount <storageAccountName> -Container documents  -FolderPath "drafts\*" -JsonFile "drafts\blobinfo.json" -SASToken $sastoken
```

where:
- `<storageAccountName>` is the name of the storage deployed.
