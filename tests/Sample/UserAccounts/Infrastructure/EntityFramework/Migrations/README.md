﻿- `Add-Migration [NAME] -OutputDir .\UserAccounts\Infrastructure\EntityFramework\Migrations -Project Sample`
- `Script-Migration -StartupProject Sample.Application.Web -Project Sample`
- `Update-Database -StartupProject Sample.Application.Web -Project Sample`
