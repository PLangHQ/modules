# Google.Drive Module

This module provides a simple interface for interacting with Google Drive, enabling you to retrieve a list of files from a folder and download files using their file ID.

## Prerequisites

Before using this module, you'll need to enable the Google Drive API and create a service account key. Follow these steps to set up your Google account:

### Google API Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/).
2. Create a new project.
3. Search for "Google Drive API" and select it.
4. Click "Enable" to activate the Google Drive API for your project.
5. Open the menu (top left), select "IAM & Admin."
6. Click "Service Accounts" under the IAM & Admin menu.
7. Click "Create Service Account."
8. Give the service account a name, then click "Done."
9. Select the newly created account, then navigate to the "Keys" tab.
10. Click "Add Key" and select "JSON" as the key type.
11. Download the JSON key file and save it on your computer.

## Module Setup

### Download Required Libraries

To use the `Google.Drive` module, you'll need to download and include several dependencies in your `.modules` folder:

- `Google.Drive.dll`
- `Google.Apis.dll`
- `Google.Apis.Core.dll`
- `Google.Apis.Drive.v3.dll`
- `Google.Apis.Auth.dll`

These libraries can be found on [NuGet.org](https://www.nuget.org/). Search for the packages, download them, unzip the package, and place the DLL files (latest version, currently .NET 6.0) in your `.modules` folder.

### Finding the Folder ID

To retrieve or download files from a specific folder in Google Drive, you need the folder's ID. You can find this ID by navigating to the folder in your browser. The ID is the last part of the URL. For example:

https://drive.google.com/drive/u/0/folders/tAff2u44jU2nZgpJkbY5mSwW3


In this case, the folder ID is `tAff2u44jU2nZgpJkbY5mSwW3`.

## Usage Example

Here's a basic example of how to use the `Google.Drive` module:

```plang
Start
- get list of all my files in "tAff2u44jU2nZgpJkbY5mSwW3" on google drive, %files%
- write out "List of all files: %files%"
- go through %files%, call ProcessFile

ProcessFile
- download file from google drive, fileId=%item.id%, save it to file/%item.name%
```

## Initial Run

When you run the module for the first time, you'll be prompted to provide your service account key. Paste the content of the JSON file you downloaded during the Google API setup.

## Contributing

Contributions to this module are welcome. Please feel free to submit pull requests or open issues on the repository to improve functionality or fix bugs.

