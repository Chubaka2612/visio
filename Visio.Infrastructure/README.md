# 🚀 Visio Deployment

This project includes infrastructure deployment via Terraform and zip deployment for both the Web App and Function App using PowerShell.

## Deployment Steps

1. **Deploy Infrastructure**

```powershell
cd Visio\Visio.Infrastructure
.\deploy.ps1 -Environment qa  
```

> Replace `qa` with `dev` or `prod` as needed.  

2. **Deploy Web App**

```powershell
cd Visio\Visio.Infrastructure\scripts

.\deploy-web.ps1 -ProjectPath "..\..\Visio.Web\Visio.Web.csproj" -PublishFolderName "publish-qa" -ZipFileName "publish-qa.zip" -ResourceGroup "rg-visio-qa-infra" -AppServiceName "visio-qa-web"
```

3. **Deploy Function App**

```powershell
cd Visio\Visio.Infrastructure\scripts

.\deploy-web.ps1 -ProjectPath "..\..\Visio.Recognition\Visio.Recognition.csproj" -PublishFolderName "publish-qa-func" -ZipFileName "publish-qa-func.zip" -ResourceGroup "rg-visio-qa-infra" -AppServiceName "visio-qa-function"
```

> Both the zip and publish folders will be created in the root `Visio` directory.

4. **Destroy Infrastructure (Optional)**

```powershell
cd Visio\Visio.Infrastructure
.\deploy.ps1 -Environment qa -Destroy   
```