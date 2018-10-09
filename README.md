# Form2RoleBot

## About
Form2Role bot is a Discord bot that takes data from Google Sheets in order to assign roles to users. Created using Discord.NET. You can get the latest version at: https://github.com/talios0/Form2RoleBot/releases

## Required Fields

### Discord ID
As of now, it is required to have the **Discord Username with Discriminator (ie. User#1234)**, although you can also use their nickname with discriminator or just the discriminator (just the numbers). However, using their nickname or just the number isn't encouraged since the number isn't unique and it is common for nicknames to change. No matter what method you decide to use, it is best practice to allow previous submissions to be changed in case the user change theirs identification (username/nickname). 
**See JSON configuration for more information on how to configure the bot to find the field.**

*Note that a change to the username will also cause the discriminator to change*.

## Roles
Build v0.1.2 does not create roles if they aren't found and doesn't remove roles from the user. For now, this needs to be done manually for now. 

The bot does support multiple roles per data box (checkboxes in google forms) and use of the " + " which denotes two roles together.

ie. "Moderator + SuperUser" will grant the roles "Moderator" and "SuperUser".

## nicknames
Form2Role Bot allows for a nickname field which will change the user's nickname. 

**See JSON configuration for more information how to configure the bot to find this field**

## Commands
I would like to preface this to say the use of commands isn't neccessary. The bot will *automatically update all connected servers around every hour*. However, one command does exist which will force an update.

`!update`
Forces an update to all connected servers. This will bypass the standard sheet checker and is available to all users.

## JSON Configuration
Configuration files are located in the /Config/ folder. If there isn't a Config folder or any of the files are corrupt/missing, the bot will automatically create new ones.

Make sure **all information is in quotes except** for "RolesStartAfter", "RolesEndBefore", "DiscordIDField", and "NicknameField"

### Bot Configuration
Located in Config/config.json

#### Token
This is the Discord Bot token. 
- You can generate one by going to www.discordapp.com/developers.
- There, login to your Discord account and select *"Create an application"*.
- Give your application a name and a description. 
- Select *"Save Changes"* at the bottom right and then navigate to the *"Bot"* section on the left-hand side. 
- Once there, create a bot user by clicking on *"Add Bot"*. 
- After confirming, *reveal the token*. 
- Copy/Paste the token into the configuration file.

`"Token": "tokenhere",`


#### Prefix
The prefix is the character that comes before a command. By default, the prefix is '!', but can be changed.

`"Prefix": "!",`

#### UpdateDelay
The delay between checking for updates to the provided Google Sheet. This is a number specfied in minutes.

`"UpdateDelay": 3600`

### Google Configuration
Located in Config/googleConfig.json

#### API Key
The API Key is required by the bot to retrieve information from Google Sheets. To generate, follow the instructions below.
- Go to https://console.developers.google.com 
- Login and Agree to the Terms of Service
- Create a Project by pressing *"Create Project"* on the right-hand side
- Click *"Create"*, give the project a name and then *"Create"* again.
- Once the dashboard loads, click on *"ENABLE APIS AND SERVICES"* in blue text.
- Search for *"Sheets"* and click on *"Google Sheets API"* and then *"ENABLE"*
- Click on *"Credentials"* on the left side of the page.
- Select *"Create credentials"* and then *"API Key"*
- Copy/Paste the key in the box into the configuration file.
`"APIKey": "apikey",`

#### SpreadsheetID
Go to your Google Sheets document and make sure it is *accessible to anyone with the link*. Copy/Paste the ID into the configuration file. ie. Copy/Paste the bolded information: /spreadsheets/d/**spreadsheetID**

`"SpreadsheetID": "spreadsheetID",`

#### Range
This is the range of data to take from Google Sheets.

`"Range": "B2:M",`

#### RolesStartAfter
Specifies where the roles start in the sheet. For example, if the roles start at the second field in the provided range then use 
`"RolesStartAfter": 1,`

#### RolesEndBefore
Specifies how far from the end of the range the roles end. For example, if the last field isn't a role but the second to last field is, then use:

`RolesEndBefore: 1,`

#### DiscordIDField
Specifies where the Discord ID can be found in the data. (This will only work at the beginning or the end) By default, the configuration specifies "-1" which means it'll treat the first field as the Discord ID. Using a number like "15" means it'll take the 16th position in the range.

`"DiscordIDField": -1,`

#### Nickname Field
Specifies where the Nickname can be found in the data. By default, the configuration specifies "-1" which means that the bot will treat it as the last field in the provided range. In addition, "-2" can also be specified which should be used if there isn't a nickname field. Using a number like "15" means it'll take the 16th position in the range. (starts at 0)

`"NicknameField": -1`

## Bot Permissions
While on the *"Bot"* page (www.discordapp.com/developers) make sure to give the bot permission to *"Manage Nicknames"* if you want the bot change nicknames and *"Manage Roles"*.

## Oddities and what to do
If the bot doesn't do anything in the console when it should, press *enter* in the console.
This occurs because console write outs are paused when text is highlighted.

## Editing the Code
You'll find some comments in the code that may help you. (or not, but hey, can't say I didn't do anything xD). If you need help understanding some, you can always message me.

## Other
When will this be updated? - I don't know.

What will be in future updates? - Roles will be added if they aren't found, Roles will be removed from a user if they have a role they shouldn't.
