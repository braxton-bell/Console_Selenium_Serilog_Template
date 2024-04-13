# Workaround for Including `selenium-manager` in Visual Studio Project Template Export

## Overview

This folder contains the `selenium-manager` directory, which is part of the Selenium package. The Selenium Manager is responsible for automatically downloading the correct WebDriver required for the project.

## Issue

When exporting the project template using Visual Studio's Export Template Wizard, an error occurs because the `selenium-manager` folder is dynamically created in the `bin/debug` directory during the build process. Since this directory does not exist in the project source folder, Visual Studio fails to locate it during the export process, causing the export to fail.

## Workaround

To ensure the `selenium-manager` folder is included during the export process, follow these steps:

1. **Build the Project**: First, build the project to allow Selenium to create the `selenium-manager` folder in the `bin/debug` directory.

2. **Copy the Folder**: Manually copy the `selenium-manager` folder from the `bin/debug` directory to the root of the project directory.

3. **Prevent Inclusion in Build**: Ensure Visual Studio is set to never include the copied `selenium-manager` folder in the build. This is necessary to avoid any conflicts or redundancy during the build process.

## Steps

1. Build the project to generate the `selenium-manager` folder:
   ```
   <project-root>/bin/debug/selenium-manager
   ```

2. Copy the `selenium-manager` folder to the root of the project:
   ```
   <project-root>/selenium-manager
   ```

3. Configure Visual Studio to exclude the copied folder from the build process:
   - Right-click on the `selenium-manager` folder in the Solution Explorer.
   - Select "Properties".
   - Set the "Build Action" to "None" and "Copy to Output Directory" to "Do not copy".

This ensures that the `selenium-manager` folder is present for the template export but does not interfere with the build process.

## Note

This is a temporary workaround to facilitate the template export. The `selenium-manager` folder should only be included in the project root for the duration of the export process.