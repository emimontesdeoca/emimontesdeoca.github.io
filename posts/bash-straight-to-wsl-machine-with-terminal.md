Been loving WSL for development and testing since the day it came out, then the Terminal app dropped and I couldn't be happier, looks good, works perfectly and great performance.

It would be stupid to not use it with the WSL distro instead of using the console that gives you.

[![Image from Gyazo](https://i.gyazo.com/245fb4ff4fbd5297b2a8d9917dbee236.png)](https://gyazo.com/245fb4ff4fbd5297b2a8d9917dbee236)

This console is obviously good, but after you've used Terminal, there's no going back. It's just so much better, also *c o l o r s*.

Now if you fire up the terminal and open a new tab to load your WSL distro, it loads your mounted user folder.

[![Image from Gyazo](https://i.gyazo.com/dac1264f7b8dbae1f64e769b64c551da.png)](https://gyazo.com/dac1264f7b8dbae1f64e769b64c551da)

This is not really an issue, because if you just do `cd ~` it just loads the distro user folder. 

The thing is that I don't want to do it again and again so let's update a file to do it by itself.

## .bashrc

Fire up the terminal or the console, and go into your profile folder and then edit the `.bashrc` file using your favourite text editor.

Add `cd ~` at the end of the file before the last instruction.

[![Image from Gyazo](https://i.gyazo.com/b601aba59b9e877bc3926ab9ceb2b98c.png)](https://gyazo.com/b601aba59b9e877bc3926ab9ceb2b98c)

Save, reopen the WSL console with the terminal and you should be on the profiles folder.

Keep in mind that you'll have to do this for all your WSL installations, since we are updating the `bashrc` file for a single one.