﻿- `Add-Migration [NAME] -OutputDir .\UserAccounts\Infrastructure\EntityFramework\Migrations -Project Sample`
- `Script-Migration -StartupProject Sample.App.Web -Project Sample`
- `Update-Database -StartupProject Sample.App.Web -Project Sample`