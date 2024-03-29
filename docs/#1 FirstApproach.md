# How to start?

### Structure of dialogs

First of all you need to understand structure of dialogs.
Every line of dialog is called node - that's just C# class. Node has a lot of attributes but the most important are:
- node type - who's is line of dialog (AI/player)
- title - thing that is shown when player is making decision and is shown in dialog editor
- text - just content of this line of code
- method and arguments - to call method after some line you need to provide name of the method and eventually the arguments for the method
- chances - if there is more than one AI node connected to single player node you need to specify how rare is each response
- dialog start type - if there are more than one dialog node that starts the dialog tree (more than one dialog tree), that option will appear and allow you to choose how start dialog node should be chosen (by random or by some condition)


### Prepare the scene

To prepare your scene for this tool you need to create/add game *tag* **"GameController"** to your DialogSystemInfo holder *(probably just your game controller will do the best job here)*.
Now just head to *window -> Dialog Visual Editor* in your Unity Editor. *(If is greyed-out please enter the gamemode)*. Now select your Dialog Actor from list of GameObjects on your scene and click *"Create new dialog actor" button*.


### Visual editor guide

To create your first line of dialog *(dialog node)* click on "Add Node button" and change all options to what you want.
Now if you have your lines of dialog *(dialog nodes)* please head to inspector - open it by clicking this symbol: ![Inspector](https://user-images.githubusercontent.com/20907620/184965952-2eded3c3-1536-4855-a125-5e4b90b6f380.png)
*[Photo outdated]*

Now you can modify your line of dialog *(dialog node)* in inspector *(be sure to select dialog node previous by clicking it)*.
If you hit right mouse button on dialog node you will be able to destroy selected node, make new link *(connection to other dialog node)* and destroy old link. You can exit this popup window by clicking *"Back button"* either clicking other node.
To **link** or destroy link to other node, just hit on the target node after clicking right button in popup.
