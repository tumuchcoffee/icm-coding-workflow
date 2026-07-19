# Code Initialization

This feature is intended to produce the code projects and containers.

## Angular

Create the project for the angular code using the latest LTS version as of today's date 2026-07-18. The layout will use a component for each part of the screen:

* Header: the header component will have, from left to right, a hamburger icon that drops down the main menu, the application title as an H1 tag with the text "FAST Dashboard". Each of these is floated to the left. Floating to the far right is a user profile icon that links, for now, to the root url. It will get a new link once the user profile code is written. The header component will be "sticky" and not scroll with the page.
* Drop down menu: the main menu will expand out from the left of the screen in a panel. Each link will be an H3 tag in size.
* Optional right hand detail pane: when needed, a right hand panel will be available to epand out a default width of 150px.
* Footer: the default content in the footer will be a centered H3 tag with the text "FAST Dashboard" that will be sticky and not scroll with the page.
* Content area: the main area where components appear.

This version will not have authentication. The angular code will use the matching version of PrimeNG components as the default choice for controls.

## .NET API

Create the .net project that will be the backend api to the angular frontend. Create a single Health Check controller for this version.

## Database

Create a SQL Server database with scripts to create and run the database on a local development machine. In the \source\03-sql folder. create folder called script to hold a scripted copy of all database objects to make the database schema available to AI models.

## Testing

Create a postman collection in \source\04-testing\postman that calls the api code.
