---
title: Deploy an ASP.NET Core web app to Azure using Visual Studio | Microsoft Docs
author: rick-anderson
description: 
keywords: ASP.NET Core,
ms.author: riande
manager: wpickett
ms.date: 10/14/2016
ms.topic: article
ms.assetid: 78571e4a-a143-452d-9cf2-0860f85972e6
ms.technology: aspnet
ms.prod: aspnet-core
uid: tutorials/publish-to-azure-webapp-using-vs
---
# Deploy an ASP.NET Core web app to Azure using Visual Studio

By [Rick Anderson](https://twitter.com/RickAndMSFT), [Cesar Blum Silveira](https://github.com/cesarbs), and [Tom Dykstra](https://github.com/tdykstra)

## Prerequisites

You'll need a Microsoft Azure account. You can [open a free Azure account](https://azure.microsoft.com/pricing/free-trial/) or [Activate Visual Studio subscriber benefits](https://azure.microsoft.com/pricing/member-offers/msdn-benefits-details/). 

## Set up the development environment

* Install [Visual Studio 2017](https://docs.microsoft.com/visualstudio/install/install-visual-studio) with the following workloads:
  * **Web development**
  * **Azure development** 
  * **.NET Core and Docker** 

  If you already installed Visual Studio but don't have the required workloads, see [Modify Visual Studio](https://docs.microsoft.com/visualstudio/install/modify-visual-studio).

## Create a web app

Open Visual Studio, and then click **File > New > Project**.

![File menu](publish-to-azure-webapp-using-vs/_static/alt_new_project.png)

In the **New Project** dialog:

* In the left pane, tap **Web**

* In the center pane, tap **ASP.NET Core Web Application (.NET Core)**

* Tap **OK**

![New Project dialog](publish-to-azure-webapp-using-vs/_static/new_prj.png)

In the **New ASP.NET Core Web Application (.NET Core)** dialog:

* Tap **Web Application**

* Verify **Authentication** is set to **Individual User Accounts**

* Tap **OK**

![New ASP.NET Core Web Application (.NET Core) dialog](publish-to-azure-webapp-using-vs/_static/noath.png)

## Test the app locally

* Press **Ctrl-F5** to run the app locally

* Tap the **About** and **Contact** links. Depending on the size of your device, you might need to tap the navigation icon to show the links

![Web application open in Microsoft Edge on localhost](publish-to-azure-webapp-using-vs/_static/show.png)

* Tap **Register** and register a new user. You can use a fictitious email address. When you submit, you'll get the following error:

![A database operation failed while processing the request. SQL exception: Cannot open database ... Applying existing migrations for ApplicationDBContext may resolve this issue.](publish-to-azure-webapp-using-vs/_static/mig.png)

You can fix the problem in two different ways:

* Tap **Apply Migrations** and, once the page updates, refresh the page; or

* Open a command prompt in the project's directory, and run the following command:

  ```
  dotnet ef database update
  ```

The app displays the email that you used to register the new user and a **Log off** link.

![Web application open in Microsoft Edge. The Register link is replaced by the text Hello abc@example.com!](publish-to-azure-webapp-using-vs/_static/hello.png)

## Deploy the app to Azure

Right-click the project in **Solution Explorer** and select **Publish...**.

![Contextual menu open with Publish link highlighted](publish-to-azure-webapp-using-vs/_static/pub.png)

In the **Publish** tab of the project properties page, tap **Create**.

In the Pick a publish target dialog:

* Verify that **Microsoft Azure App Service** is selected.

* Verify **Create** is selected.*

* Tap **OK**.


In the **Create App Service** dialog:

* Accept the default values for **Web App Name** and **Subscription**.

* Tap **New...** for the **Resource Group** and enter a name for the new resource group

* Tap **New...** for the **App Service Plan** and select a **Location** near you. You can keep the default generated name and set **Size** to **Free**.

* Tap **Explore additional Azure services** to create a new database

![New Resource Group dialog: Hosting panel](publish-to-azure-webapp-using-vs/_static/cas.png)

* Tap the green **+** icon to create a new SQL Database.

![New Resource Group dialog: Services panel](publish-to-azure-webapp-using-vs/_static/sql.png)

* Tap **New...** on the **Configure SQL Database** dialog to create a new database server.

![Configure SQL Database dialog](publish-to-azure-webapp-using-vs/_static/conf.png)

* Enter an administrator user name and password, and then tap **OK**. Don't forget the user name and password you create in this step. You can keep the default **Server Name**.

> [!NOTE]
> "admin" is not allowed as the administrator user name.

![Configure SQL Server dialog](publish-to-azure-webapp-using-vs/_static/conf_servername.png)

* Tap **OK** on the  **Configure SQL Database** dialog.

![Configure SQL Database dialog](publish-to-azure-webapp-using-vs/_static/conf_final.png)

* Tap **Create** on the **Create App Service** dialog.

![Create App Service dialog](publish-to-azure-webapp-using-vs/_static/create_as.png)

In the **Publish** tab of the project properties page, verify that the profile you just created is selected, and then tap **Settings**.

In the **Publish** dialog, click the **Settings** tab.

* Expand **Databases** and check **Use this connection string at runtime**

* Expand **Entity Framework Migrations** and check **Apply this migration on publish**

* Tap **Publish** and wait until Visual Studio finishes publishing your app

![Publish dialog: Settings panel](publish-to-azure-webapp-using-vs/_static/pubs.png)

Visual Studio will publish your app to Azure and launch the cloud app in your browser.

### Test your app in Azure

* Test the **About** and **Contact** links

* Register a new user

![Web application opened in Microsoft Edge on Azure App Service](publish-to-azure-webapp-using-vs/_static/final.png)

### Update the app

* Edit the `Views/Home/About.cshtml` Razor view file and change its contents. For example:

<!-- literal_block {"ids": [], "linenos": false, "xml:space": "preserve", "language": "html", "highlight_args": {"hl_lines": [7]}} -->

```html
@{
       ViewData["Title"] = "About";
   }
   <h2>@ViewData["Title"].</h2>
   <h3>@ViewData["Message"]</h3>

   <p>My updated about page.</p>
   ```

* Right-click on the project and tap **Publish...** again

![Contextual menu open with Publish link highlighted](publish-to-azure-webapp-using-vs/_static/pub.png)

* After the app is published, verify the changes you made are available on Azure

### Clean up

When you have finished testing the app, go to the [Azure portal](https://portal.azure.com/) and delete the app.

* Select **Resource groups**, then tap the resource group you created

![Azure Portal: Resource Groups in sidebar menu](publish-to-azure-webapp-using-vs/_static/portalrg.png)

* In the **Resource group** blade, tap **Delete**

![Azure Portal: Resource Groups blade](publish-to-azure-webapp-using-vs/_static/rgd.png)

* Enter the name of the resource group and tap **Delete**. Your app and all other resources created in this tutorial are now deleted from Azure

### Next steps

* [Getting started with ASP.NET Core MVC and Visual Studio](first-mvc-app/start-mvc.md)

* [Introduction to ASP.NET Core](../index.md)

* [Fundamentals](../fundamentals/index.md)
