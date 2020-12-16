# SpreadsheetWeb Custom Actions Example - Authorize.Net integration

SpreadsheetWeb is a platform for building and generating web applications and complex calculation engines from Excel models. The software is produced and distributed by Pagos Inc.

This repository contains sample code that aims to demonstrate how to inject custom code that can communicate with the [Authorize.Net](https://www.authorize.net/) API. The provided sample code submits a record to the Authorize.Net data structure, but the same basic principles can be applied to any functions permitted by the target service.

> The sample can be used only with SpreadsheetWeb Server Packages - must have an active [server license](https://www.spreadsheetweb.com/server-pricing/).

> For versions before 6.15

> You will also need to have an active Authorize.Net account in order to request access to their web service. 

## Download

If you have `git` command line installed on your computer, you can download the sample by running:

```bash
> git clone https://github.com/spreadsheetweb/samples-custom-actions-authorizenet
```

Alternatively, you can click the `Clone or Download` button on the GitHub page.

## How do Custom Actions work in the SpreadsheetWeb platform?

- Custom actions must be written as a .NET Framework Library. In our sample, we have generated a solution in C#, which can be viewed and modified with Microsoft Visual Studio or other compatible software.
- When creating a new library project you must add references to several prerequisite .NET libraries that will be provided to you. Alternatively, you can utilize the ones that are included in the sample or contact our support for more information on versions that are compatible with your server version.
- The copy included in the sample can be found under the _Pagos References_ folder. These are also added as references to the Visual Studio project.

    ```bash
    > Pagos.Designer.Interfaces.External.dll
    > Pagos.SpreadsheetWeb.SpreadsheetWebAPI.dll
    > Pagos.SpreadsheetWeb.Web.Api.Objects.dll
    ```

- In the imports section of your class file, make sure to include the following namespaces from the aforementioned libraries:

    ```C#
    using Pagos.Designer.Interfaces.External.CustomHooks;
    using Pagos.Designer.Interfaces.External.Messaging;
    using Pagos.SpreadsheetWeb.Web.Api.Objects.Calculation;
    ```

- The library needs to implement one or more interfaces exposed by the `Pagos.Designer.Interfaces.External.CustomHooks` namespace. More details regarding these interfaces can be found at the following help page: [Custom Actions in Designer](https://pagosinc.atlassian.net/wiki/spaces/SSWEB/pages/501186561/Custom+Actions+in+Designer).

    - `IBeforeCalculation`
    - `IAfterCalculation`
    - `IBeforeSave`
    - `IAfterSave`
    - `IBeforePrint`
    - `IAfterPrint`
    - `IBeforeEmail`
    - `IAfterEmail`

- Each of those interfaces expose specific methods, which you will need to implement. These can be seen in the sample.

## How to run the sample

1. Download the sample. This is a Visual Studio solution that includes a single C# class library.
2. Open the solution file in Visual Studio. Before proceeding, you should set your Authorize.Net account credentials.

    ```C#
    namespace AuthorizeNetExample
    {
        public class Class1 : IAfterCalculation
        {
    
            private string apiLoginId = "your_api_login_id";
            private string transactionKey = "your_current_transaction_key";
            
            ...
        } 
        ...
    }   
    ```

3. Resolve NuGet Packages. This solution utilizes extra dependencies which can be retrieved by using NuGet Package Manager. Simply right-click on _References_ in the _Solution Explorer_ in Visual Studio, then select _NuGet Packages_. In the Nuget Manager window, click on the **Restore** button, as shown in the following screenshot:

    ![NuGetPackages](./Images/NuGetPackages.png)
    
4. Compile the solution. The output of the compilation will be the **AuthorizeNetExample.dll** library, which can subsequently be uploaded as a custom action.
5. Create a Designer application on your SpreadsheetWeb server. For this, you will need an Excel file, which will act as the primary calculation. This can be found under the _Sample_ directory. You can also review [this link](https://pagosinc.atlassian.net/wiki/spaces/SSWEB/pages/35954/Custom+Applications) for more detailed on creating an application. **Important Note:** Remember to select  _Designer_ as the application type.
6. Once the application is created, navigate to _Custom Actions_, create a **New** custom action and submit the previously compiled **AuthorizeNetExample.dll** (or **AuthorizeNetExampleMerged.dll** - see [Special considerations](#special-considerations-for-referenced-libraries) section below) assembly file.
7. You will also need to create a user interface for the application. Navigate to the **User Interface Designer** and add controls to the default home page. Associate these controls with the named ranges from the Excel file. Alternatively you may use the **Generate** button, which will attempt to auto-generate a basic user interface based on the Excel calculation model's named range metadata. A video tutorial can be found at the following link: [User Interface in SpreadsheetWEB Designer](https://www.spreadsheetweb.com/project/user-interface-designer/). An example user interface can be seen below.

    ![UI-and-button-with-hook-attached.png](./Images/UI-and-button-with-hook-attached.png)
    
    > **Important Note:** In order for the custom action code to be executed, it will need to be attached to a button, as shown in the screenshot above. 
    
8. Preview the page or Publish the application.
9. Open the application and enter values in the corresponding textboxes.
10. Click the **Submit** button.

## What is the sample about?

Upon clicking the **Submit** button, the application should connect to the Authorize.Net web service and generate a new [Payment Transaction](https://developer.authorize.net/api/reference/#payment-transactions) entry, which you can subsequently browse from the Authorize.Net Dashboard.

![AuthorizeNetDashboard](./Images/Dashboard.PNG)

## Special considerations for referenced libraries

For the **AuthorizeNetExample.dll** to be properly loaded into the application domain at runtime, you will also need to provide any extra libraries referenced by the project (including those downloaded via NuGet Package Manager). 

There are two options you can choose from:

1. Since you have access to your own SpreadsheetWeb server, you can copy the relevant assembly files (.dll) into the root bin directory for the web application. By default, this is located at:

    ```bash
    > C:\inetpub\wwwroot\Designer\bin
    ```
    
    For this sample, simply copy the following file:
    
    ```bash
    > AuthorizeNet.dll
    ```
    
    This file can be found in the packages directory. For details on the specific location, simply right-click on the dependency under the _Solution Explorer > References_ dropdown and click _Properties_. The _Properties_ panel will show the location where the assembly is hosted.
    
2. Another option for distributing the third-party assemblies is to merge them into a single assembly. Subsequently, you would simply upload the new merged assembly to the server via the _Custom Actions_ module of the Designer platform.

    The merge operation can be performed using [ILMerge](https://www.nuget.org/packages/ilmerge). To perform the merge, you will need to pull down the _ILMerge_ nuget package and then execute a command triggered by a post-build event. The sample solution already has this event defined. It can be reviewed in the project's **Properties** window (the command below references v2.14.1208 of _ILMerge_ - if your version differs, then substitute the package file path as required):

    > "$(SolutionDir)\packages\ILMerge.2.14.1208\tools\ILMerge.exe" /out:"$(TargetDir)\AuthorizeNetExampleMerged.dll" "$(TargetDir)\AuthorizeNetExample.dll" "$(TargetDir)\AuthorizeNet.dll"
    
    As output from this statement, the _ILMerge_ component will generate a separate assembly (**AuthorizeNetExampleMerged.dll**) after the sample's build. Rather than utilizing the **AuthorizeNetExample.dll** for the instructions above, you can utilize this _Merged_ version of the assembly.
    
    
