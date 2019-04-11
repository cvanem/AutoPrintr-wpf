# AutoPrintr

![logo](http://i.imgur.com/v7D7COV.png "AutoPrintr Logo")

Automatically print to the desired printer (instantly) - by http://www.repairshopr.com/

## Installation

Head to our [Help Page](http://feedback.repairshopr.com/knowledgebase/articles/944752-cloudprint-issues-meet-autoprintr) on AutoPrintr if you are an end user


## Installer Build Instructions
1.  Update the pusher application key in AppSettings.cs prior to building.  This key has been intentionally removed from the source repository for security reasons.
2.  Update the assembly verion in AssembyVersion.cs.  Verify version is correclty displayed in the About window, which can opened by right clicking the AutoPrintr system tray menu icon.
2.  In visual studio, build the AutoPrintr project for release.  Files should be built to AutoPrintr\bin\Release
3.  Install Smart Install Maker if not already installed.  The demo version can be used.
4.  In the Installer Directory, doublick click or open the AutoPrintr.smm file in the Smart Install Maker application
5.  Update version to match version in step 2.
6.  Under the files tab, remove all old and unused files and add the newly built files in the AutoPrintr\bin\Release directory.

## Open Source

MIT License.
