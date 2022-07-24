# How to start?

First of all you need to understand structure of dialogs.
Every line of dialog is called node - that's just C# class. Node has a lot of attributes but the most important are:
- node type - who's is line of dialog (AI/player)
- title - thing that is shown when player is making decision and is shown in dialog editor
- text - just content of this line of code
- method and arguments - to call method after some line you need to provide name of the method and eventually the arguments for the method
- chances - if there is more than one AI node connected to single player node you need to specify how rare is each response
